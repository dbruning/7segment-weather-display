using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockWeatherDisplay.Core
{
	public class WeatherData
	{
		public float ProbabilityOfRainNext12Hours { get; set; }
		public float PrecipitationMillimetresNext12Hours { get; set; }
		public float CurrentTempCelcius { get; set; }

		public float TodaysHighCelcius { get; set; }

		public float TodaysLowCelcius { get; set; }
		public float TomorrowsHighCelcius { get; set; }
		public float TomorrowsLowCelcius { get; set; }
	}
}
