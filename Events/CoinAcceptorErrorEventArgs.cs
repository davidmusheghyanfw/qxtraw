using CCnetWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCnetWPF.Events
{
	public class CoinAcceptorErrorEventArgs : EventArgs
	{
		public CoinAcceptorErrors Error { get; private set; }
		public string ErrorMessage { get; private set; }
		//public byte ErrorCode { get; private set; }

		public CoinAcceptorErrorEventArgs(CoinAcceptorErrors error, String errorMessage)
		{
			Error = error;
			ErrorMessage = errorMessage;
			//ErrorCode = errorCode;
		}
	}
}
