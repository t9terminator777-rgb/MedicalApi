using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController(MedicalDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetAll(string? name, CancellationToken cancellationToken)
    {
        var query = db.Patients.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(name))
        {
            var escapedName = name.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var pattern = $"%{escapedName}%";
            query = query.Where(patient => EF.Functions.ILike(patient.Name, pattern, "\\"));
        }

        return Ok(await query.OrderBy(patient => patient.Name).ThenBy(patient => patient.Id)
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Patient>> GetById(int id, CancellationToken cancellationToken)
    {
        var patient = await db.Patients.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("{id:int}/spravki")]
    public async Task<ActionResult<IEnumerable<Spravka>>> GetPatientSpravki(int id, CancellationToken cancellationToken)
    {
        if (!await db.Patients.AnyAsync(item => item.Id == id, cancellationToken))
            return NotFound(new { message = $"Пациент с Id {id} не найден." });

        var certificates = await db.Spravki.AsNoTracking().Where(item => item.PatientId == id)
            .OrderBy(item => item.Id).ToListAsync(cancellationToken);
        return Ok(certificates);
    }

    [HttpPost]
    public async Task<ActionResult<Patient>> Create(PatientRequest input, CancellationToken cancellationToken)
    {
        if (input.BirthDate == default || input.BirthDate > DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "Дата рождения не может быть пустой или находиться в будущем." });

        var patient = new Patient
        {
            Name = input.Name,
            BirthDate = input.BirthDate,
            Phone = input.Phone,
            Email = input.Email,
            BloodType = input.BloodType
        };
        db.Patients.Add(patient);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Patient>> Update(int id, PatientRequest input, CancellationToken cancellationToken)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (patient is null)
            return NotFound(new { message = $"Пациент с Id {id} не найден." });
        if (input.BirthDate == default || input.BirthDate > DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "Дата рождения не может быть пустой или находиться в будущем." });

        patient.Name = input.Name;
        patient.BirthDate = input.BirthDate;
        patient.Phone = input.Phone;
        patient.Email = input.Email;
        patient.BloodType = input.BloodType;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound(new { message = $"Пациент с Id {id} не найден." });
        }

        return Ok(patient);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (patient is null)
            return NotFound(new { message = $"Пациент с Id {id} не найден." });

        var restriction = new { message = "Нельзя удалить пациента, пока у него есть выданные справки." };
        if (await db.Spravki.AnyAsync(item => item.PatientId == id, cancellationToken))
            return BadRequest(restriction);

        db.Patients.Remove(patient);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound(new { message = $"Пациент с Id {id} не найден." });
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: "FK_Spravki_Patients_PatientId"
            })
        {
            return BadRequest(restriction);
        }

        return NoContent();
    }
}
