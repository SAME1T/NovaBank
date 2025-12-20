using NovaBank.Core.Enums;
namespace NovaBank.Contracts.PaymentOrders;
public sealed record CreatePaymentOrderRequest(Guid AccountId, string PayeeName, string PayeeIban, decimal Amount, Currency Currency, string CronExpr);
public sealed record PaymentOrderResponse(Guid Id, Guid AccountId, string PayeeName, string PayeeIban, decimal Amount, string Currency, string CronExpr, string Status, DateTime? NextRunAt);

