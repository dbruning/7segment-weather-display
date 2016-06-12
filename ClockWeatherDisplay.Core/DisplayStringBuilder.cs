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
			if (localDateTime?.Second % 2 == 0) line1 += ".";

			// - third and founth digits are minutes
			var currentTimeMinutes = (localDateTime != null)
				? localDateTime.Value.ToString("mm")
				: "    ";
			line1 += currentTimeMinutes;

			// - fifth and sixth digits are likelihood of precipitation in next 12 hours
			if (weatherData != null)
			{
				line1 += Math.Min(99f, weatherData.ProbabilityOfRainNext12Hours * 100).ToString("00");
			}
			else
			{
				line1 += "  ";
			}

			// - seventh and eigth digits are mm rain (if any) in next 12 hours
			if (weatherData != null)
			{
				if (weatherData.PrecipitationMillimetresNext12Hours >= 10f)
				{
					// If more than 10mm, just show the first 2 digits e.g 15 for 15mm
					line1 += Math.Min(99f, weatherData.PrecipitationMillimetresNext12Hours).ToString("00");
				}
				else
				{
					// Less than 10 - show 1d.p. e.g. 0.2
					line1 += Math.Min(99f, weatherData.PrecipitationMillimetresNext12Hours).ToString("0.0");
				}
			}
			else
			{
				line1 += "  ";
			}

			// Panel 2 is temp
			if (weatherData != null)
			{
				line2 += weatherData.CurrentTempCelcius.ToString("00");
				line2 += weatherData.TodaysHighCelcius.ToString("00");
				line2 += weatherData.TomorrowsLowCelcius.ToString("00");
				line2 += weatherData.TomorrowsHighCelcius.ToString("00");
			}
			else
			{
				line2 += "        ";
			}

		}
	}
}
