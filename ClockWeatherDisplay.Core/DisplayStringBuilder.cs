using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockWeatherDisplay.Core
{
	public class DisplayStringBuilder
	{
		public void BuildDisplayStrings(WeatherData weatherData, DateTime? localDateTime, ref string line1, ref string line2)
		{
			line1 = "";
			line2 = "";

			// Current time
			// -- first 2 digits are hours
			var currentTimeHours = (localDateTime != null)
				? localDateTime.Value.ToString("HH")
				: "    ";
			line1 += currentTimeHours;

			// - blinking dot between hours & minutes
			if (localDateTime?.Second%2 == 0) line1 += ".";

			// - third and founth digits are minutes
			var currentTimeMinutes = (localDateTime != null)
				? localDateTime.Value.ToString("mm")
				: "    ";
			line1 += currentTimeMinutes;

			// - fifth and sixth digits are likelihood of precipitation in next 12 hours
			if (weatherData != null) {
				line2 += Math.Max(99f, weatherData.ProbabilityOfRainNext12Hours * 100).ToString("00");
			} else {
				line2 += "  ";
			}

			// - seventh and eigth digits are mm rain (if any) in next 12 hours
			if (weatherData != null) {
				line2 += Math.Max(99f, weatherData.PrecipitationMillimetresNext12Hours * 100).ToString("00");
			} else {
				line2 += "  ";
			}

			// Panel 2 is temp
			if (weatherData != null)
			{
				line2 += weatherData.CurrentTempCelcius.ToString("00");
				line2 += weatherData.TodaysHighCelcius.ToString("00");
				line2 += weatherData.TomorrowsLowCelcius.ToString("00");
				line2 += weatherData.TomorrowsHighCelcius.ToString("00");
			} else
			{
				line2 += "        ";
			}

		}
	}
}
