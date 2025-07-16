using CCnetWPF.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCnetWPF.Events
{
	[Serializable]
	internal class InvalidRespondException : Exception
	{
		public InvalidRespondException(CctalkMessage respond)
			: this(respond, "Invalid respond")
		{
		}

		public InvalidRespondException(CctalkMessage respond, string message)
			: base(message)
		{
			InvalidRespond = respond;
		}

		public CctalkMessage InvalidRespond { get; private set; }

	}
}
