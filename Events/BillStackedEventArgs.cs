using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCnetWPF.Events
{
    public class BillStackedEventArgs
    {

        private bool cancel;

        public bool Cancel { get => cancel; set => cancel = value; }

        private int cashCode;
        public int Cashcode { get => cashCode; set => cashCode = value; }

        public BillStackedEventArgs(int cashCode)
        {
            this.cashCode = cashCode;
        }
    }

    public delegate void BillStackingHandler(object sender, BillStackedEventArgs e);
}
