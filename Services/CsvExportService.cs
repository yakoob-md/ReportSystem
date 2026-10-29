using System.Globalization;
using System.Text;
using ReportSystem.Models;

namespace ReportSystem.Services
{
    public class CsvExportService
    {
        public byte[] GenerateCsv(ReportExportModel model)
        {
            var builder = new StringBuilder();

            builder.AppendLine(Csv(model.ReportTitle));
            builder.AppendLine(Csv("Generated") + "," + Csv(model.GeneratedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
            builder.AppendLine(Csv("Page") + "," + Csv(model.PageNumber.ToString(CultureInfo.InvariantCulture)));
            builder.AppendLine(Csv("Page size") + "," + Csv(model.PageSize.ToString(CultureInfo.InvariantCulture)));
            builder.AppendLine(Csv("Total matching records") + "," + Csv(model.TotalCount.ToString(CultureInfo.InvariantCulture)));

            AddOptionalDetail(builder, "Company", model.CompanyName);
            AddOptionalDetail(builder, "Address", model.CompanyAddress);
            AddOptionalDetail(builder, "Email", model.CompanyEmail);
            AddOptionalDetail(builder, "Phone", model.CompanyPhone);
            AddOptionalDetail(builder, "Description", model.Description);

            foreach (var filter in model.Filters)
                builder.AppendLine(Csv(filter.Key) + "," + Csv(filter.Value));

            builder.AppendLine();
            builder.AppendLine(string.Join(",", model.Columns.Select(Csv)));

            foreach (var row in model.Rows)
                builder.AppendLine(string.Join(",", row.Select(CsvCell)));

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
        }

        private static void AddOptionalDetail(StringBuilder builder, string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                builder.AppendLine(Csv(label) + "," + Csv(value));
        }

        private static string CsvCell(object? value)
        {
            if (value is null)
                return Csv(string.Empty);

            // Numeric values are data, not formulas. All other values are treated as text.
            if (value is decimal or int or long or double or float)
                return Csv(ReportExportModel.FormatValue(value));

            return Csv(ReportExportModel.FormatValue(value));
        }

        private static string Csv(string? value)
        {
            value ??= string.Empty;

            // Reduce spreadsheet formula-injection risk for text.
            if (value.Length > 0 &&
                value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            {
                value = "'" + value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
