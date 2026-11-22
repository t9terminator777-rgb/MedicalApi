using MedicalApi.Data;
using MedicalApi.Models;
using MedicalApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController(MedicalDbContext db, IGosReestr gosReestr) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Patient>> GetAll() => Ok(db.Patients.ToList());

    [HttpGet("{id:int}")]
    public ActionResult<Patient> GetById(int id)
    {
        var patient = db.Patients.FirstOrDefault(item => item.Id == id);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("{id:int}/spravki")]
    public ActionResult<IEnumerable<Spravka>> GetPatientSpravki(int id)
    {
        if (!db.Patients.Any(item => item.Id == id))
            return NotFound(new { message = $"Пациент с Id {id} не найден." });

        return Ok(ClinicData.Spravki.Where(item => item.PatientId == id).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<Patient>> Create(CreatePatientRequest request, CancellationToken cancellationToken)
    {
        if (!Citizen.IsValidPin(request.Pin))
            return BadRequest(new { message = "ПИН должен содержать ровно 14 цифр." });

        Citizen? citizen;
        try
        {
            citizen = await gosReestr.FindAsync(request.Pin!, cancellationToken);
        }
        catch (GosReestrUnavailableException)
        {
            return StatusCode(503, new { message = "Госреестр временно недоступен. Попробуйте позже." });
        }

        if (citizen is null)
            return BadRequest(new { message = "ПИН не найден в госреестре." });

        if (await db.Patients.AnyAsync(patient => patient.Pin == citizen.Pin, cancellationToken))
            return Conflict(new { message = "Пациент с таким ПИН уже зарегистрирован." });

        var patient = new Patient
        {
            Pin = citizen.Pin,
            Name = citizen.Name,
            BirthDate = citizen.BirthDate.ToDateTime(TimeOnly.MinValue),
            Phone = request.Phone,
            Email = request.Email,
            BloodType = request.BloodType
        };

        db.Patients.Add(patient);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException error) when (error.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Patients_Pin" })
        {
            return Conflict(new { message = "Пациент с таким ПИН уже зарегистрирован." });
        }

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Patient> Update(int id, Patient input)
    {
        var patient = db.Patients.FirstOrDefault(item => item.Id == id);
        if (patient is null)
            return NotFound(new { message = $"Пациент с Id {id} не найден." });
        if (input.BirthDate == default || input.BirthDate.Date > DateTime.Today)
            return BadRequest(new { message = "Дата рождения не может быть пустой или находиться в будущем." });

        patient.Name = input.Name;
        patient.BirthDate = input.BirthDate;
        patient.Phone = input.Phone;
        patient.Email = input.Email;
        patient.BloodType = input.BloodType;
        db.SaveChanges();
        return Ok(patient);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var patient = db.Patients.FirstOrDefault(item => item.Id == id);
        if (patient is null)
            return NotFound(new { message = $"Пациент с Id {id} не найден." });

        db.Patients.Remove(patient);
        db.SaveChanges();
        ClinicData.Spravki.RemoveAll(item => item.PatientId == id);
        ClinicData.Appointments.RemoveAll(item => item.PatientId == id);
        return NoContent();
    }
}
