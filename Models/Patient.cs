using System.ComponentModel.DataAnnotations;

namespace MedicalApi.Models;

public class Patient
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите имя пациента.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 100 символов.")]
    public string Name { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    [Phone(ErrorMessage = "Укажите телефон в корректном формате.")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Укажите корректный email.")]
    public string? Email { get; set; }

    [StringLength(10, ErrorMessage = "Группа крови слишком длинная.")]
    public string? BloodType { get; set; }
}