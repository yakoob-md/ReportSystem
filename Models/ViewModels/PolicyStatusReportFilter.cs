
namespace ReportSystem.Models.ViewModels
{
	public class PolicyStatusReportFilter
	{
		// Filter by the policy's issue date.
		public DateTime? IssueDateFrom { get; set; }

		public DateTime? IssueDateTo { get; set; }

		// Current status stored in dbo.Policies.
		public string? Status { get; set; }

		// Status-history event filters.
		public string? PreviousStatus { get; set; }

		public string? NewStatus { get; set; }

		// Optional policy number search.
		public string? PolicyNumber { get; set; }
	}
}
