namespace ReportSystem.Models.ViewModels
{
    public class PolicyStatusReportViewModel
    {
		public PolicyStatusReportFilter Filter { get; set; } = new();

		public IReadOnlyList<PolicyStatusReportRow> Rows { get; set; }
			= Array.Empty<PolicyStatusReportRow>();

		public int PageNumber { get; set; } = 1;

		public int PageSize { get; set; } = 50;

		public int TotalCount { get; set; }

		public int TotalPages =>
			(int)Math.Ceiling(TotalCount / (double)PageSize);

		public PolicyReportOptions ReportOptions { get; set; } = new();
	}
}
