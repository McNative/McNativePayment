using System;

namespace McNativePayment.Utils
{
    public static class Extensions
    {

        public static long ToUnixTime(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

    }
}
