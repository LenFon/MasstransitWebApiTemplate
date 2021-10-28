using Destructurama.Attributed;
using System;

namespace MasstransitWebApiTemplate
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// 123456789 results in "123***789"
        /// </summary>
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        public string Summary { get; set; }

        /// <summary>
        ///  123456789 results in "123_REMOVED_789"
        /// </summary>
        [LogMasked(Text = "_REMOVED_", ShowFirst = 3, ShowLast = 3)]
        public string Summary2 { get; set; }

        /// <summary>
        ///  123456789
        /// </summary>
        [NotLogged]
        public string Summary3 { get; set; }
    }
}
