namespace MedicalApi.Models;

public sealed record Citizen(string Pin, string Name, DateOnly BirthDate)
{
    public static bool IsValidPin(string? pin) =>
        pin is { Length: 14 } && pin.All(char.IsAsciiDigit);
}
