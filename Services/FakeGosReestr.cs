using MedicalApi.Models;

namespace MedicalApi.Services;

public sealed class FakeGosReestr : IGosReestr
{
    private readonly Dictionary<string, Citizen> _citizens = new()
    {
        ["21203199850123"] = new("21203199850123", "Айбек Асанов", new DateOnly(1998, 3, 12)),
        ["12507200350456"] = new("12507200350456", "Нургуль Токтосунова", new DateOnly(2003, 7, 25)),
        ["20211198500789"] = new("20211198500789", "Бакыт Жумабаев", new DateOnly(1985, 11, 2)),
        ["13001200150321"] = new("13001200150321", "Айпери Садыкова", new DateOnly(2001, 1, 30)),
        ["21409200650654"] = new("21409200650654", "Тимур Ким", new DateOnly(2006, 9, 14))
    };

    public async Task<Citizen?> FindAsync(string pin, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1500, cancellationToken);
        if (pin == "20101200000000")
            throw new GosReestrUnavailableException();

        return _citizens.GetValueOrDefault(pin);
    }
}
