using System.ComponentModel.DataAnnotations;

namespace MedicalApi.Models;

// Тело запроса содержит только связи и дату. Название и цену задаёт сервер из каталога.
public class SpravkaRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "PatientId должен быть положительным числом.")]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "SpravkaTypeId должен быть положительным числом.")]
    public int SpravkaTypeId { get; set; }

    public DateTime? IssuedAt { get; set; }
}