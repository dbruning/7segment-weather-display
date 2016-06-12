using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockWeatherDisplay.Core
{
	public interface IDisplayUpdater
	{
		void UpdateDisplay(string line1, string line2);
	}
}
