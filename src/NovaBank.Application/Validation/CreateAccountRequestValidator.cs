using FluentValidation;
using NovaBank.Contracts.Accounts;

namespace NovaBank.Application.Validation;

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId boş olamaz.");

        RuleFor(x => x.AccountNo)
            .GreaterThan(0).WithMessage("AccountNo geçerli bir sayı olmalıdır.");

        RuleFor(x => x.OverdraftLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Ek hesap limiti negatif olamaz.");
    }
}

