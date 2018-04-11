using System;

namespace HL7Populator
{
    public static class HL7Extensions
    {
        public static string GetDateTimeString(this DateTime date, bool includeTime)
        {
            if (date == DateTime.MinValue)
                return string.Empty;
            else if (includeTime)
                return date.ToString("yyyyMMddHHmmss");
            else
                return date.ToString("yyyyMMdd");
        }

        public static string GetDateTimeString(this DateTime? date, bool includeTime)
        {
            if (!date.HasValue || date == DateTime.MinValue)
                return string.Empty;
            else if (includeTime)
                return date.Value.ToString("yyyyMMddHHmmss");
            else
                return date.Value.ToString("yyyyMMdd");
        }
    }
}
