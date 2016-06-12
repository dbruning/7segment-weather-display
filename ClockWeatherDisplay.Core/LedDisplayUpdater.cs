using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClockWeatherDisplay.Core;
using Glovebox.Graphics.Components;
using Glovebox.Graphics.Drivers;

namespace ClockWeatherDisplay
{
	public class LedDisplayUpdater : IDisplayUpdater
	{
		private MAX7219 _driver;
		private SevenSegmentDisplay _display;

		public LedDisplayUpdater()
		{
			_driver = new MAX7219(2);
			_display = new SevenSegmentDisplay(_driver);
		}

		public void UpdateDisplay(string line1, string line2)
		{
			_display.FrameClear();
			_display.FrameDraw();
			_display.SetBrightness(4);

			_display.DrawString(line1, 0);
			_display.DrawString(line2, 1);

			_display.FrameDraw();

		}
	}
}
