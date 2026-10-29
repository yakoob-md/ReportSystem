using System.Net;
using System.Text;
using ReportSystem.Models;

namespace ReportSystem.Services
{
    public class HtmlExportService
    {
        public byte[] GenerateHtml(ReportExportModel model)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html lang=\"en\"><head>");
            builder.AppendLine("<meta charset=\"utf-8\">");
            builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            builder.AppendLine($"<title>{Encode(model.ReportTitle)}</title>");
            builder.AppendLine("""
                <style>
                    body { font-family: Arial, sans-serif; margin: 30px; color: #202938; }
                    .letterhead { border-bottom: 2px solid #d9dee7; padding-bottom: 16px; margin-bottom: 24px; }
                    h1 { color: #17365d; margin-bottom: 8px; }
                    .muted { color: #596579; }
                    .metadata { background: #f4f6f9; padding: 12px 16px; margin: 18px 0; }
                    table { width: 100%; border-collapse: collapse; }
                    th, td { border: 1px solid #d9dee7; padding: 8px; text-align: left; vertical-align: top; }
                    th { background: #17365d; color: white; }
                    tr { page-break-inside: avoid; }
                    @media print {
                        body { margin: 10mm; }
                        thead { display: table-header-group; }
                        .metadata { break-inside: avoid; }
                    }
                </style>
                """);
            builder.AppendLine("</head><body>");

            builder.AppendLine("<header class=\"letterhead\">");
            AddOptional(builder, "h2", model.CompanyName);
            AddOptional(builder, "div", model.CompanyAddress, "muted");
            AddOptional(builder, "div", model.CompanyEmail, "muted");
            AddOptional(builder, "div", model.CompanyPhone, "muted");
            builder.AppendLine("</header>");

            builder.AppendLine($"<h1>{Encode(model.ReportTitle)}</h1>");
            AddOptional(builder, "p", model.Description, "muted");

            builder.AppendLine("<div class=\"metadata\">");
            builder.AppendLine($"<div><strong>Generated:</strong> {Encode(model.GeneratedAt.ToString("dd MMM yyyy HH:mm"))}</div>");
            builder.AppendLine($"<div><strong>Page:</strong> {model.PageNumber} | <strong>Page size:</strong> {model.PageSize} | <strong>Total matching records:</strong> {model.TotalCount}</div>");
            if (model.Filters.Count > 0)
            {
                builder.AppendLine("<hr><strong>Report filters</strong><ul>");
                foreach (var filter in model.Filters)
                    builder.AppendLine($"<li><strong>{Encode(filter.Key)}:</strong> {Encode(filter.Value)}</li>");
                builder.AppendLine("</ul>");
            }
            builder.AppendLine("</div>");

            builder.AppendLine("<table><thead><tr>");
            foreach (var column in model.Columns)
                builder.AppendLine($"<th>{Encode(column)}</th>");
            builder.AppendLine("</tr></thead><tbody>");

            if (model.Rows.Count == 0)
            {
                builder.AppendLine($"<tr><td colspan=\"{Math.Max(model.Columns.Count, 1)}\">No records match the selected filters.</td></tr>");
            }
            else
            {
                foreach (var row in model.Rows)
                {
                    builder.AppendLine("<tr>");
                    foreach (var cell in row)
                        builder.AppendLine($"<td>{Encode(ReportExportModel.FormatValue(cell))}</td>");
                    builder.AppendLine("</tr>");
                }
            }

            builder.AppendLine("</tbody></table></body></html>");
            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        private static void AddOptional(StringBuilder builder, string tag, string? value, string? cssClass = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var classAttribute = string.IsNullOrWhiteSpace(cssClass)
                ? string.Empty
                : $" class=\"{Encode(cssClass)}\"";

            builder.AppendLine($"<{tag}{classAttribute}>{Encode(value)}</{tag}>");
        }

        private static string Encode(string? value) =>
            WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
