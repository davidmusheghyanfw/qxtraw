using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCnetWPF.Events
{
    public class BillReceivedEventArgs : EventArgs
    {

        public enum BillRecievedStatus
        {
            Rejected,
            Accepted,
        }

        private BillRecievedStatus status;
        private decimal value;
        private string rejectedReason;

        public BillRecievedStatus Status { get => status; set => status = value; }
        public decimal Value { get => value; set => this.value = value; }
        public string RejectedReason { get => rejectedReason; set => rejectedReason = value; }

        public BillReceivedEventArgs(BillRecievedStatus status, decimal value, string rejectedReason)
        {
            this.Status = status;
            this.Value = value;
            this.RejectedReason = rejectedReason;
        }
    }

    public delegate void BillReceivedHandler(object sender, BillReceivedEventArgs e);
}
