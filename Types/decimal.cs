using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EbbsSoft.ExtensionHelpers.DecimalsHelpers
{
    public static class Utils
    {
                /// <summary>
        /// Get Current Coordinates
        /// </summary>
        /// <returns></returns>
        public static (decimal lat, decimal lng) GetCoordinates()
        {
            dynamic rawData = null;

            Task task = Task.Run(async() =>
            {
                string url = "http://ip-api.com/json/";
                using (HttpClient httpClient= new HttpClient())
                using (HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url))
                using (HttpContent httpContent = httpResponseMessage.Content)
                {
                    rawData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await httpContent.ReadAsStringAsync());
                }
            });

            Task.WaitAll(task);
            return (Convert.ToDecimal(rawData["lat"]), Convert.ToDecimal(rawData["lon"]));
        }
    }
}