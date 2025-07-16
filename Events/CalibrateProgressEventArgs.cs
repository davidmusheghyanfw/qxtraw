using System;

namespace CCnetWPF.Events
{
    public class CalibrateProgressEventArgs : EventArgs
    {
        public int ProgressPercentage { get; set; }
        public string Message { get; set; }

        public CalibrateProgressEventArgs(int progressPercentage, string message)
        {
            ProgressPercentage = progressPercentage;
            Message = message;
        }
    }

    public delegate void CalibrateProgressHandler(object sender, CalibrateProgressEventArgs e);
}