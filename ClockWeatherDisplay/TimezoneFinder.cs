using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;

namespace ClockWeatherDisplay
{
	/// <summary>
	/// Single responsibility: hold the response from Google Maps API (time zones)
	/// https://developers.google.com/maps/documentation/timezone/intro#Responses
	/// </summary>
	public sealed class TimeZoneResponse
	{
		public int DstOffset { get; set; }
		public int RawOffset { get; set; }
		public string TimeZoneId { get; set; }
		public string TimeZoneName { get; set; }
		public string Status { get; set; }
	}
	public sealed class TimeZoneFinder
	{
		public int LookupCurrentOffsetSeconds(Settings settings) {
			// https://developers.google.com/maps/documentation/timezone/start
			using (var client = new RestClient(new Uri("https://maps.googleapis.com/maps/api/")))
			{
				var unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
				// https://maps.googleapis.com/maps/api/timezone/json?location=38.908133,-77.047119&timestamp=1458000000&key=
				var request = new RestRequest($"timezone/json?location={settings.Latitude},{settings.Longitude}&timestamp={unixTimestamp}&key={settings.GoogleMapsApiKey}", Method.GET);
				var apiResult = client.Execute<TimeZoneResponse>(request).Result;
				// Check for 200 OK response
				if (apiResult.StatusCode != HttpStatusCode.OK) {
					throw new Exception($"Status code {apiResult.StatusCode} from Google Maps API call: {apiResult.Content}");
				}
				// Check for a non-OK Status code
				if (apiResult.Data.Status != "OK") {
					throw new Exception($"Status {apiResult.Data.Status} from Google Maps API call");
				}
				// Return the total offset
				return apiResult.Data.RawOffset + apiResult.Data.DstOffset;
			}
		}
	}
}
