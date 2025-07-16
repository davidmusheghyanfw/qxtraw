using System;

namespace CCnetWPF.Events
{
    public class BillPowerUpEventArgs : EventArgs
    {
        public enum BillPowerUpStatus
        {
            PoweredUp,
            PoweredDown,
        }

        private BillPowerUpStatus status;
        private int value;
        private string reason;

        public BillPowerUpStatus Status
        {
            get => status;
            set => status = value;
        }

        public string Reason
        {
            get => reason;
            set => reason = value;
        }

        public BillPowerUpEventArgs(BillPowerUpStatus status, string reason)
        {
            this.Status = status;
            this.Reason = reason;
        }
    }

    public delegate void BillPowerUpHandler(object sender, BillPowerUpEventArgs e);
}
