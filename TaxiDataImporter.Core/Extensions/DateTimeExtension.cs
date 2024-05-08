namespace TaxiDataImporter.Core.Extensions;

public static class DateTimeExtension
{
    public static DateTime? ConvertToUtc(this DateTime? dateTime)
    {
        if (dateTime.HasValue)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime.Value, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        }
        return null;
    }
}