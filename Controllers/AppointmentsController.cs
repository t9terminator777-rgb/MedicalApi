using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApi.Controllers;

// Дополнительный модуль клиники: запись пациента на приём к врачу.
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Appointment>> GetAll([FromQuery] int? patientId)
    {
        var appointments = ClinicData.Appointments.AsEnumerable();
        if (patientId.HasValue)
            appointments = appointments.Where(item => item.PatientId == patientId.Value);
        return Ok(appointments.OrderBy(item => item.ScheduledAt).ToList());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Appointment> GetById(int id)
    {
        var appointment = ClinicData.Appointments.FirstOrDefault(item => item.Id == id);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpPost]
    public ActionResult<Appointment> Create(AppointmentRequest request)
    {
        var validation = ValidateRequest(request);
        if (validation is not null)
            return BadRequest(new { message = validation });

        var appointment = new Appointment
        {
            Id = ClinicData.NextAppointmentId(),
            PatientId = request.PatientId,
            DoctorName = request.DoctorName,
            ScheduledAt = request.ScheduledAt,
            Reason = request.Reason
        };
        ClinicData.Appointments.Add(appointment);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Appointment> Update(int id, AppointmentRequest request)
    {
        var appointment = ClinicData.Appointments.FirstOrDefault(item => item.Id == id);
        if (appointment is null)
            return NotFound(new { message = $"Запись с Id {id} не найдена." });

        var validation = ValidateRequest(request);
        if (validation is not null)
            return BadRequest(new { message = validation });

        appointment.PatientId = request.PatientId;
        appointment.DoctorName = request.DoctorName;
        appointment.ScheduledAt = request.ScheduledAt;
        appointment.Reason = request.Reason;
        return Ok(appointment);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var appointment = ClinicData.Appointments.FirstOrDefault(item => item.Id == id);
        if (appointment is null)
            return NotFound(new { message = $"Запись с Id {id} не найдена." });

        ClinicData.Appointments.Remove(appointment);
        return NoContent();
    }

    private static string? ValidateRequest(AppointmentRequest request)
    {
        if (!ClinicData.Patients.Any(item => item.Id == request.PatientId))
            return $"Пациент с Id {request.PatientId} не найден.";
        if (request.ScheduledAt == default || request.ScheduledAt < DateTime.Now)
            return "Укажите дату и время приёма в будущем.";
        return null;
    }
}