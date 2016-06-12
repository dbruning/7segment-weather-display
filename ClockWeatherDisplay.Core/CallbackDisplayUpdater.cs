using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockWeatherDisplay.Core
{
	public class CallbackDisplayUpdater: IDisplayUpdater
	{
		private readonly Action<string, string> _callback;

		public CallbackDisplayUpdater(Action<string, string> callback)
		{
			_callback = callback;
		}

		public void UpdateDisplay(string line1, string line2)
		{
			_callback(line1, line2);
		}
	}
}
