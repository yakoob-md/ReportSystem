namespace ReportSystem.Models.ViewModels
{
    public class PolicyStatusReportPageResult
    {
		public int TotalCount { get; set; }

		public IReadOnlyList<PolicyStatusReportRow> Rows { get; set; }
			= Array.Empty<PolicyStatusReportRow>();
	}
}
