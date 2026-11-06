using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Patient>> GetAll() => Ok(ClinicData.Patients);

    [HttpGet("{id:int}")]
    public ActionResult<Patient> GetById(int id)
    {
        var patient = ClinicData.Patients.FirstOrDefault(item => item.Id == id);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("{id:int}/spravki")]
    public ActionResult<IEnumerable<Spravka>> GetPatientSpravki(int id)
    {
        if (!ClinicData.Patients.Any(item => item.Id == id))
            return NotFound(new { message = $"Пациент с Id {id} не найден." });

        return Ok(ClinicData.Spravki.Where(item => item.PatientId == id).ToList());
    }

    [HttpPost]
    public ActionResult<Patient> Create(Patient patient)
    {
        if (patient.BirthDate == default || patient.BirthDate.Date > DateTime.Today)
            return BadRequest(new { message = "Дата рождения не может быть пустой или находиться в будущем." });

        patient.Id = ClinicData.NextPatientId();
        ClinicData.Patients.Add(patient);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Patient> Update(int id, Patient input)
    {
        var patient = ClinicData.Patients.FirstOrDefault(item => item.Id == id);
        if (patient is null)
            return NotFound(new { message = $"Пациент с Id {id} не найден." });
        if (input.BirthDate == default || input.BirthDate.Date > DateTime.Today)
            return BadRequest(new { message = "Дата рождения не может быть пустой или находиться в будущем." });

        patient.Name = input.Name;
        patient.BirthDate = input.BirthDate;
        patient.Phone = input.Phone;
        patient.Email = input.Email;
        patient.BloodType = input.BloodType;
        return Ok(patient);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var patient = ClinicData.Patients.FirstOrDefault(item => item.Id == id);
        if (patient is null)
            return NotFound(new { message = $"Пациент с Id {id} не найден." });

        ClinicData.Patients.Remove(patient);
        ClinicData.Spravki.RemoveAll(item => item.PatientId == id);
        ClinicData.Appointments.RemoveAll(item => item.PatientId == id);
        return NoContent();
    }
}