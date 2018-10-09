using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Helper {
    public class TimeHelper {

        public static System.DateTime ConvertIntDateTime(double d)
         {
             System.DateTime time = System.DateTime.MinValue;
             System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
             time = startTime.AddSeconds(d);
             return time;
         }

        public static long ConvertDateTimeInt(System.DateTime time)
         {
             long intResult = 0;
             System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
             intResult = (long)(time - startTime).TotalSeconds;
             return intResult;
         }

        public static long CurrentUnixTime() {
            return ConvertDateTimeInt(System.DateTime.Now);
        }

}
}
