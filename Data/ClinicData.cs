using MedicalApi.Models;

namespace MedicalApi.Data;

// Учебное хранилище в памяти. При перезапуске сервера записи пациентов, справок
// и приёмов создаются заново; каталог видов справок заполнен начальными данными.
public static class ClinicData
{
    public static List<Patient> Patients { get; } =
    [
        new Patient { Id = 1, Name = "Алиса Иванова", BirthDate = new DateTime(2000, 5, 15), Phone = "+996111222333", Email = "alice@example.com", BloodType = "O+" },
        new Patient { Id = 2, Name = "Бек Садыков", BirthDate = new DateTime(1998, 9, 3), Phone = "+996555123456", Email = "bek@example.com", BloodType = "A+" }
    ];

    public static List<SpravkaType> SpravkaTypes { get; } =
    [
        new SpravkaType { Id = 1, Name = "Справка 083", Price = 100m, ValidDays = 30 },
        new SpravkaType { Id = 2, Name = "Санитарная книжка", Price = 750m, ValidDays = 365 },
        new SpravkaType { Id = 3, Name = "Справка в бассейн", Price = 200m, ValidDays = 180 },
        new SpravkaType { Id = 4, Name = "Справка для спортсекции", Price = 250m, ValidDays = 180 }
    ];

    public static List<Spravka> Spravki { get; } =
    [
        new Spravka { Id = 1, PatientId = 1, SpravkaTypeId = 1, Name = "Справка 083", Price = 100m, IssuedAt = new DateTime(2026, 1, 10) },
        new Spravka { Id = 2, PatientId = 1, SpravkaTypeId = 3, Name = "Справка в бассейн", Price = 200m, IssuedAt = new DateTime(2026, 1, 12) }
    ];

    public static List<Appointment> Appointments { get; } = [];

    public static int NextPatientId() => NextId(Patients.Select(item => item.Id));
    public static int NextSpravkaId() => NextId(Spravki.Select(item => item.Id));
    public static int NextSpravkaTypeId() => NextId(SpravkaTypes.Select(item => item.Id));
    public static int NextAppointmentId() => NextId(Appointments.Select(item => item.Id));

    private static int NextId(IEnumerable<int> ids) => ids.DefaultIfEmpty(0).Max() + 1;
}