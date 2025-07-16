using System;

namespace CCnetWPF.Events
{
    public class CalibrateFinishEventArgs : EventArgs
    {
        public enum CalibrateFinishStatus
        {
            Success,
            Failure,
        }

        public CalibrateFinishStatus Status { get; set; }
        public string Message { get; set; }

        public CalibrateFinishEventArgs(CalibrateFinishStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    public delegate void CalibrateFinishHandler(object sender, CalibrateFinishEventArgs e);
}