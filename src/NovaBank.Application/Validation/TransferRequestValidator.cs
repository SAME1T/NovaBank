using FluentValidation;
using NovaBank.Contracts.Transactions;

namespace NovaBank.Application.Validation;

public class TransferInternalRequestValidator : AbstractValidator<TransferInternalRequest>
{
    public TransferInternalRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty().WithMessage("Gönderen hesap ID boş olamaz.");

        RuleFor(x => x.ToAccountId)
            .NotEmpty().WithMessage("Alıcı hesap ID boş olamaz.")
            .NotEqual(x => x.FromAccountId).WithMessage("Aynı hesaba transfer yapılamaz.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");
    }
}

public class TransferExternalRequestValidator : AbstractValidator<TransferExternalRequest>
{
    public TransferExternalRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty().WithMessage("Gönderen hesap ID boş olamaz.");

        RuleFor(x => x.ToIban)
            .NotEmpty().WithMessage("Alıcı IBAN boş olamaz.")
            .MinimumLength(26).WithMessage("IBAN geçerli bir formatta olmalıdır.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");
    }
}

