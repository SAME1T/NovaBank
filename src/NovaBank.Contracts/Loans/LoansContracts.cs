using NovaBank.Core.Enums;
namespace NovaBank.Contracts.Loans;
public sealed record CalcLoanRequest(decimal Principal, Currency Currency, decimal InterestRateAnnual, int TermMonths, decimal? InsuranceRate);
public sealed record CalcLoanResponse(decimal MonthlyInstallment, decimal TotalPayment, decimal TotalInterest);
public sealed record ApplyLoanRequest(Guid CustomerId, decimal Principal, Currency Currency, decimal InterestRateAnnual, int TermMonths, DateTime StartDate);
public sealed record LoanResponse(Guid Id, Guid CustomerId, decimal Principal, string Currency, decimal InterestRateAnnual, int TermMonths, DateTime StartDate, string Status);

