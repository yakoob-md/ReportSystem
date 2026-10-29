using System.Globalization;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ReportSystem.Models;

namespace ReportSystem.Services
{
    public class DocxExportService
    {
        public byte[] GenerateDocx(ReportExportModel model)
        {
            using var memoryStream = new MemoryStream();

            using (var document = WordprocessingDocument.Create(
                memoryStream, WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                var body = new Body();

                AddLetterhead(body, model);
                body.Append(CreateParagraph(model.ReportTitle, true, "30", "17365D"));

                if (!string.IsNullOrWhiteSpace(model.Description))
                    body.Append(CreateParagraph(model.Description));

                body.Append(CreateParagraph(
                    "Generated: " + model.GeneratedAt.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture)));

                body.Append(CreateParagraph(
                    $"Page {model.PageNumber} | Page size: {model.PageSize} | Total matching records: {model.TotalCount}"));

                if (model.Filters.Count > 0)
                {
                    body.Append(CreateParagraph("Report filters", true, "22", "17365D"));
                    foreach (var filter in model.Filters)
                        body.Append(CreateParagraph($"{filter.Key}: {filter.Value}"));
                }

                body.Append(CreateParagraph(""));

                var table = CreateReportTable(model);
                body.Append(table);

                // SectionProperties must be the final body child.
                body.Append(CreatePageSectionProperties());

                mainPart.Document = new Document(body);
                mainPart.Document.Save();
            }

            return memoryStream.ToArray();
        }

        private static void AddLetterhead(Body body, ReportExportModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.CompanyName))
                body.Append(CreateParagraph(model.CompanyName, true, "32", "17365D"));
            if (!string.IsNullOrWhiteSpace(model.CompanyAddress))
                body.Append(CreateParagraph(model.CompanyAddress));
            if (!string.IsNullOrWhiteSpace(model.CompanyEmail))
                body.Append(CreateParagraph(model.CompanyEmail));
            if (!string.IsNullOrWhiteSpace(model.CompanyPhone))
                body.Append(CreateParagraph(model.CompanyPhone));

            if (!string.IsNullOrWhiteSpace(model.CompanyName)
                || !string.IsNullOrWhiteSpace(model.CompanyAddress)
                || !string.IsNullOrWhiteSpace(model.CompanyEmail)
                || !string.IsNullOrWhiteSpace(model.CompanyPhone))
            {
                body.Append(CreateParagraph("____________________________________________________________"));
                body.Append(CreateParagraph(""));
            }
        }

        private static Table CreateReportTable(ReportExportModel model)
        {
            var table = new Table();
            var borders = new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4 },
                new BottomBorder { Val = BorderValues.Single, Size = 4 },
                new LeftBorder { Val = BorderValues.Single, Size = 4 },
                new RightBorder { Val = BorderValues.Single, Size = 4 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 });

            table.AppendChild(new TableProperties(borders));

            var headerRow = new TableRow();
            foreach (var header in model.Columns)
                headerRow.Append(CreateTableCell(header, true));
            table.Append(headerRow);

            foreach (var data in model.Rows)
            {
                var row = new TableRow();
                for (int i = 0; i < model.Columns.Count; i++)
                {
                    var value = i < data.Count ? data[i] : null;
                    row.Append(CreateTableCell(ReportExportModel.FormatValue(value)));
                }
                table.Append(row);
            }

            if (model.Rows.Count == 0)
            {
                var emptyRow = new TableRow();
                emptyRow.Append(CreateTableCell("No records match the selected filters."));
                table.Append(emptyRow);
            }

            return table;
        }

        private static TableCell CreateTableCell(string? text, bool isHeader = false)
        {
            var runProperties = new RunProperties(new FontSize { Val = "16" });
            if (isHeader)
            {
                runProperties.Append(new Bold());
                runProperties.Append(new Color { Val = "FFFFFF" });
            }

            var paragraph = new Paragraph(new Run(
                runProperties,
                new Text(text ?? string.Empty)
                {
                    Space = SpaceProcessingModeValues.Preserve
                }));

            var properties = new TableCellProperties();

            if (isHeader)
            {
                properties.Append(new Shading
                {
                    Val = ShadingPatternValues.Clear,
                    Fill = "17365D"
                });
            }

            properties.Append(new TableCellVerticalAlignment
            {
                Val = TableVerticalAlignmentValues.Center
            });

            return new TableCell(properties, paragraph);
        }

        private static Paragraph CreateParagraph(
            string? text,
            bool bold = false,
            string fontSize = "20",
            string? color = null)
        {
            var runProperties = new RunProperties(new FontSize { Val = fontSize });
            if (bold)
                runProperties.Append(new Bold());
            if (!string.IsNullOrWhiteSpace(color))
                runProperties.Append(new Color { Val = color });

            return new Paragraph(new Run(
                runProperties,
                new Text(text ?? string.Empty)
                {
                    Space = SpaceProcessingModeValues.Preserve
                }));
        }

        private static SectionProperties CreatePageSectionProperties()
        {
            return new SectionProperties(
                new PageSize
                {
                    Width = 15840U,
                    Height = 12240U,
                    Orient = PageOrientationValues.Landscape
                },
                new PageMargin
                {
                    Top = 720,
                    Right = 720U,
                    Bottom = 720,
                    Left = 720U,
                    Header = 360U,
                    Footer = 360U,
                    Gutter = 0U
                });
        }
    }
}
