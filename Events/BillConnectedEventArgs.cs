using System;

namespace CCnetWPF.Events
{
    public class BillConnectedEventArgs : EventArgs
    {
        public enum BillConnectedStatus
        {
            Connected,
            Disconnected,
        }

        private BillConnectedStatus status;
        private int value;
        private string disconnectReason;

        public BillConnectedStatus Status
        {
            get => status;
            set => status = value;
        }

        public int Value
        {
            get => value;
            set => this.value = value;
        }

        public string DisconnectReason
        {
            get => disconnectReason;
            set => disconnectReason = value;
        }

        public BillConnectedEventArgs(BillConnectedStatus status, int value, string disconnectReason)
        {
            this.Status = status;
            this.Value = value;
            this.DisconnectReason = disconnectReason;
        }
    }

    public delegate void BillConnectedHandler(object sender, BillConnectedEventArgs e);
}
