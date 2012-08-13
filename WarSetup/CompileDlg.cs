using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace WarSetup
{
    public partial class CompileDlg : Form
    {
        private Thread _worker;
        private ManualResetEvent _event;

        private delegate void SetInfoDelegate(string text, int progress);
        

        public CompileDlg(Thread worker, int steps)
        {
            _event = new ManualResetEvent(false);
            _worker = worker;
            InitializeComponent();
            progressBar1.Maximum = steps;
            if (worker == null)
                CancelBtn.Enabled = false;
        }


        public void WaitForSafeAccess()
        {
            if (_worker != null)
                _event.WaitOne();
        }

        public void SetInfo(string text, int progress)
        {
            SetInfoDelegate si = new SetInfoDelegate(DoSetInfo);
            BeginInvoke(si, new object[] { text, progress });
        }

        public void OnFinish()
        {
            CancelBtn.Text = "Close";
            CancelBtn.Enabled = true;
            System.Media.SystemSounds.Asterisk.Play();
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Text = "Compilation finished";
        }

        private void DoSetInfo(string text, int progress)
        {
            progressBar1.Value = progress;
            Info.Text = text;

            if (progress == progressBar1.Maximum)
                OnFinish();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            if ((_worker != null) && _worker.IsAlive)
            {
                this.Text = "Aborting compilation. Please wait...";
                _worker.Abort();
                _worker.Join();
                OnFinish();
                this.Text = "Aborted";
            }
            else
                Close();
        }

        private void CompileDlg_Shown(object sender, EventArgs e)
        {
            _event.Set();
        }
    }
}