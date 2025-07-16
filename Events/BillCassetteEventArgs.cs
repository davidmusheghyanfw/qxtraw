using CCnetWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCnetWPF.Events
{
    public class BillCassetteEventArgs
    {
        
        public static CasetteStatus BillCassetteStatus { get; set; }
        public CasetteStatus Status { get; set; }

        public BillCassetteEventArgs(CasetteStatus status)
        {
            this.Status = status;
        }
    }

    public delegate void BillCassetteHandler(object sender, BillCassetteEventArgs e);
}
