using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using EbbsSoft.ExtensionHelpers.BooleanHelpers;

namespace EbbsSoft.ExtensionHelpers.DateTimeHelpers
{
    public static class Utils
    {
        /// <summary>
        /// Add & Count Only Business Days.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="addDays"></param>
        /// <returns></returns>
        public static DateTime AddWorkingDays(this DateTime date, int addDays)
        {
            int temp = addDays < 0 ? -1 : 1;
            DateTime newDate = date;
            while (addDays != 0)
            {
                newDate = newDate.AddDays(temp)
                ;
                if (newDate.DayOfWeek != DayOfWeek.Saturday 
                                      && newDate.DayOfWeek 
                                      != DayOfWeek.Sunday 
                                      && !newDate.IsHoliday())
                {
                    addDays -= temp;
                }
            }
            return newDate;
        }

        /// <summary>
        /// Unix TimeStamp To DateTime
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public static DateTime UnixToDateTime(this double unixTimestamp)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);
            long unixTimeStampInTicks = (long) (unixTimestamp * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Local);
        }

        /// <summary>
        /// DateTime To Unix TimeStamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static Double DateTimeToUnix(this DateTime dateTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
            return (double) unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }

        /// <summary>
        /// Get DateTimeFromInternet
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="username">username required, Sign up for an account http://api.geonames.org</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromInternet(this DateTime dt, string username)
        {
            // Raw Data Place Holder.
            dynamic rawData = null;

            // Tuple For Corrdinates.
            (decimal lat, decimal lng) = EbbsSoft.ExtensionHelpers.DecimalsHelpers.Utils.GetCoordinates();

            Task task = Task.Run(async() =>
            {
                string url = $"http://api.geonames.org/timezoneJSON?lat={lat}&lng={lng}&username={username}";
                using (HttpClient httpClient = new HttpClient())
                using (HttpResponseMessage httpResponseMessage= await httpClient.GetAsync(url))
                using (HttpContent httpContent = httpResponseMessage.Content)
                {
                    // Attempt To Read Data.
                    rawData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await httpContent.ReadAsStringAsync());
                }
            });
            Task.WaitAny(task);

            // If Raw Data Is Null, Return MinValue For DateTime.
            return rawData == null ? DateTime.MinValue : (DateTime)Convert.ToDateTime(Convert.ToString(rawData["time"]));
        }
   
        /// <summary>
        /// Get Days In Month.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> AllDaysInMonth(this DateTime dateTime)
        {
            int days = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            for (int i = 1; i <= days; i++)
            {
                yield return new DateTime(dateTime.Year, dateTime.Month, i);
            }
        }  
    }
}