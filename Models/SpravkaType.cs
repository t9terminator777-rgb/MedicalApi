using System.ComponentModel.DataAnnotations;

namespace MedicalApi.Models;

public class SpravkaType
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите название вида справки.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 100 символов.")]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "Цена должна быть больше нуля.")]
    public decimal Price { get; set; }

    [Range(1, 3650, ErrorMessage = "Срок действия должен быть от 1 до 3650 дней.")]
    public int ValidDays { get; set; }
}