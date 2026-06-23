using FluentValidation;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Validators;

public class UserValidator : AbstractValidator<UserDto>
{
    public UserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("El nombre de usuario solo puede contener letras, números y guión bajo.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre completo no puede exceder 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres.")
            .EmailAddress().WithMessage("El formato del email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
            .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres.")
            .When(x => string.IsNullOrWhiteSpace(x.Password) == false);

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(r => r is "Admin" or "Cashier" or "Supervisor")
            .WithMessage("El rol debe ser Admin, Cashier o Supervisor.");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("La empresa es obligatoria.");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
