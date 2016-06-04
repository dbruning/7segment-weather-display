using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockWeatherDisplay
{
    public static class MathExtensions
    {
        public static float ToCelcius(this float fahrenheit)
        {
            return (fahrenheit - 32) / 1.8f;
        }
    }
}
