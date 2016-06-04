﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using ForecastIOPortable;
using Glovebox.Graphics.Components;
using Glovebox.Graphics;
using Glovebox.Graphics.Drivers;
using ClockWeatherDisplay;

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

			// I haven't committed my Settings class to the repo because it contains my latitude, longitude and forecast.io API key.
			// If you're trying to make this work but get errors here, just make up a Settings class with properties Latitude, Longitude, ApiKey
			var settings = new Settings();

			ssd.FrameClear();
			ssd.FrameDraw();
			ssd.SetBrightness(4);

			while (true)
			{
				try {
					var client = new ForecastApi(settings.ApiKey);
					var result = client.GetWeatherDataAsync(settings.Latitude, settings.Longitude).Result;

					var currentTempToDisplay = result.Currently.Temperature.ToCelcius().ToString("00");
					var dailyHighForecastTempDisplay= result.Daily.Days[0].MaxTemperature.ToCelcius().ToString("00");
					var dailyLowForecastTempDisplay = result.Daily.Days[0].MinTemperature.ToCelcius().ToString("00");

					data.Clear();
					data.Append(currentTempToDisplay);
					data.Append("  ");
					data.Append(dailyLowForecastTempDisplay);
					data.Append(dailyHighForecastTempDisplay);

	//				if (blink = !blink) { data.Append("."); }  // add a blinking dot on bottom right as an I'm alive indicator

					ssd.DrawString(data.ToString());

					ssd.DrawString(count++, 1);

					ssd.FrameDraw();
				} catch (Exception e) {
					// I dunno, log it or something?
				}

				Task.Delay(2000).Wait();
			}
		}
	}
}
