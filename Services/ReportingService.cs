using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using ReportSystem.Data;
using ReportSystem.Models.ViewModels;

namespace ReportSystem.Services
{
    public class ReportingService : IReportingService
    {

        private readonly SqlConnectionFactory _connectionFactory;

        public ReportingService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

		//public async Task<IReadOnlyList<PolicyReportRow>> GetPolicies(
		//    PolicyReportFilter filter,
		//    int pageNumber,
		//    int pageSize)
		//{
		//    const string sql = """
		//SELECT COUNT(*)
		//FROM dbo.Policies AS p
		//INNER JOIN dbo.Parties AS party
		//    ON p.PolicyholderPartyId = party.PartyId
		//INNER JOIN dbo.InsuranceProducts AS product
		//    ON p.ProductId = product.ProductId
		//WHERE
		//    (@IssueDateFrom IS NULL
		//        OR p.IssueDate >= @IssueDateFrom)
		//    AND
		//    (@IssueDateTo IS NULL
		//        OR p.IssueDate < DATEADD(day, 1, @IssueDateTo))
		//    AND
		//    (@PolicyStatus IS NULL
		//        OR p.PolicyStatus = @PolicyStatus);

		//SELECT
		//    p.PolicyId,
		//    p.PolicyNumber,
		//    CASE
		//        WHEN party.PartyType = 'Organization'
		//            THEN party.OrganizationName
		//        ELSE LTRIM(RTRIM(
		//            CONCAT(party.FirstName, ' ', party.LastName)
		//        ))
		//    END AS PolicyholderName,
		//    product.ProductName,
		//    p.IssueDate,
		//    p.EffectiveFrom,
		//    p.EffectiveTo,
		//    p.PolicyStatus,
		//    p.PremiumAmount,
		//    p.CurrencyCode
		//FROM dbo.Policies AS p
		//INNER JOIN dbo.Parties AS party
		//    ON p.PolicyholderPartyId = party.PartyId
		//INNER JOIN dbo.InsuranceProducts AS product
		//    ON p.ProductId = product.ProductId
		//WHERE
		//    (@IssueDateFrom IS NULL
		//        OR p.IssueDate >= @IssueDateFrom)
		//    AND
		//    (@IssueDateTo IS NULL
		//        OR p.IssueDate < DATEADD(day, 1, @IssueDateTo))
		//    AND
		//    (@PolicyStatus IS NULL
		//        OR p.PolicyStatus = @PolicyStatus)
		//ORDER BY
		//    p.IssueDate DESC,
		//    p.PolicyNumber,
		//    p.PolicyId
		//OFFSET @Offset ROWS
		//FETCH NEXT @PageSize ROWS ONLY;
		//""";

		//    await using var connection = _connectionFactory.CreateConnection();

		//    var parameters = new
		//    {
		//        IssueDateFrom = filter.IssueDateFrom?.Date,
		//        IssueDateTo = filter.IssueDateTo?.Date,
		//        PolicyStatus = string.IsNullOrWhiteSpace(filter.Status)
		//    ? null
		//    : filter.Status.Trim(),
		//       
		//    };


		//    var rows = await connection.QueryAsync<PolicyReportRow>(sql, parameters);

		//    return rows.AsList();
		//}


		public async Task<PolicyReportPageResult> GetPoliciesAsync(
			PolicyReportFilter filter,
			int pageNumber,
			int pageSize)
		{
			const string sql = """
        SELECT COUNT(*)
        FROM dbo.Policies AS p
        INNER JOIN dbo.Parties AS party
            ON p.PolicyholderPartyId = party.PartyId
        INNER JOIN dbo.InsuranceProducts AS product
            ON p.ProductId = product.ProductId
        WHERE
            (@IssueDateFrom IS NULL
                OR p.IssueDate >= @IssueDateFrom)
            AND
            (@IssueDateTo IS NULL
                OR p.IssueDate < DATEADD(day, 1, @IssueDateTo))
            AND
            (@PolicyStatus IS NULL
                OR p.PolicyStatus = @PolicyStatus);

        SELECT
            p.PolicyId,
            p.PolicyNumber,
            CASE
                WHEN party.PartyType = 'Organization'
                    THEN party.OrganisationName
                ELSE LTRIM(RTRIM(
                    CONCAT(party.FirstName, ' ', party.LastName)
                ))
            END AS PolicyholderName,
            product.ProductName,
            p.IssueDate,
            p.EffectiveFrom,
            p.EffectiveTo,
            p.PolicyStatus,
            p.PremiumAmount,
            p.CurrencyCode
        FROM dbo.Policies AS p
        INNER JOIN dbo.Parties AS party
            ON p.PolicyholderPartyId = party.PartyId
        INNER JOIN dbo.InsuranceProducts AS product
            ON p.ProductId = product.ProductId
        WHERE
            (@IssueDateFrom IS NULL
                OR p.IssueDate >= @IssueDateFrom)
            AND
            (@IssueDateTo IS NULL
                OR p.IssueDate < DATEADD(day, 1, @IssueDateTo))
            AND
            (@PolicyStatus IS NULL
                OR p.PolicyStatus = @PolicyStatus)
        ORDER BY
            p.IssueDate DESC,
            p.PolicyNumber,
            p.PolicyId
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

			var parameters = new
			{
				IssueDateFrom = filter.IssueDateFrom?.Date,
				IssueDateTo = filter.IssueDateTo?.Date,
				PolicyStatus = string.IsNullOrWhiteSpace(filter.PolicyStatus)
					? null
					: filter.PolicyStatus.Trim(),
				Offset = (pageNumber - 1) * pageSize,
				PageSize = pageSize
			};

			await using var connection =
				_connectionFactory.CreateConnection();

			using var results = await connection.QueryMultipleAsync(
				sql,
				parameters);

			var totalCount = await results.ReadSingleAsync<int>();

			var policies = (await results.ReadAsync<PolicyReportRow>())
				.AsList();

			return new PolicyReportPageResult
			{
				TotalCount = totalCount,
				Policies = policies
			};
		}


		public async Task<IReadOnlyList<PolicyReportRow>> GetAllPoliciesAsync(
			PolicyReportFilter filter)
		{
			const string sql = """
        SELECT
            p.PolicyId,
            p.PolicyNumber,
            CASE
                WHEN party.PartyType = 'Organization'
                    THEN party.OrganisationName
                ELSE LTRIM(RTRIM(
                    CONCAT(party.FirstName, ' ', party.LastName)
                ))
            END AS PolicyholderName,
            product.ProductName,
            p.IssueDate,
            p.EffectiveFrom,
            p.EffectiveTo,
            p.PolicyStatus,
            p.PremiumAmount,
            p.CurrencyCode
        FROM dbo.Policies AS p
        INNER JOIN dbo.Parties AS party
            ON p.PolicyholderPartyId = party.PartyId
        INNER JOIN dbo.InsuranceProducts AS product
            ON p.ProductId = product.ProductId
        WHERE
            (@IssueDateFrom IS NULL
                OR p.IssueDate >= @IssueDateFrom)
            AND
            (@IssueDateTo IS NULL
                OR p.IssueDate < DATEADD(day, 1, @IssueDateTo))
            AND
            (@PolicyStatus IS NULL
                OR p.PolicyStatus = @PolicyStatus)
        ORDER BY
            p.IssueDate DESC,
            p.PolicyNumber,
            p.PolicyId;
        """;

			var parameters = new
			{
				IssueDateFrom = filter.IssueDateFrom?.Date,
				IssueDateTo = filter.IssueDateTo?.Date,
				PolicyStatus = string.IsNullOrWhiteSpace(filter.PolicyStatus)
					? null
					: filter.PolicyStatus.Trim()
			};

			await using var connection =
				_connectionFactory.CreateConnection();

			var policies = await connection.QueryAsync<PolicyReportRow>(
				sql,
				parameters);

			return policies.AsList();
		}

		// for policystatus report
		public async Task<PolicyStatusReportPageResult> GetPolicyStatusReportAsync
			(PolicyStatusReportFilter filter,
			int pageNumber,
			int pageSize)
		{
			const string sql = """
        SELECT COUNT(*)
        FROM dbo.PolicyStatusHistory AS h
        INNER JOIN dbo.Policies AS p
            ON h.PolicyId = p.PolicyId
        INNER JOIN dbo.Parties AS party
            ON p.PolicyholderPartyId = party.partyId
        LEFT JOIN dbo.InsuranceProducts AS product
            ON p.ProductId = product.productId
        WHERE
            (@IssueDateFrom IS NULL
                OR p.IssueDate >= @IssueDateFrom)
            AND
            (@IssueDateTo IS NULL
                OR p.IssueDate < DATEADD(day, 1, @IssueDateTo))
            AND
            (@PolicyStatus IS NULL
                OR p.PolicyStatus = @PolicyStatus)
            AND
            (@PreviousStatus IS NULL
                OR h.PreviousStatus = @PreviousStatus)
            AND
            (@NewStatus IS NULL
                OR h.NewStatus = @NewStatus)
            AND
            (@PolicyNumber IS NULL
                OR p.PolicyNumber LIKE '%' + @PolicyNumber + '%');

        SELECT
            p.PolicyId,
            p.PolicyNumber,
            CASE
                WHEN party.partyType = 'Organisation'
                    THEN party.organisationName
                ELSE LTRIM(RTRIM(
                    CONCAT(party.firstName, ' ', party.lastName)
                ))
            END AS PolicyholderName,
            product.productName AS ProductName,
            p.IssueDate,
            p.PolicyStatus AS CurrentPolicyStatus,
            h.PreviousStatus,
            h.NewStatus,
            h.ChangedAt,
            h.ChangeReason
        FROM dbo.PolicyStatusHistory AS h
        INNER JOIN dbo.Policies AS p
            ON h.PolicyId = p.PolicyId
        INNER JOIN dbo.Parties AS party
            ON p.PolicyholderPartyId = party.partyId
        LEFT JOIN dbo.InsuranceProducts AS product
            ON p.ProductId = product.productId
        WHERE
            (@IssueDateFrom IS NULL
                OR p.IssueDate >= @IssueDateFrom)
            AND
            (@IssueDateTo IS NULL
                OR p.IssueDate < DATEADD(day, 1, @IssueDateTo))
            AND
            (@PolicyStatus IS NULL
                OR p.PolicyStatus = @PolicyStatus)
            AND
            (@PreviousStatus IS NULL
                OR h.PreviousStatus = @PreviousStatus)
            AND
            (@NewStatus IS NULL
                OR h.NewStatus = @NewStatus)
            AND
            (@PolicyNumber IS NULL
                OR p.PolicyNumber LIKE '%' + @PolicyNumber + '%')
        ORDER BY
            h.ChangedAt DESC,
            p.PolicyNumber,
            h.PolicyStatusHistoryId
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

			var parameters = new
			{
				IssueDateFrom = filter.IssueDateFrom?.Date,
				IssueDateTo = filter.IssueDateTo?.Date,

				PolicyStatus = string.IsNullOrWhiteSpace(filter.Status)
			? null
			: filter.Status.Trim(),

				PreviousStatus = string.IsNullOrWhiteSpace(filter.PreviousStatus)
			? null
			: filter.PreviousStatus.Trim(),

				NewStatus = string.IsNullOrWhiteSpace(filter.NewStatus)
			? null
			: filter.NewStatus.Trim(),

				PolicyNumber = string.IsNullOrWhiteSpace(filter.PolicyNumber)
			? null
			: filter.PolicyNumber.Trim(),

				Offset = (pageNumber - 1) * pageSize,
				PageSize = pageSize
			};

            await using var connection = _connectionFactory.CreateConnection();

            using var results = await connection.QueryMultipleAsync(sql, parameters);

            var totalCount = await results.ReadSingleAsync<int>();

            var rows = (await results.ReadAsync<PolicyStatusReportRow>()).AsList();

            return new PolicyStatusReportPageResult
            {
                TotalCount = totalCount,
                Rows = rows
            };
		}

        public async Task<IReadOnlyList<PolicyStatusReportRow>>GetAllPolicyStatusReportAsync(PolicyStatusReportFilter filter)
        {
			const string sql = """
        SELECT
            p.PolicyId,
            p.PolicyNumber,
            CASE
                WHEN party.partyType = 'Organization'
                    THEN party.organisation
                ELSE LTRIM(RTRIM(
                    CONCAT(party.firstName, ' ', party.lastName)
                ))
            END AS PolicyholderName,
            product.productName AS ProductName,
            p.IssueDate,
            p.PolicyStatus AS CurrentPolicyStatus,
            h.PreviousStatus,
            h.NewStatus,
            h.ChangedAt,
            h.ChangeReason
        FROM dbo.PolicyStatusHistory AS h
        INNER JOIN dbo.Policies AS p
            ON h.PolicyId = p.PolicyId
        INNER JOIN dbo.Parties AS party
            ON p.PolicyholderPartyId = party.partyId
        LEFT JOIN dbo.InsuranceProducts AS product
            ON p.ProductId = product.productId
        WHERE
            (@IssueDateFrom IS NULL
                OR p.IssueDate >= @IssueDateFrom)
            AND
            (@IssueDateTo IS NULL
                OR p.IssueDate < DATEADD(day, 1, @IssueDateTo))
            AND
            (@PolicyStatus IS NULL
                OR p.PolicyStatus = @PolicyStatus)
            AND
            (@PreviousStatus IS NULL
                OR h.PreviousStatus = @PreviousStatus)
            AND
            (@NewStatus IS NULL
                OR h.NewStatus = @NewStatus)
            AND
            (@PolicyNumber IS NULL
                OR p.PolicyNumber LIKE '%' + @PolicyNumber + '%')
        ORDER BY
            h.ChangedAt DESC,
            p.PolicyNumber,
            h.PolicyStatusHistoryId;
        """;

			var parameters = new
			{
				IssueDateFrom = filter.IssueDateFrom?.Date,
				IssueDateTo = filter.IssueDateTo?.Date,

				Status = string.IsNullOrWhiteSpace(filter.Status)
					? null
					: filter.Status.Trim(),

				PreviousStatus = string.IsNullOrWhiteSpace(filter.PreviousStatus)
					? null
					: filter.PreviousStatus.Trim(),

				NewStatus = string.IsNullOrWhiteSpace(filter.NewStatus)
					? null
					: filter.NewStatus.Trim(),

				PolicyNumber = string.IsNullOrWhiteSpace(filter.PolicyNumber)
					? null
					: filter.PolicyNumber.Trim()
			};

			
			await using var connection =
				_connectionFactory.CreateConnection();

			var rows = await connection.QueryAsync<PolicyStatusReportRow>(
				sql,
				parameters);

			return rows.AsList();
		}
    }
}
