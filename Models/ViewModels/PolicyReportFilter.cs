namespace ReportSystem.Models.ViewModels
{
    public class PolicyReportFilter
    {
		public DateTime? IssueDateFrom { get; set; }

		public DateTime? IssueDateTo { get; set; }

		public string? PolicyStatus { get; set; }
	}
}
