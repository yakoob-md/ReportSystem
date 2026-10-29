
namespace ReportSystem.Models.ViewModels
{
	public class PolicyStatusReportRow
	{
		public int PolicyId { get; set; }

		public string PolicyNumber { get; set; } = string.Empty;

		public string PolicyholderName { get; set; } = string.Empty;

		public string? ProductName { get; set; }

		public DateTime IssueDate { get; set; }

		public string? CurrentPolicyStatus { get; set; }

		public string? PreviousStatus { get; set; }

		public string NewStatus { get; set; } = string.Empty;

		public DateTime ChangedAt { get; set; }

		public string? ChangeReason { get; set; }
	}
}
