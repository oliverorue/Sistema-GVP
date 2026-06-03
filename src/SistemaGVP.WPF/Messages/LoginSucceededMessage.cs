using CommunityToolkit.Mvvm.Messaging.Messages;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.WPF.Messages;

/// <summary>
/// Mensaje enviado via WeakReferenceMessenger cuando el login es exitoso.
/// Reemplaza el patrón legacy de eventos fuertes para evitar memory leaks.
/// </summary>
public sealed class LoginSucceededMessage : ValueChangedMessage<UserDto>
{
    public LoginSucceededMessage(UserDto value) : base(value)
    {
    }
}
