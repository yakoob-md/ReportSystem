namespace ReportSystem.Models.ViewModels
{
    public class PolicyReportFilterViewModel
    {
        public PolicyReportFilter filter { get; set; } = new();

		public IReadOnlyList<PolicyReportRow> Policies { get; set; }
		= Array.Empty<PolicyReportRow>();


		public int PageNumber { get; set; } = 1;

		public int PageSize { get; set; } = 50;

		public int TotalCount { get; set; }

		public int TotalPages =>
			(int)Math.Ceiling(TotalCount / (double)PageSize);

		// Optional details used to customise the report presentation.
		public PolicyReportOptions ReportOptions { get; set; } = new();

	}
}
