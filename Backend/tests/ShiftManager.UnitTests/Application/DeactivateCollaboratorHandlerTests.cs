using ShiftManager.Application.Collaborators.DeactivateCollaborator;
using ShiftManager.Application.Collaborators.ListCollaborators;
using ShiftManager.Domain.Collaborators;
using ShiftManager.Domain.Common;

namespace ShiftManager.UnitTests.Application;

public class DeactivateCollaboratorHandlerTests
{
    private readonly FakeCollaboratorRepository _repository = new();

    private ValueTask<Result> Deactivate(Guid id) =>
        new DeactivateCollaboratorHandler(_repository)
            .Handle(new DeactivateCollaboratorCommand(id), CancellationToken.None);

    [Fact]
    public async Task Deactivates_and_saves()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");

        var result = await Deactivate(ana.Id);

        Assert.True(result.IsSuccess);
        Assert.False(ana.IsActive);
        Assert.Equal(1, _repository.SaveCount);
    }

    [Fact]
    public async Task Is_idempotent()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");

        await Deactivate(ana.Id);
        var second = await Deactivate(ana.Id);

        Assert.True(second.IsSuccess);
        Assert.False(ana.IsActive);
    }

    [Fact]
    public async Task Fails_when_the_collaborator_does_not_exist()
    {
        var result = await Deactivate(Guid.NewGuid());

        Assert.Equal(CollaboratorErrors.NotFound, result.Error);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Deactivated_collaborators_leave_the_listing()
    {
        var ana = _repository.Seed("Ana", "ana@example.com", "ES");
        _repository.Seed("Bruno", "bruno@example.com", "BR");

        await Deactivate(ana.Id);
        var listed = await new ListCollaboratorsHandler(_repository)
            .Handle(new ListCollaboratorsQuery(), CancellationToken.None);

        Assert.Equal("Bruno", Assert.Single(listed).Name);
    }
}
