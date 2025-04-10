import socket
import datetime
import re

SOH = '\x01'
sequence_num = 1  # Server's sequence number

def extract_seq_num(msg):
    match = re.search(b'34=(\d+)', msg)
    if match:
        return int(match.group(1))
    return None

def extract_cl_ord_id(msg):
    match = re.search(b'11=([^\x01]+)', msg)
    return match.group(1).decode() if match else "UNKNOWN"

def extract_symbol(msg):
    match = re.search(b'55=([^\x01]+)', msg)
    return match.group(1).decode() if match else "AAPL"

def extract_order_qty(msg):
    match = re.search(b'38=([^\x01]+)', msg)
    return match.group(1).decode() if match else "100"

def extract_side(msg):
    match = re.search(b'54=([^\x01]+)', msg)
    return match.group(1).decode() if match else "1"  # 1 = Buy

def make_fix_msg(fields):
    global sequence_num
    body = SOH.join([f"{tag}={val}" for tag, val in fields]) + SOH
    body_length = len(body)
    header = f"8=FIX.4.4{SOH}9={body_length}{SOH}"
    msg_without_checksum = header + body
    checksum = sum(bytearray(msg_without_checksum, 'ascii')) % 256
    full_msg = msg_without_checksum + f"10={checksum:03}{SOH}"
    
    # Don't increment sequence number here - it's already set in the fields
    return full_msg.encode('ascii')

def create_logon_ack(client_seq):
    global sequence_num
    # Use the next expected sequence number from client
    sequence_num = client_seq
    fields = [
        ('35', 'A'),
        ('34', str(sequence_num)),
        ('49', 'MOCK'),
        ('56', 'CLIENT'),
        ('52', datetime.datetime.utcnow().strftime('%Y%m%d-%H:%M:%S.%f')[:-3]),
        ('98', '0'),
        ('108', '30')
    ]
    sequence_num += 1  # Now increment after creating the message
    return make_fix_msg(fields)

def create_execution_report(cl_ord_id, symbol, qty, side):
    global sequence_num
    fields = [
        ('35', '8'),  # ExecutionReport
        ('34', str(sequence_num)),  # Use current sequence number
        ('49', 'MOCK'),
        ('56', 'CLIENT'),
        ('52', datetime.datetime.utcnow().strftime('%Y%m%d-%H:%M:%S.%f')[:-3]),
        ('37', 'ORDER123'),  # OrderID
        ('11', cl_ord_id),   # ClOrdID
        ('17', 'EXEC456'),   # ExecID
        ('20', '0'),         # ExecTransType (not required in FIX 4.4, but included)
        ('39', '0'),         # OrdStatus: New
        ('55', symbol),
        ('54', side),
        ('38', qty),         # OrderQty
        ('150', '0'),        # ExecType: New
        ('151', qty),        # LeavesQty
        ('14', '0'),         # CumQty
        ('60', datetime.datetime.utcnow().strftime('%Y%m%d-%H:%M:%S.%f')[:-3]),  # TransactTime
        ('32', qty),         # LastQty 
        ('31', '100.00'),    # LastPx
        ('6', '100.00')      # AvgPx
    ]
    sequence_num += 1  # Increment after creating the message
    return make_fix_msg(fields)

# --- Start TCP server ---
HOST = '127.0.0.1'
PORT = 9900

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen(1)
    print(f"Mock FIX server listening on {HOST}:{PORT}")
    conn, addr = s.accept()
    with conn:
        print('Connected by', addr)
        while True:
            data = conn.recv(1024)
            if not data:
                break

            print("\n💬 Received Raw FIX Msg:\n", data.decode(errors="ignore"))
            
            # Extract client's sequence number for tracking
            client_seq = extract_seq_num(data)
            print(f"Client sequence number: {client_seq}")

            if b"35=A" in data:  # Logon message
                # For logon response, use next sequence after client's
                fix_response = create_logon_ack(client_seq + 1 if client_seq else 2)
                print("✅ Sending FIX Logon ACK:\n", fix_response.decode(errors="ignore"))
                conn.sendall(fix_response)

            elif b"35=D" in data:  # New Order Single
                # Extract necessary order details
                cl_ord_id = extract_cl_ord_id(data)
                symbol = extract_symbol(data)
                qty = extract_order_qty(data)
                side = extract_side(data)

                # For application messages, use our current sequence
                fix_exec_report = create_execution_report(cl_ord_id, symbol, qty, side)
                print("📩 Sending Execution Report (Ack):\n", fix_exec_report.decode(errors="ignore"))
                conn.sendall(fix_exec_report)