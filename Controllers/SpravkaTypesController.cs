using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpravkaTypesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<SpravkaType>> GetAll() => Ok(ClinicData.SpravkaTypes);

    [HttpGet("{id:int}")]
    public ActionResult<SpravkaType> GetById(int id)
    {
        var type = ClinicData.SpravkaTypes.FirstOrDefault(item => item.Id == id);
        return type is null ? NotFound() : Ok(type);
    }

    [HttpPost]
    public ActionResult<SpravkaType> Create(SpravkaType type)
    {
        type.Id = ClinicData.NextSpravkaTypeId();
        ClinicData.SpravkaTypes.Add(type);
        return CreatedAtAction(nameof(GetById), new { id = type.Id }, type);
    }

    [HttpPut("{id:int}")]
    public ActionResult<SpravkaType> Update(int id, SpravkaType input)
    {
        var type = ClinicData.SpravkaTypes.FirstOrDefault(item => item.Id == id);
        if (type is null)
            return NotFound(new { message = $"Вид справки с Id {id} не найден." });

        type.Name = input.Name;
        type.Price = input.Price;
        type.ValidDays = input.ValidDays;
        return Ok(type);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var type = ClinicData.SpravkaTypes.FirstOrDefault(item => item.Id == id);
        if (type is null)
            return NotFound(new { message = $"Вид справки с Id {id} не найден." });
        if (ClinicData.Spravki.Any(item => item.SpravkaTypeId == id))
            return Conflict(new { message = "Нельзя удалить вид справки, который уже использован." });

        ClinicData.SpravkaTypes.Remove(type);
        return NoContent();
    }
}