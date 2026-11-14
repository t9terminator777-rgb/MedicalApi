using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpravkaTypesController(MedicalDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpravkaType>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await db.SpravkaTypes.AsNoTracking().OrderBy(item => item.Id).ToListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpravkaType>> GetById(int id, CancellationToken cancellationToken)
    {
        var type = await db.SpravkaTypes.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return type is null ? NotFound() : Ok(type);
    }

    [HttpPost]
    public async Task<ActionResult<SpravkaType>> Create(SpravkaTypeRequest request, CancellationToken cancellationToken)
    {
        var type = new SpravkaType { Name = request.Name, Price = request.Price, ValidDays = request.ValidDays };
        db.SpravkaTypes.Add(type);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = type.Id }, type);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SpravkaType>> Update(int id, SpravkaTypeRequest request, CancellationToken cancellationToken)
    {
        var type = await db.SpravkaTypes.FindAsync([id], cancellationToken);
        if (type is null)
            return NotFound(new { message = $"Вид справки с Id {id} не найден." });

        type.Name = request.Name;
        type.Price = request.Price;
        type.ValidDays = request.ValidDays;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound(new { message = $"Вид справки с Id {id} не найден." });
        }
        return Ok(type);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var type = await db.SpravkaTypes.FindAsync([id], cancellationToken);
        if (type is null)
            return NotFound(new { message = $"Вид справки с Id {id} не найден." });
        if (await db.Spravki.AnyAsync(item => item.SpravkaTypeId == id, cancellationToken))
            return UsedType();

        db.SpravkaTypes.Remove(type);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound(new { message = $"Вид справки с Id {id} не найден." });
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: "FK_Spravki_SpravkaTypes_SpravkaTypeId" })
        {
            return UsedType();
        }
        return NoContent();
    }

    private ConflictObjectResult UsedType() =>
        Conflict(new { message = "Нельзя удалить вид справки, который уже использован." });
}
