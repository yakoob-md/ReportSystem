using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ReportSystem.Models.ViewModels;
using ReportSystem.Services;
using static System.Net.WebRequestMethods;

namespace ReportSystem.Controllers
{
	public class ReportsController : Controller
	{
		private readonly IReportingService _reportingService;
		private readonly ExcelExportService _excelService;

		private readonly DocxExportService _docService;

		private readonly HtmlExportService _htmlService;

		private readonly CsvExportService _csvService;

		public ReportsController(
			IReportingService reportingService,
			ExcelExportService excelService,
			DocxExportService docService,
			CsvExportService csvService,
			HtmlExportService htmlService
			)
		{
			_reportingService = reportingService;
			_excelService = excelService;
			_docService = docService;
			_csvService = csvService;
			_htmlService = htmlService;
			
		}

		//     [HttpGet]
		//     public async Task<IActionResult> Index(
		//[FromQuery] PolicyReportFilter filter)
		//     {
		//if (filter.IssueDateFrom.HasValue &&
		//  filter.IssueDateTo.HasValue &&
		//  filter.IssueDateFrom.Value.Date >
		//  filter.IssueDateTo.Value.Date)
		//{
		//	ModelState.AddModelError(
		//		nameof(filter.IssueDateTo),
		//		"The end date must be on or after the start date.");

		//	return View(new PolicyReportFilterViewModel
		//	{
		//		filter = filter,
		//		Policies = Array.Empty<PolicyReportRow>()
		//	});
		//}

		//var policies = await _reportingService.GetPolicies(filter);

		//var viewModel = new PolicyReportFilterViewModel
		//{
		//	filter = filter,
		//	Policies = policies
		//};
		//         return View(viewModel);
		//     }

		//https://localhost:7153/Reports?pageNumber=1&filter.IssueDateFrom=2026-10-04&__Invariant=filter.IssueDateFrom&filter.IssueDateTo=2026-10-28&__Invariant=filter.IssueDateTo&filter.PolicyStatus=&pageSize=25


		[HttpGet]
		public async Task<IActionResult> Index(
			[FromQuery] PolicyReportFilter filter,
			int pageNumber = 1,
			int pageSize = 50)
		{

			NormalizePagination(ref pageNumber, ref pageSize);

			if (!ValidateFilter(filter))
			{
				return View(new PolicyReportFilterViewModel
				{
					filter = filter,
					PageNumber = pageNumber,
					PageSize = pageSize,
					TotalCount = 0
				});
			}
			var result = await _reportingService.GetPoliciesAsync(
			filter,
			pageNumber,
			pageSize);

			var viewModel = new PolicyReportFilterViewModel
			{
				filter = filter,
				Policies = result.Policies,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = result.TotalCount
			};

			return View(viewModel);
		}

		// for preview
		[HttpGet]
		public async Task<IActionResult> Preview(
			[FromQuery] PolicyReportFilter filter,
			[FromQuery] PolicyReportOptions reportOptions,
			int pageNumber = 1,
			int pageSize = 50)
		{
			NormalizePagination(ref pageNumber, ref pageSize);

			if (!ValidateFilter(filter))
				return BadRequest("The selected filters are invalid.");

			var result = await _reportingService.GetPoliciesAsync(
				filter, pageNumber, pageSize);

			var viewModel = new PolicyReportFilterViewModel
			{
				filter = filter,
				Policies = result.Policies,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = result.TotalCount,
				ReportOptions = reportOptions

			};
			return View("Preview", viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> Download(
			[FromQuery] PolicyReportFilter filter,
			[FromQuery] PolicyReportOptions reportOptions,
			int pageNumber = 1, int pageSize = 50,
			string format = "csv")
		{
			NormalizePagination(ref pageNumber, ref pageSize);

			if (!ValidateFilter(filter))
				return BadRequest("The selected filters are invalid.");

			var result = await _reportingService.GetPoliciesAsync(
				filter, pageNumber, pageSize);

			var viewModel = new PolicyReportFilterViewModel
			{
				filter = filter,
				Policies = result.Policies,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = result.TotalCount,
				ReportOptions = reportOptions
			};

			var exportModel = ReportExportMapper.FromPolicyRegister(viewModel);

			switch (format.ToLowerInvariant())
			{
				case "docx":
					//var docxBytes = _docService.GenerateDocx(viewModel);

					//return File(
					//	docxBytes,
					//	"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
					//	"PolicyRegister.docx");

					return File(
						_docService.GenerateDocx(exportModel),
						"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
						$"PolicyRegister_Page{pageNumber}.docx");

				case "excel":
					//var excelBytes = _excelService.GenerateExcel(viewModel);

					//return File(
					//	excelBytes,
					//	"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					//	$"PolicyRegister_Page{pageNumber}.xlsx");

					return File(
						_excelService.GenerateExcel(exportModel),
						"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
						$"PolicyRegister_Page{pageNumber}.xlsx");

				case "html":
					//return File(
					//	_htmlService.GenerateHtml(viewModel),
					//	"text/html; charset=utf-8",
					//	$"PolicyRegister_Page{pageNumber}.html");

					return File(
					_htmlService.GenerateHtml(exportModel),
					"text/html; charset=utf-8",
					$"PolicyRegister_Page{pageNumber}.html");

				case "csv":
					//return File(
					//	_csvService.GenerateCsv(viewModel),
					//	"text/csv; charset=utf-8",
					//	$"PolicyRegister_Page{pageNumber}.csv");

					return File(
					_csvService.GenerateCsv(exportModel),
					"text/csv; charset=utf-8",
					$"PolicyRegister_Page{pageNumber}.csv");

				default:
					return BadRequest(
						"Unsupported format. CSV is currently available.");
			}
		}

        private static void NormalizePagination(
			ref int pageNumber, ref int pageSize)
		{
			// Keep page values within permitted limits.
			pageNumber = Math.Max(1, pageNumber);

			pageSize = pageSize switch
			{
				25 => 25,
				50 => 50,
				100 => 100,
				_ => 50
			};
		}

		private bool ValidateFilter(PolicyReportFilter filter)
		{
			if (filter.IssueDateFrom.HasValue &&
				filter.IssueDateTo.HasValue &&
				filter.IssueDateFrom.Value.Date >
				filter.IssueDateTo.Value.Date)
			{
				ModelState.AddModelError(
					"Filter.IssueDateTo",
					"The end date must be on or after the start date.");

				return false;
			}
			return true;
		}
			

	}
}
