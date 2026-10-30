using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Models.ViewModels;
using ReportSystem.Services;

namespace ReportSystem.Controllers
{
    
    public class ReportCatalogController : Controller
    {

        private readonly IReportingService _reportingService;

        public ReportCatalogController(IReportingService reportingService) 
        {
            _reportingService = reportingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadReport
            (string reportType,
            int pageNumber = 1,
            int pageSize = 50)
        {

            NormalizePagination(ref pageNumber, ref pageSize);

            switch (reportType)
            {
                case "PolicyRegister":
                    {
                        var filter = new PolicyReportFilter();

                        var result = await _reportingService.GetPoliciesAsync(filter, pageNumber, pageSize);

                        var viewModel = new PolicyReportFilterViewModel
                        {
                            filter = filter,
                            Policies = result.Policies,
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalCount = result.TotalCount
                        };


                        return PartialView("~/Views/Reports/_PolicyRegister.cshtml", viewModel);
                    }

				case "PolicyStatus":
					{
						var filter = new PolicyStatusReportFilter();

						var result = await _reportingService.GetPolicyStatusReportAsync(filter, pageNumber, pageSize);

						var viewModel = new PolicyStatusReportViewModel
						{
							Filter = filter,
							Rows = result.Rows,
							PageNumber = pageNumber,
							PageSize = pageSize,
							TotalCount = result.TotalCount
						};


						return PartialView("~/Views/PolicyStatusReport/_PolicyStatus.cshtml", viewModel);
					}



				case "PremiumDue":
                    return PartialView("_ComingSoon", "Premium Due Report");

                case "PremiumCollectionSummary":
					return PartialView("_ComingSoon", "Premium Due Report");

                case "OutstandingPremium":
					return PartialView("_ComingSoon", "Outstanding Premium Report");

				case "PremiumCollection":
					return PartialView("_ComingSoon","Premium Collection Report");

                default:
                    return BadRequest("Invalid Report Selection");
			}
        }

        private static void NormalizePagination
            (ref int pageNumber, ref int pageSize)
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
