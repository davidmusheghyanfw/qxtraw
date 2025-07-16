using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCnetWPF.Events
{
	[Serializable]
	internal class InvalidRespondFormatException : Exception
	{
		public InvalidRespondFormatException(Byte[] respondRawData) : this(respondRawData, "Invalid respond")
		{
		}

		public InvalidRespondFormatException(byte[] respondRawData, string message) : base(message)
		{
			InvalidRespondData = respondRawData;
		}

		public Byte[] InvalidRespondData { get; private set; }

	}
}
