using System.ComponentModel.DataAnnotations;

namespace MedicalApi.Models;

public sealed class CreatePatientRequest
{
    public string? Pin { get; init; }

    [Phone]
    public string? Phone { get; init; }

    [EmailAddress]
    public string? Email { get; init; }

    [StringLength(10)]
    public string? BloodType { get; init; }
}
