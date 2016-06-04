﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Glovebox.Graphics.Components;
using Glovebox.Graphics;
using Glovebox.Graphics.Drivers;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ClockWeatherDisplay
{
	public sealed class StartupTask : IBackgroundTask
	{
		BackgroundTaskDeferral _deferral;   // for a headless Windows 10 for IoT projects you need to hold a deferral to keep the app active in the background
		double temperature;
		bool blink = false;
		StringBuilder data = new StringBuilder(40);

		public void Run(IBackgroundTaskInstance taskInstance)
		{
			_deferral = taskInstance.GetDeferral();  // get the deferral handle

			int count = 0;

			MAX7219 driver = new MAX7219(2);
			SevenSegmentDisplay ssd = new SevenSegmentDisplay(driver);
//			BMP180 bmp = new BMP180(BMP180.Mode.HIGHRES);

			ssd.FrameClear();
			ssd.FrameDraw();
			ssd.SetBrightness(4);

			while (true)
			{
//				temperature = bmp.Temperature.DegreesCelsius;

				data.Clear();

				// is temperature less than 3 digits and there is a decimal part too then right pad to 5 places as decimal point does not take up a digit space on the display
				if (temperature < 100 && temperature != (int)temperature) { data.Append($"{Math.Round(temperature, 1)}C".PadRight(5)); }
				else { data.Append($"{Math.Round(temperature, 0)}C".PadRight(4)); }

//				data.Append(Math.Round(bmp.Pressure.Hectopascals, 0));
				data.Append(123);

				if (blink = !blink) { data.Append("."); }  // add a blinking dot on bottom right as an I'm alive indicator

				ssd.DrawString(data.ToString());

				ssd.DrawString(count++, 1);

				ssd.FrameDraw();

				Task.Delay(2000).Wait();
			}
		}
	}
}
