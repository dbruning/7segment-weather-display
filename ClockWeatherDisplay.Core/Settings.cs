namespace ClockWeatherDisplay.Core
{
	public sealed class Settings
	{
		public string ForecastIoApiKey { get; set; }
		public string GoogleMapsApiKey { get; set; }

		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public Settings()
		{
			Latitude = -37.714858;
			Longitude = 175.996928;
			ForecastIoApiKey = "b33cab685e11a21b56afa485f21d414a";
			GoogleMapsApiKey = "AIzaSyBh1qyKBpGSvfjSa9p42vX8rbTFbPu80v4";
		}
	}
}
