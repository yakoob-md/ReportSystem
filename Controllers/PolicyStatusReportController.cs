
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Models;
using ReportSystem.Models.ViewModels;
using ReportSystem.Services;

namespace ReportSystem.Controllers
{
	public class PolicyStatusReportController : Controller
	{
		private readonly IReportingService _reportingService;
		private readonly CsvExportService _csvService;
		private readonly HtmlExportService _htmlService;
		private readonly ExcelExportService _excelService;
		private readonly DocxExportService _docxService;

		public PolicyStatusReportController(
			IReportingService reportingService,
			CsvExportService csvService,
			HtmlExportService htmlService,
			ExcelExportService excelService,
			DocxExportService docxService)
		{
			_reportingService = reportingService;
			_csvService = csvService;
			_htmlService = htmlService;
			_excelService = excelService;
			_docxService = docxService;
		}

		[HttpGet]
		public async Task<IActionResult> Index(
			[FromQuery] PolicyStatusReportFilter filter,
			int pageNumber = 1,
			int pageSize = 50)
		{
			NormalizePagination(ref pageNumber, ref pageSize);

			var result = await _reportingService
				.GetPolicyStatusReportAsync(
					filter,
					pageNumber,
					pageSize);

			var viewModel = new PolicyStatusReportViewModel
			{
				Filter = filter,
				Rows = result.Rows,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = result.TotalCount
			};

			return View(viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> Preview(
			[FromQuery] PolicyStatusReportFilter filter,
			[FromQuery] PolicyReportOptions reportOptions,
			int pageNumber = 1,
			int pageSize = 50)
		{
			NormalizePagination(ref pageNumber, ref pageSize);

			var result = await _reportingService
				.GetPolicyStatusReportAsync(
					filter,
					pageNumber,
					pageSize);

			var viewModel = new PolicyStatusReportViewModel
			{
				Filter = filter,
				Rows = result.Rows,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = result.TotalCount,
				ReportOptions = reportOptions
			};

			return View("Preview", viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> Download(
			[FromQuery] PolicyStatusReportFilter filter,
			[FromQuery] PolicyReportOptions reportOptions,
			int pageNumber = 1,
			int pageSize = 50,
			[FromQuery] string format = "csv")
		{
			NormalizePagination(ref pageNumber, ref pageSize);

			// Get the filtered records for the requested page.
			var result = await _reportingService
				.GetPolicyStatusReportAsync(
					filter,
					pageNumber,
					pageSize);

			var viewModel = new PolicyStatusReportViewModel
			{
				Filter = filter,
				Rows = result.Rows,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = result.TotalCount,
				ReportOptions = reportOptions
			};

			// Convert report-specific data into the shared export model.
			var exportModel =
				ReportExportMapper.FromPolicyStatusReport(viewModel);

			format = (format ?? "csv").Trim().ToLowerInvariant();
			Console.WriteLine($"Policy Status export format: {format}");

			// Route each format to its correct exporter.
			switch (format.Trim().ToLowerInvariant())
			{
				case "csv":
					return File(
						_csvService.GenerateCsv(exportModel),
						"text/csv; charset=utf-8",
						$"PolicyStatus_Page{pageNumber}.csv");

				case "html":
					return File(
						_htmlService.GenerateHtml(exportModel),
						"text/html; charset=utf-8",
						$"PolicyStatus_Page{pageNumber}.html");

				case "excel":
					return File(
						_excelService.GenerateExcel(exportModel),
						"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
						$"PolicyStatus_Page{pageNumber}.xlsx");

				case "docx":
					return File(
						_docxService.GenerateDocx(exportModel),
						"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
						$"PolicyStatus_Page{pageNumber}.docx");

				default:
					return BadRequest(
						"Unsupported export format. Use csv, html, excel, or docx.");
			}
		}

		private static void NormalizePagination(
			ref int pageNumber,
			ref int pageSize)
		{
			pageNumber = Math.Max(1, pageNumber);

			pageSize = pageSize switch
			{
				25 => 25,
				50 => 50,
				100 => 100,
				_ => 50
			};
		}
	}
}
