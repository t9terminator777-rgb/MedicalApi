using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpravkiController : ControllerBase
{
    // Можно отфильтровать по пациенту или части названия и отсортировать по цене/дате.
    [HttpGet]
    public ActionResult<IEnumerable<Spravka>> GetAll(
        [FromQuery] int? patientId,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? order = "asc")
    {
        IEnumerable<Spravka> result = ClinicData.Spravki;
        if (patientId.HasValue)
            result = result.Where(item => item.PatientId == patientId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            result = result.Where(item => item.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        var descending = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);
        result = (sortBy ?? "id").ToLowerInvariant() switch
        {
            "price" => descending ? result.OrderByDescending(item => item.Price) : result.OrderBy(item => item.Price),
            "date" => descending ? result.OrderByDescending(item => item.IssuedAt) : result.OrderBy(item => item.IssuedAt),
            _ => descending ? result.OrderByDescending(item => item.Id) : result.OrderBy(item => item.Id)
        };

        return Ok(result.ToList());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Spravka> GetById(int id)
    {
        var spravka = ClinicData.Spravki.FirstOrDefault(item => item.Id == id);
        return spravka is null ? NotFound() : Ok(spravka);
    }

    [HttpPost]
    public ActionResult<Spravka> Create(SpravkaRequest request)
    {
        var patient = ClinicData.Patients.FirstOrDefault(item => item.Id == request.PatientId);
        if (patient is null)
            return BadRequest(new { message = $"Пациент с Id {request.PatientId} не найден." });

        var type = ClinicData.SpravkaTypes.FirstOrDefault(item => item.Id == request.SpravkaTypeId);
        if (type is null)
            return BadRequest(new { message = $"Вид справки с Id {request.SpravkaTypeId} не найден." });

        var issuedAt = request.IssuedAt ?? DateTime.Now;
        if (issuedAt > DateTime.Now)
            return BadRequest(new { message = "Дата выдачи не может находиться в будущем." });

        var spravka = new Spravka
        {
            Id = ClinicData.NextSpravkaId(),
            PatientId = patient.Id,
            SpravkaTypeId = type.Id,
            Name = type.Name,
            Price = type.Price,
            IssuedAt = issuedAt
        };
        ClinicData.Spravki.Add(spravka);
        return CreatedAtAction(nameof(GetById), new { id = spravka.Id }, spravka);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Spravka> Update(int id, SpravkaRequest request)
    {
        var spravka = ClinicData.Spravki.FirstOrDefault(item => item.Id == id);
        if (spravka is null)
            return NotFound(new { message = $"Справка с Id {id} не найдена." });
        var patient = ClinicData.Patients.FirstOrDefault(item => item.Id == request.PatientId);
        if (patient is null)
            return BadRequest(new { message = $"Пациент с Id {request.PatientId} не найден." });
        var type = ClinicData.SpravkaTypes.FirstOrDefault(item => item.Id == request.SpravkaTypeId);
        if (type is null)
            return BadRequest(new { message = $"Вид справки с Id {request.SpravkaTypeId} не найден." });

        var issuedAt = request.IssuedAt ?? spravka.IssuedAt;
        if (issuedAt > DateTime.Now)
            return BadRequest(new { message = "Дата выдачи не может находиться в будущем." });

        spravka.PatientId = patient.Id;
        spravka.SpravkaTypeId = type.Id;
        spravka.Name = type.Name;
        spravka.Price = type.Price;
        spravka.IssuedAt = issuedAt;
        return Ok(spravka);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var spravka = ClinicData.Spravki.FirstOrDefault(item => item.Id == id);
        if (spravka is null)
            return NotFound(new { message = $"Справка с Id {id} не найдена." });

        ClinicData.Spravki.Remove(spravka);
        return NoContent();
    }
}