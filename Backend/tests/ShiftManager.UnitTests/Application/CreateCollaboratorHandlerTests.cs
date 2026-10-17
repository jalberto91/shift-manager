using ShiftManager.Application.Collaborators.CreateCollaborator;
using ShiftManager.Application.Collaborators.ListCollaborators;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Common;

namespace ShiftManager.UnitTests.Application;

public class CreateCollaboratorHandlerTests
{
    private readonly FakeCollaboratorRepository _repository = new();

    private Task<Result<CollaboratorDto>> Create(
        string? name, string? email, string? country, bool? countryKnown = true) =>
        new CreateCollaboratorHandler(_repository, new FakeCountryCatalog(countryKnown))
            .Handle(new CreateCollaboratorCommand(name, email, country), CancellationToken.None).AsTask();

    [Fact]
    public async Task Persists_and_returns_the_normalized_collaborator()
    {
        var result = await Create(" Ana ", "ANA@Example.com", "es");

        Assert.True(result.IsSuccess);
        Assert.Equal("ana@example.com", result.Value.Email);
        Assert.Equal("ES", result.Value.CountryCode);
        Assert.Single(_repository.Stored);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Rejects_invalid_shape_without_saving()
    {
        var result = await Create("", "not-an-email", "ESP");

        Assert.True(result.IsFailure);
        Assert.Empty(_repository.Stored);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Rejects_a_country_the_catalog_does_not_know()
    {
        var result = await Create("Ana", "ana@example.com", "ZZ", countryKnown: false);

        Assert.Equal(CollaboratorErrors.UnknownCountry, result.Error);
        Assert.Empty(_repository.Stored);
    }

    [Fact]
    public async Task Accepts_the_country_when_the_catalog_does_not_answer()
    {
        var result = await Create("Ana", "ana@example.com", "ES", countryKnown: null);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Rejects_an_email_already_in_use()
    {
        _repository.Seed("Bruno", "ana@example.com", "BR");

        var result = await Create("Ana", "ANA@example.com", "ES");

        Assert.Equal(CollaboratorErrors.EmailTaken, result.Error);
        Assert.Single(_repository.Stored);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Rejects_the_email_of_a_deactivated_collaborator()
    {
        _repository.Seed("Bruno", "ana@example.com", "BR").Deactivate();

        var result = await Create("Ana", "ana@example.com", "ES");

        Assert.Equal(CollaboratorErrors.EmailTaken, result.Error);
    }
}
