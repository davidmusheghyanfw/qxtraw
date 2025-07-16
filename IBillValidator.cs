using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCnetWPF.Events;

namespace CCnetWPF.Adapter
{
    public interface IBillValidator
    {
        PortConnectedArgs Init(string portName);
        event Action<object, BillReceivedEventArgs> BillReceived;
        event Action<object, BillStackedEventArgs> BillStacked;
        event Action<object, BillConnectedEventArgs> OnConnectedEvent;
        event Action<object, BillPowerUpEventArgs> OnPowerUpEvent;

        bool IsConnected { get; set; }

        string EnableBill();
        string DisableBill();
        void CloseValidator();
        void ReturnBill();
        void EscrowStack();
        Tuple<bool, string> ToggleBarcode(bool state);

    }
}
