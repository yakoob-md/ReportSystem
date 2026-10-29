using System.Globalization;

namespace ReportSystem.Models
{
    /// <summary>
    /// Format-neutral representation of a report. Individual reports map
    /// their view models into this structure before exporting.
    /// </summary>
    public class ReportExportModel
    {
        public string ReportTitle { get; set; } = "Report";
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyEmail { get; set; }
        public string? CompanyPhone { get; set; }
        public string? Description { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }

        public Dictionary<string, string> Filters { get; set; } = new();
        public List<string> Columns { get; set; } = new();
        public List<List<object?>> Rows { get; set; } = new();

        public static string FormatValue(object? value)
        {
            if (value is null)
                return string.Empty;

            if (value is DateTime dateTime)
            {
                return dateTime.TimeOfDay == TimeSpan.Zero
                    ? dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : dateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }

            if (value is DateOnly dateOnly)
                return dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            if (value is IFormattable formattable)
                return formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;

            return value.ToString() ?? string.Empty;
        }
    }
}
