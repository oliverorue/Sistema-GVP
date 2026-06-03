using FluentValidation;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Validators;

public class CustomerValidator : AbstractValidator<CustomerDto>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("El RUC no puede exceder 50 caracteres.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("El teléfono no puede exceder 30 caracteres.");

        RuleFor(x => x.Email)
            .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres.")
            .EmailAddress().WithMessage("El formato del email no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres.");

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("El límite de crédito no puede ser negativo.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("La empresa es obligatoria.");
    }
}
