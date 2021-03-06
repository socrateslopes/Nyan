﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Message = Nyan.Core.Modules.Log.Message;

namespace Nyan.Tools.LogMonitor
{
    public partial class frmMain : Form
    {
        private static bool mustUpdate = false;

        private static readonly Dictionary<Message.EContentType, Color> colorDictionary =
            new Dictionary<Message.EContentType, Color>();

        private ListViewItem _lastItem = null;


        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PrepareColors();
            PrepareColumns();

        }

        private void PrepareColumns()
        {
            lstMain.Columns.Clear();
            lstMain.Columns.Add("Server", "Server", 52);
            lstMain.Columns.Add("Type", "Type", 102);
            lstMain.Columns.Add("App", "App", 132);
            lstMain.Columns.Add("TimeStamp", "TimeStamp", 132);
            lstMain.Columns.Add("Content", "Content", 800);

            lstMain.DoubleBuffered(true);

        }

        private static void PrepareColors()
        {
            colorDictionary.Add(Message.EContentType.Audit, Color.PaleGreen);
            colorDictionary.Add(Message.EContentType.Debug, Color.LightSalmon);
            colorDictionary.Add(Message.EContentType.Generic, Color.Gray);
            colorDictionary.Add(Message.EContentType.Warning, Color.Magenta);
            colorDictionary.Add(Message.EContentType.Exception, Color.Red);
            colorDictionary.Add(Message.EContentType.Maintenance, Color.DeepSkyBlue);
            colorDictionary.Add(Message.EContentType.StartupSequence, Color.Yellow);
            colorDictionary.Add(Message.EContentType.Info, Color.MediumSlateBlue);
            colorDictionary.Add(Message.EContentType.MoreInfo, Color.DarkSlateBlue);
            colorDictionary.Add(Message.EContentType.ShutdownSequence, Color.YellowGreen);
        }

        private delegate void AddItemCallback(object o);

        public void ProcessLogEntry(object oSource)
        {
            if (InvokeRequired)
            {
                AddItemCallback d = ProcessLogEntry;
                Invoke(d, new[] { oSource });
            }
            else
            {
                var oMessage = (Message)oSource;

                if (oMessage.Type == Message.EContentType.Generic && chkIgGen.Checked) return;

                if ((int)oMessage.Type < 10) oMessage.Type = (Message.EContentType)Enum.ToObject(typeof(Message.EContentType), (int)oMessage.Type * 10);

                try
                {
                    var a = new ListViewItem
                    {
                        Text = oMessage.TraceInfo.MachineName,
                        ForeColor = Color.White,
                        BackColor = colorDictionary[oMessage.Type],
                        UseItemStyleForSubItems = false
                    };

                    a.SubItems.Add(oMessage.Type.ToString());
                    a.SubItems.Add(oMessage.TraceInfo.BaseAssembly);
                    a.SubItems.Add(oMessage.CreationTime.ToString("yyyyMMdd:HHmmss.fff"));
                    a.SubItems.Add(oMessage.Content);

                    for (var i = 0; i < a.SubItems.Count; i++)
                    {
                        a.SubItems[i].ForeColor = colorDictionary[oMessage.Type];
                        a.SubItems[i].BackColor = Color.Black;
                    }

                    lstMain.Items.Add(a);

                    _lastItem = a;

                    mustUpdate = true;

                }
                catch (Exception e) { }
            }
        }

        private void lstMain_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tmrMaintenance_Tick(object sender, EventArgs e)
        {
            //if (!mustUpdate) return;

            try
            {
                mustUpdate = false;
                _lastItem.EnsureVisible();
            }
            catch { }

        }

        private void btnAdjustColumns_Click(object sender, EventArgs e)
        {
            lstMain.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void chkIgGen_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.lstMain.Items.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string s = "";
            foreach (ListViewItem o in lstMain.SelectedItems)
            {
                s += o.Text;

                foreach (ListViewItem.ListViewSubItem si in o.SubItems)
                {
                    s += "\t" + si.Text;
                }

                s += Environment.NewLine;
            }

            try
            {
                Clipboard.SetText(s);
            }
            catch { }
        }

        private void chkAutoUpd_CheckedChanged(object sender, EventArgs e)
        {
            tmrMaintenance.Enabled = chkAutoUpd.Checked;
        }
    }
}