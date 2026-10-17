using ShiftManager.Application.Abstractions;
using ShiftManager.Domain.Collaborators;

namespace ShiftManager.UnitTests.Application;

internal sealed class FakeCollaboratorRepository : ICollaboratorRepository
{
    public List<Collaborator> Stored { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<Collaborator>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Collaborator>>(Stored.Where(c => c.IsActive).ToList());

    public Task<Collaborator?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Stored.FirstOrDefault(c => c.Id == id));

    public Task<bool> EmailExistsAsync(string email, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult(Stored.Any(c => c.Email == email && c.Id != excludingId));

    public void Add(Collaborator collaborator) => Stored.Add(collaborator);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }

    public Collaborator Seed(string name, string email, string countryCode)
    {
        var collaborator = Collaborator.Create(name, email, countryCode).Value;
        Stored.Add(collaborator);
        return collaborator;
    }
}

internal sealed class FakeCountryCatalog(bool? answer = true) : ICountryCatalog
{
    public Task<bool?> IsKnownAsync(string countryCode, CancellationToken cancellationToken) =>
        Task.FromResult(answer);
}
