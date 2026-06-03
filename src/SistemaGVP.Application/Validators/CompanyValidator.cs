using FluentValidation;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Validators;

public class CompanyValidator : AbstractValidator<CompanyDto>
{
    public CompanyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("El RUC es obligatorio.")
            .MaximumLength(50).WithMessage("El RUC no puede exceder 50 caracteres.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("La dirección es obligatoria.")
            .MaximumLength(500).WithMessage("La dirección no puede exceder 500 caracteres.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("El teléfono es obligatorio.")
            .MaximumLength(30).WithMessage("El teléfono no puede exceder 30 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 1).WithMessage("La tasa de IVA debe estar entre 0 y 1 (0% a 100%).");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es obligatoria.")
            .MaximumLength(10).WithMessage("La moneda no puede exceder 10 caracteres.");

        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("El umbral de stock bajo no puede ser negativo.");
    }
}
