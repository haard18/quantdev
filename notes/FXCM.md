# Learning FXCM

### Config File

- This file consists of all the default values needed for establishing connection with an exchange or individual broker/market maker/trader
- Following are the sections in the Config File

1. [DEFAULT]

- These are the default values to be made for connections

  1. ConnectionType – Defines the type of connection to be made.

     - `initiator`: client starts the connection
     - `acceptor`: server listens for connection
  2. HeartBtInt – The heartbeat interval (in seconds) to keep the session alive. Must be less than or equal to 60.
  3. ReconnectInterval – Time in seconds after which the client retries the connection if disconnected.
  4. FileStorePath – Path to where FIX session state and sequence numbers are stored.

     - Ensures persistent session handling between disconnects.
  5. FileLogPath – Directory where FIX message logs (incoming/outgoing) and session events are written.
  6. StartDay / StartTime – Day and time FIX session is allowed to begin.

     - Example: `StartDay=Sunday` and `StartTime=00:00:00`
  7. EndDay / EndTime – Day and time FIX session is expected to end.

     - Setting it to `Saturday 00:00:00` allows 24/7 connection during the week.
  8. UseDataDictionary – Enables message validation using a FIX XML schema.

     - Set to `Y` to enforce structure and tag correctness.
  9. DataDictionary – Path to the XML file defining message types, tag names, and rules.

     - Example: `FIXFXCM10.xml` (FXCM-specific format)
  10. ValidateUserDefinedFields – If `Y`, any unknown/custom tags will cause validation failure.

      - Set to `N` in development or for brokers with non-standard fields.
  11. ValidateFieldsHaveValues – If `Y`, fields with no values will be rejected.
  12. ValidateFieldsOutOfOrder – If `Y`, tags must follow FIX field order.

      - Set to `N` for flexibility.
  13. ValidateUnorderedGroupFields – If `Y`, checks for proper ordering inside repeating groups.
  14. ResetOnDisconnect – If `Y`, resets session state and seq numbers on disconnect.
  15. ResetOnLogout – If `Y`, resets on receiving or sending a logout message.
  16. ResetOnLogon – If `Y`, resets when logging in again.
  17. ResetSeqNumFlag – Sends tag `141=Y` on Logon to request sequence reset from counterparty.
  18. SendResetSeqNumFlag – Same as above; ensures reset logic is explicitly requested.
  19. ContinueInitializationOnError – If `Y`, the FIX engine keeps running even if one session fails.
  20. PrintIncoming – Prints all incoming FIX messages to console/log for debugging.
  21. PrintOutgoing – Prints outgoing FIX messages for visibility.
  22. PrintEvents – Logs FIX session state changes like connect, disconnect, logon, logout.
  23. CheckLatency – If `Y`, rejects messages with timestamp gaps.

      - Set to `N` to disable for smoother dev/testing.
  24. TargetSubID – FXCM-specific value required in every message (tag 57).
  25. Username – Your assigned username (tag 553) for Logon message.
  26. Password – Password (tag 554). Often omitted from config and entered at runtime for security.

2. [SESSION]

- Each `[SESSION]` block represents one FIX session between your client and the broker (here, FXCM).
- It inherits values from `[DEFAULT]` unless explicitly overridden.

  1. SocketConnectHost – The IP or domain to connect to.

     - Example: `fixdemo2.fxcorporate.com` (FXCM demo server)
  2. SocketConnectPort – The port to connect to on the host.

     - Example: `80` for unencrypted HTTP port.
  3. BeginString – FIX protocol version.

     - `FIX.4.4` is used by FXCM.
  4. SenderCompID – Your client ID assigned by FXCM.

     - This will appear in `49=` in FIX messages (SenderCompID tag).
  5. TargetCompID – FXCM's identifier.

     - Sent in `56=` in FIX messages (TargetCompID tag).
  6. MDEntryType – FXCM-specific value. Not a standard QuickFIX field, but likely used internally to filter what kind of Market Data you're subscribing to (e.g. bids/offers).

---

### Additional Fields *Might* be Added Later (Good to Know)


| Field                     | Description                                                                             |
| --------------------------- | ----------------------------------------------------------------------------------------- |
| `SessionQualifier`        | Used to run multiple sessions with same IDs (rare).                                     |
| `TargetSubID`             | Required by FXCM (tag 57), sometimes moved from`[DEFAULT]` to `[SESSION]` if it varies. |
| `StartTime` / `EndTime`   | Override per session (if needed).                                                       |
| `TransportDataDictionary` | If the transport version differs from app-level FIX version.                            |
| `UseDataDictionary=Y`     | Often repeated inside`[SESSION]` for clarity.                                           |

---

###  Notes

- `SenderCompID`, `TargetCompID`, `BeginString` form the **SessionID** in QuickFIX.
- You can have multiple `[SESSION]` blocks in one config file for different brokers, environments (demo/live), or use cases (market data, order routing).
