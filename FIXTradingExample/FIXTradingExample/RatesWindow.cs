using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FIXTradingExample
{
  public partial class RatesWindow : Form
  {
    #region Disable Close Button
    private const int CP_NOCLOSE_BUTTON = 0x200;
    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams myCp = base.CreateParams;
        myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
        return myCp;
      }
    }
    #endregion

    // delegate for the update function
    delegate void updateCallback(QuickFix44.MarketDataSnapshotFullRefresh snapshot);

    private Dictionary<QuickFix.Symbol, int> map = new Dictionary<QuickFix.Symbol, int>();

    /**
     * Constructor
     */
    public RatesWindow()
    {
      InitializeComponent();
      // create and format the columns of the DataGridView
      grdRates.Columns.Add("Instrument", "Instrument");
      grdRates.Columns.Add("Updated", "Updated");
      grdRates.Columns["Updated"].DefaultCellStyle.Format = "HH:mm:ss dd MMM yy";
      grdRates.Columns.Add("Bid", "Bid");
      grdRates.Columns["Bid"].DefaultCellStyle.Format = "N5";
      grdRates.Columns["Bid"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      grdRates.Columns.Add("Ask", "Ask");
      grdRates.Columns["Ask"].DefaultCellStyle.Format = "N5";
      grdRates.Columns["Ask"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      grdRates.Columns.Add("MinQuantity", "MinQuantity");
    }

    /**
     * Retrun an array of QuickFix.Symbol that are in the DataGridView
     */
    public QuickFix.Symbol[] symbols()
    {
      return map.Keys.ToArray<QuickFix.Symbol>();
    }

    /**
     * Return the MinQuantity value of the specified symbol, converted to a double
     */
    public double minQty(QuickFix.Symbol symbol)
    {
      return Convert.ToDouble(grdRates.Rows[map[symbol]].Cells["MinQuantity"].Value);
    }

    /**
     * Attempt to update the instrument display in the DataGridView, thread-safe
     */
    public void update(QuickFix44.MarketDataSnapshotFullRefresh snapshot)
    {
      if (grdRates.InvokeRequired)
      {
        updateCallback d = new updateCallback(update);
        this.Invoke(d, new object[] { snapshot });
      }
      else
      {
        QuickFix.Symbol instrument = snapshot.getSymbol();
        double minQty = 0D;
        // calculate the minimum quantity depending on the instrument type and XCMMinQuantity field
        try { minQty = snapshot.getDouble(9095) * (10000 * ((snapshot.getInt(9080) == 1) ? 1 : ((double)1/10000))); } // FXCMMinQuantity
        catch (Exception e) { }
        // if the currency is already in the DataGridView
        if (map.ContainsKey(instrument))
        {
          // update only the cells of the row that have changed
          DataGridViewRow row = grdRates.Rows[map[instrument]];
          row.Cells["Updated"].Value = getClose(snapshot);
          row.Cells["Bid"].Value = getPrice(snapshot, "0", Convert.ToDouble(row.Cells["Bid"].Value));
          row.Cells["Ask"].Value = getPrice(snapshot, "1", Convert.ToDouble(row.Cells["Ask"].Value));
          row.Cells["MinQuantity"].Value = minQty;
        }
        else // otherwise add it to the DataGridView
        {
          map.Add(instrument, map.Count);
          grdRates.Rows.Add(
            instrument,
            getClose(snapshot),
            getPrice(snapshot, "0", 0D),
            getPrice(snapshot, "1", 0D),
            minQty
          );
        }
        // force the interface to refresh
        Application.DoEvents();
      }
    }

    /**
     * Override the default behavior of the data grid and ignore RowIndex out of bounds errors
     */
    private void grdRates_DataError(object sender, DataGridViewDataErrorEventArgs e) { }

    /**
     * Return the requested rate, 0 = Bid, 1 = Ask
     */
    private double getPrice(QuickFix44.MarketDataSnapshotFullRefresh mds, String p, double previous)
    {
      double price = previous;
      try
      {
        // grab the market data entries from the snapshot
        QuickFix44.MarketDataSnapshotFullRefresh.NoMDEntries group = new QuickFix44.MarketDataSnapshotFullRefresh.NoMDEntries();
        // go through the entries
        for (uint i = 1; i <= mds.getNoMDEntries().getValue(); i++)
        {
          group = (QuickFix44.MarketDataSnapshotFullRefresh.NoMDEntries)mds.getGroup(i, group);
          //if the entry type is the price requested, set the price to the value of the entry price
          if (group.getMDEntryType().getValue().Equals(p[0])) price = group.getMDEntryPx().getValue();
        }
      }
      catch (Exception e) { } // ignore errors
      return price; // if not found, return the previous rate
    }

    /**
     * Return the update time for the snapshot, converted to the local time zone
     */
    private DateTime getClose(QuickFix44.MarketDataSnapshotFullRefresh mds)
    {
      DateTime close = new DateTime(0L);
      try
      {
        DateTime last = new DateTime(0L);
        QuickFix44.MarketDataSnapshotFullRefresh.NoMDEntries group = new QuickFix44.MarketDataSnapshotFullRefresh.NoMDEntries();
        for (uint i = 1; i < mds.getNoMDEntries().getValue(); i++)
        {
          group = (QuickFix44.MarketDataSnapshotFullRefresh.NoMDEntries)mds.getGroup(i, group);
          if (group.getMDEntryTime().getValue() != null)
          {
            last = new DateTime(group.getMDEntryDate().getValue().Ticks + group.getMDEntryTime().getValue().Ticks);
            close = ((close.Ticks > last.Ticks) ? close : last);
          }
        }
      }
      catch (Exception e) { }
      return TimeZoneInfo.ConvertTime(close, TimeZoneInfo.Utc, TimeZoneInfo.Local);
    }
  }
}
