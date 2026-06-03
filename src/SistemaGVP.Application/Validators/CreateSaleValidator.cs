using FluentValidation;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Validators;

public class CreateSaleValidator : AbstractValidator<CreateSaleDto>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("La empresa es obligatoria.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("El método de pago es obligatorio.")
            .Must(m => m is "Cash" or "Card" or "Transfer" or "Credit")
            .WithMessage("El método de pago debe ser Cash, Card, Transfer o Credit.");

        RuleFor(x => x.CashAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El monto en efectivo no puede ser negativo.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La venta debe tener al menos un producto.")
            .Must(items => items.Count > 0).WithMessage("La venta debe tener al menos un producto.");

        RuleForEach(x => x.Items).SetValidator(new CreateSaleDetailItemValidator());
    }
}

public class CreateSaleDetailItemValidator : AbstractValidator<CreateSaleDetailDto>
{
    public CreateSaleDetailItemValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("El producto es obligatorio.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.")
            .LessThanOrEqualTo(x => x.UnitPrice * x.Quantity)
            .WithMessage("El descuento no puede superar el subtotal del producto.");
    }
}
