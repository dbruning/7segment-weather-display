using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Reactive.Linq;
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
		private string _currentTempToDisplay;
		private string _dailyHighForecastTempDisplay;
		private string _dailyLowForecastTempDisplay;
		private int? _currentTimeOffsetSeconds = null;

		public void Run(IBackgroundTaskInstance taskInstance)
		{
			_deferral = taskInstance.GetDeferral();  // get the deferral handle

			var driver = new MAX7219(2);
			var ssd = new SevenSegmentDisplay(driver);
			var timeZoneFinder = new TimeZoneFinder();

			// I haven't committed my Settings class to the repo because it contains my latitude, longitude and forecast.io API key.
			// If you're trying to make this work but get errors here, just make up a Settings class with properties Latitude, Longitude, ApiKey
			var settings = new Settings();

			ssd.FrameClear();
			ssd.FrameDraw();
			ssd.SetBrightness(4);

			try
			{
				var forecastClient = new ForecastApi(settings.ForecastIoApiKey);

				// Forecast.io app gives you first thousand calls per day for free.
				// There are 86400 seconds in a day
				// So we can call once every say 8.64 seconds and we'll be OK
				// (although the call itself takes a few seconds)
				// ... so let's update once per 30 seconds.
				Observable.Timer(TimeSpan.FromSeconds(30))
					.StartWith(-1)
					.Subscribe(_ =>
					{
						var result = forecastClient.GetWeatherDataAsync(settings.Latitude, settings.Longitude).Result;

						_currentTempToDisplay = result.Currently.Temperature.ToCelcius().ToString("00");
						_dailyHighForecastTempDisplay = result.Daily.Days[0].MaxTemperature.ToCelcius().ToString("00");
						_dailyLowForecastTempDisplay = result.Daily.Days[1].MinTemperature.ToCelcius().ToString("00");
					}
				);

				// Google Maps API app gives you 2,500 free requests per day
				// https://developers.google.com/maps/documentation/timezone/usage-limits
				// ... however this will only be a factor for us when daylight savings changes, so an hourly check should be fine
				Observable.Timer(TimeSpan.FromHours(30))
					.StartWith(-1)
					.Subscribe(_ =>
					{
						_currentTimeOffsetSeconds = timeZoneFinder.LookupCurrentOffsetSeconds(settings);
					}
				);

				// Update the display every 10 seconds
				Observable.Timer(TimeSpan.FromSeconds(10))
					.StartWith(-1)
					.Subscribe(_ =>
					{
						// Current time
						DateTime? currentLocalTime = (_currentTimeOffsetSeconds != null)
							? DateTime.UtcNow + TimeSpan.FromSeconds(_currentTimeOffsetSeconds.Value)
							: (DateTime?)null;
						var currentTimeDigits = (currentLocalTime != null)
							? currentLocalTime.Value.ToString("HHmm")
							: "    ";
						

						data.Clear();
						data.Append(currentTimeDigits);
						data.Append(_dailyLowForecastTempDisplay);
						data.Append(_dailyHighForecastTempDisplay);
						//				if (blink = !blink) { data.Append("."); }  // add a blinking dot on bottom right as an I'm alive indicator

						ssd.DrawString(data.ToString());

//						ssd.DrawString(count++, 1);

						ssd.FrameDraw();
					}
				);
			}
			catch (Exception e)
			{
				// I dunno, log it or something?
			}

		}
	}
}
