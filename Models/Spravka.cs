namespace MedicalApi.Models;

// Выданная пациенту справка. Название и цена берутся из каталога видов справок.
public class Spravka
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int SpravkaTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime IssuedAt { get; set; }
}