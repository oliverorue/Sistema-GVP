using FluentValidation;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Validators;

public class ProductValidator : AbstractValidator<ProductDto>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(300).WithMessage("El nombre no puede exceder 300 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.");

        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("El código de barras es obligatorio.")
            .MaximumLength(100).WithMessage("El código de barras no puede exceder 100 caracteres.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("El SKU es obligatorio.")
            .MaximumLength(100).WithMessage("El SKU no puede exceder 100 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a cero.")
            .PrecisionScale(18, 2, true).WithMessage("El precio debe tener máximo 2 decimales.");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("El costo no puede ser negativo.")
            .PrecisionScale(18, 2, true).WithMessage("El costo debe tener máximo 2 decimales.")
            .LessThanOrEqualTo(x => x.Price).WithMessage("El costo no puede ser mayor al precio.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Debe seleccionar una categoría.");

        RuleFor(x => x.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.CurrentStock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock actual no puede ser negativo.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.")
            .MaximumLength(20).WithMessage("La unidad de medida no puede exceder 20 caracteres.");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500).WithMessage("La ruta de imagen no puede exceder 500 caracteres.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("La empresa es obligatoria.");
    }
}
