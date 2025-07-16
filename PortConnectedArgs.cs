using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCnetWPF.Events
{
    public class PortConnectedArgs
    {
        private PortConnectionEnum _code;
        private string _message;
        private string _portName;

        public PortConnectionEnum Code { get => _code; set => _code = value; }
        public string Message { get => _message; set => _message = value; }
        public string PortName { get => _portName; set => _portName = value; }


        public PortConnectedArgs(PortConnectionEnum code, string message, string portName)
        {
            this.Code = code;
            this.Message = message;
            this.PortName = portName;
        }
        
    }
    
    public enum PortConnectionEnum
    {
        SUCCESS = 0,
        ERROR = 1,
    }
}
