using MedicalApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController(MedicalDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var patientsCount = await db.Patients.CountAsync(cancellationToken);
        var spravkiCount = await db.Spravki.CountAsync(cancellationToken);
        var total = await db.Spravki.SumAsync(spravka => spravka.Price, cancellationToken);
        return Ok(new { patientsCount, spravkiCount, total });
    }
}
