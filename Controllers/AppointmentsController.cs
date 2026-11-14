using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MedicalApi.Controllers;

// Дополнительный модуль клиники: запись пациента на приём к врачу.
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController(MedicalDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAll(
        [FromQuery] int? patientId, CancellationToken cancellationToken)
    {
        var appointments = db.Appointments.AsNoTracking();
        if (patientId.HasValue)
            appointments = appointments.Where(item => item.PatientId == patientId.Value);
        return Ok(await appointments.OrderBy(item => item.ScheduledAt).ThenBy(item => item.Id)
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Appointment>> GetById(int id, CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<Appointment>> Create(AppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = new Appointment();
        var validationError = await SetAppointmentFields(appointment, request, cancellationToken);
        if (validationError is not null)
            return validationError;

        db.Appointments.Add(appointment);
        var saveError = await SaveAppointment(request.PatientId, cancellationToken);
        if (saveError is not null)
            return saveError;

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Appointment>> Update(int id, AppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments.FindAsync([id], cancellationToken);
        if (appointment is null)
            return NotFound(new { message = $"Запись с Id {id} не найдена." });

        var validationError = await SetAppointmentFields(appointment, request, cancellationToken);
        if (validationError is not null)
            return validationError;

        try
        {
            var saveError = await SaveAppointment(request.PatientId, cancellationToken);
            if (saveError is not null)
                return saveError;
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound(new { message = $"Запись с Id {id} не найдена." });
        }
        return Ok(appointment);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var affectedRows = await db.Appointments.Where(item => item.Id == id).ExecuteDeleteAsync(cancellationToken);
        return affectedRows == 0 ? NotFound(new { message = $"Запись с Id {id} не найдена." }) : NoContent();
    }

    private async Task<BadRequestObjectResult?> SetAppointmentFields(
        Appointment appointment, AppointmentRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Patients.AnyAsync(item => item.Id == request.PatientId, cancellationToken))
            return BadRequest(new { message = $"Пациент с Id {request.PatientId} не найден." });

        var scheduledAt = request.ScheduledAt.ToUniversalTime();
        if (scheduledAt < DateTime.UtcNow)
            return BadRequest(new { message = "Укажите дату и время приёма в будущем." });

        appointment.PatientId = request.PatientId;
        appointment.DoctorName = request.DoctorName;
        appointment.ScheduledAt = scheduledAt;
        appointment.Reason = request.Reason;
        return null;
    }

    private async Task<BadRequestObjectResult?> SaveAppointment(int patientId, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: "FK_Appointments_Patients_PatientId" })
        {
            return BadRequest(new { message = $"Пациент с Id {patientId} не найден." });
        }
    }
}
