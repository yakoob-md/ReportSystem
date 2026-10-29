namespace ReportSystem.Models.ViewModels
{
    public class PolicyReportRow
    {
		public int PolicyId { get; set; }

		public string PolicyNumber { get; set; } = "";

		public string PolicyholderName { get; set; } = "";

		public string ProductName { get; set; } = "";

		public DateTime IssueDate { get; set; }

		public DateTime EffectiveFrom { get; set; }

		public DateTime EffectiveTo { get; set; }

		public string PolicyStatus { get; set; } = "";

		public decimal PremiumAmount { get; set; }

		public string CurrencyCode { get; set; } = "";
	}
}
