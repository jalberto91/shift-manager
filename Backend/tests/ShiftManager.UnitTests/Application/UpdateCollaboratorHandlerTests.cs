using ShiftManager.Application.Collaborators.ListCollaborators;
using ShiftManager.Application.Collaborators.UpdateCollaborator;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Common;

namespace ShiftManager.UnitTests.Application;

public class UpdateCollaboratorHandlerTests
{
    private readonly FakeCollaboratorRepository _repository = new();

    private Task<Result<CollaboratorDto>> Update(
        Guid id, string? name, string? email, string? country, bool? countryKnown = true) =>
        new UpdateCollaboratorHandler(_repository, new FakeCountryCatalog(countryKnown))
            .Handle(new UpdateCollaboratorCommand(id, name, email, country), CancellationToken.None).AsTask();

    [Fact]
    public async Task Updates_and_saves_the_collaborator()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");

        var result = await Update(ana.Id, "Ana María", "ana.maria@example.com", "MX");

        Assert.True(result.IsSuccess);
        Assert.Equal("Ana María", ana.Name);
        Assert.Equal("MX", result.Value.CountryCode);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Keeps_the_own_email_without_conflict()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");

        var result = await Update(ana.Id, "Ana María", "ana@example.com", "ES");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Fails_when_the_collaborator_does_not_exist()
    {
        var result = await Update(Guid.NewGuid(), "Ana", "ana@example.com", "ES");

        Assert.Equal(CollaboratorErrors.NotFound, result.Error);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Fails_when_the_collaborator_is_deactivated()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");
        ana.Deactivate();

        var result = await Update(ana.Id, "Ana María", "ana@example.com", "ES");

        Assert.Equal(CollaboratorErrors.NotFound, result.Error);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Rejects_an_email_used_by_another_collaborator()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");
        _repository.Seed("Bruno", "bruno@example.com", "BR");

        var result = await Update(ana.Id, "Ana", "bruno@example.com", "ES");

        Assert.Equal(CollaboratorErrors.EmailTaken, result.Error);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Rejects_a_country_the_catalog_does_not_know()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");

        var result = await Update(ana.Id, "Ana", "ana@example.com", "ZZ", countryKnown: false);

        Assert.Equal(CollaboratorErrors.UnknownCountry, result.Error);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Rejects_invalid_shape_without_saving()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");

        var result = await Update(ana.Id, "", "ana@example.com", "ES");

        Assert.True(result.IsFailure);
        Assert.Equal(0, _repository.SaveCount);
    }
}
