using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockWeatherDisplay.Core
{
	/// <summary>
	/// Single responsibility: be the "game loop", the entry point that fires off and coordinates the timers
	/// </summary>
	public class MainController
	{
		private string _line1;
		private string _line2;
		private int? _currentTimeOffsetSeconds = null;
		private IDisposable _weatherTimer;
		private IDisposable _timeZoneTimer;
		private IDisposable _displayTimer;
		private WeatherData _weatherData;
		private IDisplayUpdater _displayUpdater;
		private DisplayStringBuilder _displayStringBuilder;
		private TimeZoneFinder _timeZoneFinder;

		public void Run(IDisplayUpdater displayUpdater)
		{
			_displayUpdater = displayUpdater;
			_timeZoneFinder = new TimeZoneFinder();
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
						try
						{
							_weatherData = weatherDataFetcher.FetchWeatherData();
						}
						catch (Exception weatherException)
						{
						}
					})
				;

				// Google Maps API app gives you 2,500 free requests per day
				// https://developers.google.com/maps/documentation/timezone/usage-limits
				// ... however this will only be a factor for us when daylight savings changes, so an hourly check should be fine
				_timeZoneTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromHours(1))
					.Subscribe(_ =>
					{
						try
						{
							_currentTimeOffsetSeconds = _timeZoneFinder.LookupCurrentOffsetSeconds(settings);
						}
						catch (Exception timeZoneException)
						{
						}
					})
				;

				// Update the display
				// (On a separate scheduler, e.g. thread, so that we always put out a new  display string, even when other queries are happening)
				_displayTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1), Scheduler.Default)
					.Subscribe(i =>
					{
						try
						{
							DateTime? currentLocalTime = (_currentTimeOffsetSeconds != null)
								? DateTime.UtcNow + TimeSpan.FromSeconds(_currentTimeOffsetSeconds.Value)
								: (DateTime?)null;

							_displayStringBuilder.BuildDisplayStrings(_weatherData, currentLocalTime, ref _line1, ref _line2);
							_displayUpdater.UpdateDisplay(_line1, _line2);
						} 
						catch (Exception displayException)
						{
						}
					})
				;
			}
			catch (Exception e)
			{
				// I dunno, log it or something?
			}

		}
	}
}
