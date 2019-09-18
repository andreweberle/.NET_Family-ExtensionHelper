using System;

namespace EbbsSoft.ExtensionHelpers.DoubleHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Convert From Celsius To Fahrenheit
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double ToFahrenheit(this double c) => ((9.0 / 5.0) * c) + 32;

        /// <summary>
        /// Convert From Celsius To Fahrenheit
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double ToFahrenheit(this int c) => ((9.0 / 5.0) * Convert.ToDouble(c)) + 32;

        /// <summary>
        /// Convert From Fahrenheit To Celsius
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static double ToCelsius(this double f) => (5.0 / 9.0) * (f - 32);

        /// <summary>
        /// Convert From Fahrenheit To Celsius
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static double ToCelsius(this int f) => (5.0 / 9.0) * (Convert.ToDouble(f) - 32);
    }
}