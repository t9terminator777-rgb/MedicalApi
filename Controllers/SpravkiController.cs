using MedicalApi.Data;
using MedicalApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpravkiController(MedicalDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Spravka>>> GetAll(
        [FromQuery] int? patientId,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] string? sort,
        [FromQuery] decimal? minPrice,
        CancellationToken cancellationToken,
        [FromQuery] string? order = "asc")
    {
        var query = db.Spravki.AsNoTracking();
        if (patientId.HasValue)
            query = query.Where(item => item.PatientId == patientId.Value);
        if (minPrice.HasValue)
            query = query.Where(item => item.Price >= minPrice.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var escapedSearch = search.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var pattern = $"%{escapedSearch}%";
            query = query.Where(item => EF.Functions.ILike(item.Name, pattern, "\\"));
        }

        var descending = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy ?? sort ?? "id").ToLowerInvariant() switch
        {
            "price" => (descending ? query.OrderByDescending(item => item.Price) : query.OrderBy(item => item.Price))
                .ThenBy(item => item.Id),
            "date" => (descending ? query.OrderByDescending(item => item.IssuedAt) : query.OrderBy(item => item.IssuedAt))
                .ThenBy(item => item.Id),
            _ => descending ? query.OrderByDescending(item => item.Id) : query.OrderBy(item => item.Id)
        };

        return Ok(await query.ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Spravka>> GetById(int id, CancellationToken cancellationToken)
    {
        var spravka = await db.Spravki.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return spravka is null ? NotFound() : Ok(spravka);
    }

    [HttpPost]
    public async Task<ActionResult<Spravka>> Create(SpravkaRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var issuedAt = request.IssuedAt ?? now;
        var spravka = new Spravka();
        var validationError = await SetSpravkaFields(spravka, request, issuedAt, now, cancellationToken);
        if (validationError is not null)
            return validationError;

        db.Spravki.Add(spravka);
        var saveError = await SaveSpravka(request, cancellationToken);
        if (saveError is not null)
            return saveError;

        return CreatedAtAction(nameof(GetById), new { id = spravka.Id }, spravka);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Spravka>> Update(int id, SpravkaRequest request, CancellationToken cancellationToken)
    {
        var spravka = await db.Spravki.FindAsync([id], cancellationToken);
        if (spravka is null)
            return NotFound(new { message = $"Справка с Id {id} не найдена." });

        var issuedAt = request.IssuedAt ?? spravka.IssuedAt.ToDateTime(TimeOnly.MinValue);
        var validationError = await SetSpravkaFields(spravka, request, issuedAt, DateTime.Now, cancellationToken);
        if (validationError is not null)
            return validationError;

        try
        {
            var saveError = await SaveSpravka(request, cancellationToken);
            if (saveError is not null)
                return saveError;
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound(new { message = $"Справка с Id {id} не найдена." });
        }

        return Ok(spravka);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var affectedRows = await db.Spravki.Where(item => item.Id == id).ExecuteDeleteAsync(cancellationToken);
        return affectedRows == 0 ? NotFound(new { message = $"Справка с Id {id} не найдена." }) : NoContent();
    }

    private async Task<BadRequestObjectResult?> SetSpravkaFields(
        Spravka spravka, SpravkaRequest request, DateTime issuedAt, DateTime now, CancellationToken cancellationToken)
    {
        if (!await db.Patients.AnyAsync(item => item.Id == request.PatientId, cancellationToken))
            return BadRequest(new { message = $"Пациент с Id {request.PatientId} не найден." });

        var type = await db.SpravkaTypes.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == request.SpravkaTypeId, cancellationToken);
        if (type is null)
            return BadRequest(new { message = $"Вид справки с Id {request.SpravkaTypeId} не найден." });
        if (issuedAt > now)
            return BadRequest(new { message = "Дата выдачи не может находиться в будущем." });

        spravka.PatientId = request.PatientId;
        spravka.SpravkaTypeId = type.Id;
        spravka.Name = type.Name;
        spravka.Price = type.Price;
        spravka.IssuedAt = DateOnly.FromDateTime(issuedAt);
        return null;
    }

    private async Task<BadRequestObjectResult?> SaveSpravka(SpravkaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: "FK_Spravki_Patients_PatientId" })
        {
            return BadRequest(new { message = $"Пациент с Id {request.PatientId} не найден." });
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.ForeignKeyViolation, ConstraintName: "FK_Spravki_SpravkaTypes_SpravkaTypeId" })
        {
            return BadRequest(new { message = $"Вид справки с Id {request.SpravkaTypeId} не найден." });
        }
    }
}
