using System.ComponentModel.DataAnnotations;

namespace MedicalApi.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Укажите имя врача.")]
    [StringLength(100, MinimumLength = 2)]
    public string DoctorName { get; set; } = string.Empty;

    public DateTime ScheduledAt { get; set; }

    [Required(ErrorMessage = "Укажите причину записи.")]
    [StringLength(300, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(30)]
    public string Status { get; set; } = "Запланирован";
}