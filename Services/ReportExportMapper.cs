using System.Globalization;
using ReportSystem.Models;
using ReportSystem.Models.ViewModels;

namespace ReportSystem.Services
{
    /// <summary>
    /// Converts report-specific view models to the shared export format.
    /// Add one mapping method per report; the format exporters remain shared.
    /// </summary>
    public static class ReportExportMapper
    {
        public static ReportExportModel FromPolicyRegister(
            PolicyReportFilterViewModel viewModel)
        {
            var options = viewModel.ReportOptions;

            return new ReportExportModel
            {
                ReportTitle = DefaultTitle(options.ReportTitle, "Policy Register Report"),
                CompanyName = options.CompanyName,
                CompanyAddress = options.CompanyAddress,
                CompanyEmail = options.CompanyEmail,
                CompanyPhone = options.CompanyPhone,
                Description = options.Description,
                GeneratedAt = DateTime.Now,
                PageNumber = viewModel.PageNumber,
                PageSize = viewModel.PageSize,
                TotalCount = viewModel.TotalCount,
                Filters = new Dictionary<string, string>
                {
                    ["Issue date from"] = viewModel.filter.IssueDateFrom?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "All dates",
                    ["Issue date to"] = viewModel.filter.IssueDateTo?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "All dates",
                    ["Policy status"] = string.IsNullOrWhiteSpace(viewModel.filter.PolicyStatus)
                        ? "All statuses"
                        : viewModel.filter.PolicyStatus
                },
                Columns = new List<string>
                {
                    "Policy Number", "Policyholder", "Product", "Issue Date",
                    "Effective From", "Effective To", "Status", "Currency", "Premium"
                },
                Rows = viewModel.Policies.Select(policy => new List<object?>
                {
                    policy.PolicyNumber,
                    policy.PolicyholderName,
                    policy.ProductName,
                    policy.IssueDate,
                    policy.EffectiveFrom,
                    policy.EffectiveTo,
                    policy.PolicyStatus,
                    policy.CurrencyCode,
                    policy.PremiumAmount
                }).ToList()
            };
        }

        public static ReportExportModel FromPolicyStatusReport(
            PolicyStatusReportViewModel viewModel)
        {
            var options = viewModel.ReportOptions;

            return new ReportExportModel
            {
                ReportTitle = DefaultTitle(options.ReportTitle, "Policy Status Report"),
                CompanyName = options.CompanyName,
                CompanyAddress = options.CompanyAddress,
                CompanyEmail = options.CompanyEmail,
                CompanyPhone = options.CompanyPhone,
                Description = options.Description,
                GeneratedAt = DateTime.Now,
                PageNumber = viewModel.PageNumber,
                PageSize = viewModel.PageSize,
                TotalCount = viewModel.TotalCount,
                Filters = new Dictionary<string, string>
                {
                    ["Policy number"] = string.IsNullOrWhiteSpace(viewModel.Filter.PolicyNumber)
                        ? "All policies"
                        : viewModel.Filter.PolicyNumber,
                    ["Current status"] = string.IsNullOrWhiteSpace(viewModel.Filter.Status)
                        ? "All statuses"
                        : viewModel.Filter.Status,
                    ["Previous status"] = string.IsNullOrWhiteSpace(viewModel.Filter.PreviousStatus)
                        ? "All previous statuses"
                        : viewModel.Filter.PreviousStatus,
                    ["New status"] = string.IsNullOrWhiteSpace(viewModel.Filter.NewStatus)
                        ? "All new statuses"
                        : viewModel.Filter.NewStatus,
                    ["Issue date from"] = viewModel.Filter.IssueDateFrom?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "All dates",
                    ["Issue date to"] = viewModel.Filter.IssueDateTo?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "All dates"
                },
                Columns = new List<string>
                {
                    "Policy Number", "Policyholder", "Product", "Issue Date",
                    "Current Status", "Previous Status", "New Status",
                    "Changed At", "Change Reason"
                },
                Rows = viewModel.Rows.Select(row => new List<object?>
                {
                    row.PolicyNumber,
                    row.PolicyholderName,
                    row.ProductName ?? string.Empty,
                    row.IssueDate,
                    row.CurrentPolicyStatus ?? string.Empty,
                    row.PreviousStatus ?? string.Empty,
                    row.NewStatus,
                    row.ChangedAt,
                    row.ChangeReason ?? string.Empty
                }).ToList()
            };
        }

        private static string DefaultTitle(string? suppliedTitle, string fallback)
        {
            return string.IsNullOrWhiteSpace(suppliedTitle)
                ? fallback
                : suppliedTitle.Trim();
        }
    }
}
