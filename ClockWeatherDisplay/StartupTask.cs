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

			// I haven't committed my Settings class to the repo because it contains my latitude, longitude and forecast.io API key.
			// If you're trying to make this work but get errors here, just make up a Settings class with properties Latitude, Longitude, ApiKey

			var mainController = new MainController();
			mainController.Run(new LedDisplayUpdater());
		}
	}
}
