using MedicalApi.Models;

namespace MedicalApi.Services;

public interface IGosReestr
{
    Task<Citizen?> FindAsync(string pin, CancellationToken cancellationToken = default);
}
