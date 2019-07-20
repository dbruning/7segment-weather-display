namespace ClockWeatherDisplay.Core
{
    public static class MathExtensions
    {
        public static float ToCelcius(this float fahrenheit)
        {
            return (fahrenheit - 32) / 1.8f;
        }
    }
}
