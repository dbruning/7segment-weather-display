﻿using System;
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
using ClockWeatherDisplay.Core;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ClockWeatherDisplay
{
	public sealed class StartupTask : IBackgroundTask
	{
		BackgroundTaskDeferral _deferral;   // for a headless Windows 10 for IoT projects you need to hold a deferral to keep the app active in the background
		double temperature;
		bool blink = false;
		private string _line1;
		private string _line2;
		private int? _currentTimeOffsetSeconds = null;
		private IDisposable _weatherTimer;
		private IDisposable _timeZoneTimer;
		private IDisposable _displayTimer;
		private WeatherData _weatherData;
		private LedDisplayUpdater _displayUpdater;
		private DisplayStringBuilder _displayStringBuilder;

		public void Run(IBackgroundTaskInstance taskInstance)
		{
			_deferral = taskInstance.GetDeferral();  // get the deferral handle

			var timeZoneFinder = new TimeZoneFinder();
			_displayStringBuilder = new DisplayStringBuilder();

			// I haven't committed my Settings class to the repo because it contains my latitude, longitude and forecast.io API key.
			// If you're trying to make this work but get errors here, just make up a Settings class with properties Latitude, Longitude, ApiKey
			var settings = new Settings();

			var weatherDataFetcher = new WeatherDataFetcher(settings);

			try
			{
				// Forecast.io app gives you first thousand calls per day for free.
				// There are 86400 seconds in a day
				// So we can call once every say 8.64 seconds and we'll be OK
				// (although the call itself takes a few seconds)
				// ... so let's update once per 30 seconds.
				_weatherTimer = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30))
					.Subscribe(_ =>
					{
						_weatherData = weatherDataFetcher.FetchWeatherData();
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
						DateTime? currentLocalTime = (_currentTimeOffsetSeconds != null)
							? DateTime.UtcNow + TimeSpan.FromSeconds(_currentTimeOffsetSeconds.Value)
							: (DateTime?)null;

						_displayStringBuilder.BuildDisplayStrings(_weatherData, currentLocalTime, ref _line1, ref _line2);
						_displayUpdater.UpdateDisplay(_line1, _line2);
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
