using FluentValidation;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Validators;

public class CreateInventoryMovementValidator : AbstractValidator<CreateInventoryMovementDto>
{
    public CreateInventoryMovementValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("El producto es obligatorio.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("La empresa es obligatoria.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo de movimiento es obligatorio.")
            .Must(t => t is "IN" or "OUT" or "ADJUSTMENT")
            .WithMessage("El tipo de movimiento debe ser IN, OUT o ADJUSTMENT.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo del movimiento es obligatorio.")
            .MaximumLength(100).WithMessage("El motivo no puede exceder 100 caracteres.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres.");
    }
}
