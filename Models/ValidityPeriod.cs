using System.Text.RegularExpressions;

namespace StandardLicensingGenerator.Models;

// Parses a validity period like "45 days", "2 weeks", "1 month", "5 years"
// (units may be abbreviated; a bare number means days) and computes the
// resulting expiration date from a start date.
public static class ValidityPeriod
{
    private static readonly Regex Pattern = new(@"^(\d{1,4})\s*([a-zA-Z]*)$", RegexOptions.Compiled);

    public static bool TryCompute(string text, DateTime from, out DateTime expiration)
    {
        expiration = default;

        var match = Pattern.Match(text.Trim());
        if (!match.Success)
            return false;
        if (!int.TryParse(match.Groups[1].Value, out int amount) || amount <= 0)
            return false;

        try
        {
            switch (match.Groups[2].Value.ToLowerInvariant())
            {
                case "" or "d" or "day" or "days":
                    expiration = from.AddDays(amount);
                    return true;
                case "w" or "wk" or "wks" or "week" or "weeks":
                    expiration = from.AddDays(7 * amount);
                    return true;
                case "m" or "mo" or "mos" or "month" or "months":
                    expiration = from.AddMonths(amount);
                    return true;
                case "y" or "yr" or "yrs" or "year" or "years":
                    expiration = from.AddYears(amount);
                    return true;
                default:
                    return false;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            // e.g. "9999 years" — past DateTime.MaxValue
            return false;
        }
    }
}
