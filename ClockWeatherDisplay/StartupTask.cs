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
		StringBuilder panel1Display = new StringBuilder(40);
		StringBuilder panel2Display = new StringBuilder(40);
		private string _dailyHighForecastTempDisplay;
		private string _dailyLowForecastTempDisplay;
		private int? _currentTimeOffsetSeconds = null;
		private IDisposable _weatherTimer;
		private IDisposable _timeZoneTimer;
		private IDisposable _displayTimer;
		private string _precipitationDisplay;
		private float _probabilityOfRainNext12Hours;
		private double _precipitationMillimetresNext12Hours;
		private float _currentTempCelcius;
		private float _todaysHighCelcius;
		private float _todaysLowCelcius;
		private float _tomorrowsHighCelcius;
		private float _tomorrowsLowCelcius;

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
				_weatherTimer = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30))
//					.StartWith(-1)
					.Subscribe(_ =>
					{
						var result = forecastClient.GetWeatherDataAsync(settings.Latitude, settings.Longitude).Result;

						_currentTempCelcius = result.Currently.Temperature.ToCelcius();
						_todaysHighCelcius = result.Daily.Days[0].MaxTemperature.ToCelcius();
						_todaysLowCelcius = result.Daily.Days[0].MinTemperature.ToCelcius();
						_tomorrowsHighCelcius = result.Daily.Days[1].MaxTemperature.ToCelcius();
						_tomorrowsLowCelcius = result.Daily.Days[1].MinTemperature.ToCelcius();

						var next12Hours = result.Hourly.Hours.Take(12).ToList();

						// Calculate probability of rain in the next 12 hours
						var probabilitiesOfNoRain = next12Hours.Select(h => 1 - h.PrecipitationProbability);
						var totalProbabilityOfNoRain = probabilitiesOfNoRain.Aggregate(1f, (x, y) => x*y);
						_probabilityOfRainNext12Hours = 1 - totalProbabilityOfNoRain;

						// If there is rain, calculate the total mm's of rain over the next 12 hours
						_precipitationMillimetresNext12Hours = next12Hours.Sum(h => h.PrecipitationIntensity * 25.4);
					}
				);

				// Google Maps API app gives you 2,500 free requests per day
				// https://developers.google.com/maps/documentation/timezone/usage-limits
				// ... however this will only be a factor for us when daylight savings changes, so an hourly check should be fine
				_timeZoneTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromHours(1))
					.StartWith(-1)
					.Subscribe(_ =>
					{
						_currentTimeOffsetSeconds = timeZoneFinder.LookupCurrentOffsetSeconds(settings);
					}
				);

				// Update the display every 10 seconds
				_displayTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
					.StartWith(-1)
					.Subscribe(i =>
					{
						panel1Display.Clear();
						panel2Display.Clear();

						// Current time
						// -- first 2 digits are hours
						DateTime? currentLocalTime = (_currentTimeOffsetSeconds != null)
							? DateTime.UtcNow + TimeSpan.FromSeconds(_currentTimeOffsetSeconds.Value)
							: (DateTime?)null;
						var currentTimeHours = (currentLocalTime != null)
							? currentLocalTime.Value.ToString("HH")
							: "    ";
						panel1Display.Append(currentTimeHours);

						// - blinking dot between hours & minutes
						if (i%2 == 0) panel1Display.Append(".");

						// - third and founth digits are minutes
						var currentTimeMinutes = (currentLocalTime != null)
							? currentLocalTime.Value.ToString("mm")
							: "    ";
						panel1Display.Append(currentTimeMinutes);

						// - fifth and sixth digits are likelihood of precipitation in next 12 hours
						panel1Display.Append((_probabilityOfRainNext12Hours*100).ToString("00"));

						// - seventh and eigth digits are mm rain (if any) in next 12 hours
						panel1Display.Append((_precipitationMillimetresNext12Hours*100).ToString("00"));

						// Panel 2 is temp
						panel2Display.Append(_currentTempCelcius.ToString("00"));
						panel2Display.Append(_todaysHighCelcius.ToString("00"));
						panel2Display.Append(_tomorrowsLowCelcius.ToString("00"));
						panel2Display.Append(_tomorrowsHighCelcius.ToString("00"));

						ssd.DrawString(panel1Display.ToString(), 0);
						ssd.DrawString(panel2Display.ToString(), 1);

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
