using MedicalApi.Models;
using MedicalApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GosReestrController(IGosReestr gosReestr) : ControllerBase
{
    [HttpGet("{pin}")]
    public async Task<ActionResult<Citizen>> GetByPin(string pin, CancellationToken cancellationToken)
    {
        if (!Citizen.IsValidPin(pin))
            return BadRequest(new { message = "ПИН должен содержать ровно 14 цифр." });

        try
        {
            var citizen = await gosReestr.FindAsync(pin, cancellationToken);
            return citizen is null
                ? NotFound(new { message = "ПИН не найден в госреестре." })
                : Ok(citizen);
        }
        catch (GosReestrUnavailableException)
        {
            return StatusCode(503, "Госреестр временно недоступен. Попробуйте через минуту или введите данные вручную");
        }
    }
}
