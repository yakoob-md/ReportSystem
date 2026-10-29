
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ReportSystem.Models;

namespace ReportSystem.Services
{
	public class ExcelExportService
	{
		public byte[] GenerateExcel(ReportExportModel model)
		{
			ArgumentNullException.ThrowIfNull(model);

			using var stream = new MemoryStream();

			using (var document = SpreadsheetDocument.Create(
				stream,
				SpreadsheetDocumentType.Workbook))
			{
				var workbookPart = document.AddWorkbookPart();
				workbookPart.Workbook = new Workbook();

				// Workbook view.
				workbookPart.Workbook.AppendChild(
					new BookViews(new WorkbookView()));

				// Styles.
				var stylesPart =
					workbookPart.AddNewPart<WorkbookStylesPart>();

				stylesPart.Stylesheet = CreateStylesheet();
				stylesPart.Stylesheet.Save();

				// Worksheet.
				var worksheetPart =
					workbookPart.AddNewPart<WorksheetPart>();

				var worksheet = new Worksheet();

				int tableColumnCount = Math.Max(model.Columns.Count, 1);
				int titleColumnCount = Math.Max(model.Columns.Count, 2);
				int rowNumber = 1;

				int headerRow = GetHeaderRow(model);

				// Freeze all rows above the column headings.
				var sheetViews = new SheetViews();
				var sheetView = new SheetView
				{
					WorkbookViewId = 0U
				};

				sheetView.Append(new Pane
				{
					VerticalSplit = (uint)headerRow,
					TopLeftCell = $"A{headerRow + 1}",
					ActivePane = PaneValues.BottomLeft,
					State = PaneStateValues.Frozen
				});

				sheetView.Append(new Selection
				{
					Pane = PaneValues.BottomLeft,
					ActiveCell = $"A{headerRow + 1}",
					SequenceOfReferences = new ListValue<StringValue>
					{
						InnerText = $"A{headerRow + 1}"
					}
				});

				sheetViews.Append(sheetView);
				worksheet.Append(sheetViews);

				// Approximate column widths from headings and values.
				var columns = new Columns();

				for (int col = 0; col < tableColumnCount; col++)
				{
					int maxLength = col < model.Columns.Count
						? model.Columns[col].Length
						: 10;

					foreach (var dataRow in model.Rows)
					{
						if (col < dataRow.Count)
						{
							maxLength = Math.Max(
								maxLength,
								ReportExportModel.FormatValue(dataRow[col]).Length);
						}
					}

					columns.Append(new Column
					{
						Min = (uint)(col + 1),
						Max = (uint)(col + 1),
						Width = Math.Clamp(maxLength + 2, 12, 40),
						CustomWidth = true
					});
				}

				worksheet.Append(columns);

				var sheetData = new SheetData();

				// Report title.
				uint titleRowNumber = (uint)rowNumber;
				var titleRow = new Row { RowIndex = titleRowNumber };

				titleRow.Append(CreateTextCell(
					$"A{rowNumber}",
					model.ReportTitle,
					1U));

				sheetData.Append(titleRow);

				rowNumber += 2;

				// Company and report metadata.
				AddDetail(sheetData, ref rowNumber, "Company", model.CompanyName);
				AddDetail(sheetData, ref rowNumber, "Address", model.CompanyAddress);
				AddDetail(sheetData, ref rowNumber, "Email", model.CompanyEmail);
				AddDetail(sheetData, ref rowNumber, "Phone", model.CompanyPhone);
				AddDetail(sheetData, ref rowNumber, "Description", model.Description);

				AddDetail(
					sheetData,
					ref rowNumber,
					"Generated",
					model.GeneratedAt.ToString(
						"dd MMM yyyy HH:mm",
						CultureInfo.InvariantCulture));

				AddDetail(
					sheetData, ref rowNumber, "Page",
					model.PageNumber.ToString(CultureInfo.InvariantCulture));

				AddDetail(
					sheetData, ref rowNumber, "Page size",
					model.PageSize.ToString(CultureInfo.InvariantCulture));

				AddDetail(
					sheetData, ref rowNumber, "Total matching records",
					model.TotalCount.ToString(CultureInfo.InvariantCulture));

				foreach (var filter in model.Filters)
				{
					AddDetail(
						sheetData,
						ref rowNumber,
						filter.Key,
						filter.Value);
				}

				rowNumber++;

				// Column headings.
				uint headerRowNumber = (uint)rowNumber;
				var headingRow = new Row
				{
					RowIndex = headerRowNumber
				};

				if (model.Columns.Count == 0)
				{
					headingRow.Append(CreateTextCell(
						$"A{rowNumber}",
						"No columns defined",
						3U));
				}
				else
				{
					for (int col = 0; col < model.Columns.Count; col++)
					{
						headingRow.Append(CreateTextCell(
							$"{GetColumnName(col + 1)}{rowNumber}",
							model.Columns[col],
							3U));
					}
				}

				sheetData.Append(headingRow);
				rowNumber++;

				// Data rows.
				foreach (var dataRow in model.Rows)
				{
					var row = new Row
					{
						RowIndex = (uint)rowNumber
					};

					for (int col = 0; col < model.Columns.Count; col++)
					{
						object? value = col < dataRow.Count
							? dataRow[col]
							: null;

						row.Append(CreateValueCell(
							$"{GetColumnName(col + 1)}{rowNumber}",
							value));
					}

					sheetData.Append(row);
					rowNumber++;
				}

				worksheet.Append(sheetData);

				// IMPORTANT: AutoFilter must precede MergeCells
				// in the worksheet XML element sequence.
				int lastDataRow = Math.Max(
					rowNumber - 1,
					(int)headerRowNumber);

				worksheet.Append(new AutoFilter
				{
					Reference =
						$"A{headerRowNumber}:{GetColumnName(tableColumnCount)}{lastDataRow}"
				});

				// Merge the report title across the report columns.
				worksheet.Append(new MergeCells(
					new MergeCell
					{
						Reference =
							$"A{titleRowNumber}:{GetColumnName(titleColumnCount)}{titleRowNumber}"
					}));

				worksheetPart.Worksheet = worksheet;
				worksheetPart.Worksheet.Save();

				// Register the worksheet in the workbook.
				var sheets = workbookPart.Workbook.AppendChild(new Sheets());

				sheets.Append(new Sheet
				{
					Id = workbookPart.GetIdOfPart(worksheetPart),
					SheetId = 1U,
					Name = MakeSheetName(model.ReportTitle)
				});

				workbookPart.Workbook.Save();
			}

			return stream.ToArray();
		}

		private static int GetHeaderRow(ReportExportModel model)
		{
			int row = 3;

			CountDetail(model.CompanyName, ref row);
			CountDetail(model.CompanyAddress, ref row);
			CountDetail(model.CompanyEmail, ref row);
			CountDetail(model.CompanyPhone, ref row);
			CountDetail(model.Description, ref row);

			CountDetail(
				model.GeneratedAt.ToString(
					"dd MMM yyyy HH:mm",
					CultureInfo.InvariantCulture),
				ref row);

			CountDetail(model.PageNumber.ToString(), ref row);
			CountDetail(model.PageSize.ToString(), ref row);
			CountDetail(model.TotalCount.ToString(), ref row);

			foreach (var filter in model.Filters)
			{
				CountDetail(filter.Value, ref row);
			}

			return row + 1;
		}

		private static void CountDetail(string? value, ref int row)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				row++;
			}
		}

		private static void AddDetail(
			SheetData sheetData,
			ref int rowNumber,
			string label,
			string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			var row = new Row
			{
				RowIndex = (uint)rowNumber
			};

			row.Append(CreateTextCell($"A{rowNumber}", label, 2U));
			row.Append(CreateTextCell($"B{rowNumber}", value));

			sheetData.Append(row);
			rowNumber++;
		}

		private static Cell CreateTextCell(
			string cellReference,
			string? value,
			uint styleIndex = 0U)
		{
			return new Cell
			{
				CellReference = cellReference,
				DataType = CellValues.InlineString,
				StyleIndex = styleIndex,
				InlineString = new InlineString(
					new Text(value ?? string.Empty)
					{
						Space = SpaceProcessingModeValues.Preserve
					})
			};
		}

		private static Cell CreateValueCell(
			string cellReference,
			object? value)
		{
			if (value is null)
			{
				return new Cell
				{
					CellReference = cellReference
				};
			}

			if (value is DateOnly dateOnly)
			{
				return CreateNumberCell(
					cellReference,
					dateOnly.ToDateTime(TimeOnly.MinValue).ToOADate(),
					4U);
			}

			if (value is DateTime dateTime)
			{
				return CreateNumberCell(
					cellReference,
					dateTime.ToOADate(),
					dateTime.TimeOfDay == TimeSpan.Zero ? 4U : 5U);
			}

			if (value is bool boolean)
			{
				return new Cell
				{
					CellReference = cellReference,
					DataType = CellValues.Boolean,
					CellValue = new CellValue(boolean ? "1" : "0")
				};
			}

			if (value is byte or sbyte or short or ushort or int or uint
				or long or ulong or decimal or double or float)
			{
				return new Cell
				{
					CellReference = cellReference,
					CellValue = new CellValue(
						Convert.ToString(
							value,
							CultureInfo.InvariantCulture))
				};
			}

			return CreateTextCell(
				cellReference,
				ReportExportModel.FormatValue(value));
		}

		private static Cell CreateNumberCell(
			string cellReference,
			double value,
			uint styleIndex)
		{
			return new Cell
			{
				CellReference = cellReference,
				StyleIndex = styleIndex,
				CellValue = new CellValue(
					value.ToString(CultureInfo.InvariantCulture))
			};
		}

		private static Stylesheet CreateStylesheet()
		{
			var numberingFormats = new NumberingFormats();

			numberingFormats.Append(new NumberingFormat
			{
				NumberFormatId = 164U,
				FormatCode = "yyyy-mm-dd"
			});

			numberingFormats.Append(new NumberingFormat
			{
				NumberFormatId = 165U,
				FormatCode = "yyyy-mm-dd hh:mm"
			});

			var fonts = new Fonts();
			fonts.Append(new Font());

			fonts.Append(new Font(
				new Bold(),
				new Color { Rgb = "FF17365D" },
				new FontSize { Val = 18D }));

			fonts.Append(new Font(new Bold()));

			fonts.Append(new Font(
				new Bold(),
				new Color { Rgb = "FFFFFFFF" }));

			var fills = new Fills();

			fills.Append(new Fill(new PatternFill
			{
				PatternType = PatternValues.None
			}));

			fills.Append(new Fill(new PatternFill
			{
				PatternType = PatternValues.Gray125
			}));

			fills.Append(new Fill(new PatternFill(
				new ForegroundColor { Rgb = "FF17365D" })
			{
				PatternType = PatternValues.Solid
			}));

			var borders = new Borders();
			borders.Append(new Border());

			var cellStyleFormats = new CellStyleFormats();
			cellStyleFormats.Append(new CellFormat());

			var cellFormats = new CellFormats();

			// Style 0: Default.
			cellFormats.Append(new CellFormat());

			// Style 1: Report title.
			cellFormats.Append(new CellFormat
			{
				FontId = 1U,
				ApplyFont = true
			});

			// Style 2: Metadata labels.
			cellFormats.Append(new CellFormat
			{
				FontId = 2U,
				ApplyFont = true
			});

			// Style 3: Column headings.
			cellFormats.Append(new CellFormat
			{
				FontId = 3U,
				FillId = 2U,
				ApplyFont = true,
				ApplyFill = true
			});

			// Style 4: Date.
			cellFormats.Append(new CellFormat
			{
				NumberFormatId = 164U,
				ApplyNumberFormat = true
			});

			// Style 5: Date and time.
			cellFormats.Append(new CellFormat
			{
				NumberFormatId = 165U,
				ApplyNumberFormat = true
			});

			var cellStyles = new CellStyles();

			cellStyles.Append(new CellStyle
			{
				Name = "Normal",
				FormatId = 0U,
				BuiltinId = 0U
			});

			return new Stylesheet(
				numberingFormats,
				fonts,
				fills,
				borders,
				cellStyleFormats,
				cellFormats,
				cellStyles);
		}

		private static string GetColumnName(int columnNumber)
		{
			string name = string.Empty;

			while (columnNumber > 0)
			{
				int remainder = (columnNumber - 1) % 26;

				name = (char)('A' + remainder) + name;
				columnNumber = (columnNumber - 1) / 26;
			}

			return name;
		}

		private static string MakeSheetName(string? title)
		{
			var name = string.IsNullOrWhiteSpace(title)
				? "Report"
				: title;

			foreach (char invalid in new[]
			{
				':', '\\', '/', '?', '*', '[', ']'
			})
			{
				name = name.Replace(invalid, '-');
			}

			name = name.Trim();

			if (name.Length > 31)
			{
				name = name[..31];
			}

			return string.IsNullOrWhiteSpace(name)
				? "Report"
				: name;
		}
	}
}
