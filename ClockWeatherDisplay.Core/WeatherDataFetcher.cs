using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ForecastIOPortable;

namespace ClockWeatherDisplay.Core
{
	public class WeatherDataFetcher
	{
		private readonly Settings _settings;

		public WeatherDataFetcher(Settings settings)
		{
			_settings = settings;
		}

		public WeatherData FetchWeatherData()
		{
			var result = new WeatherData();
			var forecastClient = new ForecastApi(_settings.DarkSkyApiKey);

			// Forecast.io app gives you first thousand calls per day for free.
			// There are 86400 seconds in a day
			// So we can call once every say 8.64 seconds and we'll be OK
			// (although the call itself takes a few seconds)
			// ... so let's update once per 30 seconds.
			//				_weatherTimer = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30))
			//					.Subscribe(_ =>
			//					{
			var retrieved = forecastClient.GetWeatherDataAsync(_settings.Latitude, _settings.Longitude).Result;

			result.CurrentTempCelcius = retrieved.Currently.Temperature.ToCelcius();
			result.TodaysHighCelcius = retrieved.Daily.Days[0].MaxTemperature.ToCelcius();
			result.TodaysLowCelcius = retrieved.Daily.Days[0].MinTemperature.ToCelcius();
			result.TomorrowsHighCelcius = retrieved.Daily.Days[1].MaxTemperature.ToCelcius();
			result.TomorrowsLowCelcius = retrieved.Daily.Days[1].MinTemperature.ToCelcius();

			var next12Hours = retrieved.Hourly.Hours.Take(12).ToList();

			// Calculate probability of rain in the next 12 hours
			var probabilitiesOfRain = next12Hours.Select(h => h.PrecipitationProbability).ToList();
			result.ProbabilityOfRainNext12Hours = probabilitiesOfRain.Max();
//			var probabilitiesOfNoRain = probabilitiesOfRain.Select(r => 1 - r).ToList();
//			var pRain = probabilitiesOfRain.Aggregate("", (x, y) => x + "," + y);
//			var pNoRain = probabilitiesOfNoRain.Aggregate("", (x, y) => x + "," + y);
//			var totalProbabilityOfNoRain = probabilitiesOfNoRain.Aggregate(1f, (x, y) => x * y);
//			result.ProbabilityOfRainNext12Hours = 1 - totalProbabilityOfNoRain;

			// If there is rain, calculate the total mm's of rain over the next 12 hours
			result.PrecipitationMillimetresNext12Hours = next12Hours.Sum(h => h.PrecipitationIntensity * 25.4f);

			return result;
		}
	}
}
