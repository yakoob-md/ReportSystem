
namespace ReportSystem.Models.ViewModels;

public class PolicyReportPageResult
{
	public IReadOnlyList<PolicyReportRow> Policies { get; set; }
		= Array.Empty<PolicyReportRow>();

	public int TotalCount { get; set; }
}
