using ReportSystem.Models.ViewModels;

namespace ReportSystem.Services
{
    public interface IReportingService
    {
		//Task<IReadOnlyList<PolicyReportRow>> GetAllPolicies();

		//Task<IReadOnlyList<PolicyReportRow>> GetPolicies(
		//    PolicyReportFilter filter
		//    );

		Task<PolicyReportPageResult> GetPoliciesAsync(
		PolicyReportFilter filter,
		int pageNumber,
		int pageSize);

		Task<IReadOnlyList<PolicyReportRow>> GetAllPoliciesAsync(
			PolicyReportFilter filter);

		// for policy status report - 2 new functions

		Task<PolicyStatusReportPageResult> GetPolicyStatusReportAsync(
		PolicyStatusReportFilter filter,
		int pageNumber,
		int pageSize);

		Task<IReadOnlyList<PolicyStatusReportRow>> GetAllPolicyStatusReportAsync(
			PolicyStatusReportFilter filter);

	}
}
