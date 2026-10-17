using ShiftManager.Domain.Collaborators;

namespace ShiftManager.UnitTests.Domain;

public class CollaboratorTests
{
    [Fact]
    public void Create_normalizes_name_email_and_country()
    {
        var result = Collaborator.Create("  Ana Perez ", "  Ana.Perez@Example.COM ", " es ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Ana Perez", result.Value.Name);
        Assert.Equal("ana.perez@example.com", result.Value.Email);
        Assert.Equal("ES", result.Value.CountryCode);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_a_missing_name(string? name)
    {
        var result = Collaborator.Create(name, "ana@example.com", "ES");

        Assert.Equal([CollaboratorErrors.InvalidName], result.Errors);
    }

    [Fact]
    public void Create_rejects_a_name_longer_than_the_limit()
    {
        var result = Collaborator.Create(new string('a', Collaborator.NameMaxLength + 1), "ana@example.com", "ES");

        Assert.Equal([CollaboratorErrors.InvalidName], result.Errors);
    }

    [Fact]
    public void Create_accepts_a_name_at_the_limit()
    {
        var result = Collaborator.Create(new string('a', Collaborator.NameMaxLength), "ana@example.com", "ES");

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ana")]
    [InlineData("@example.com")]
    [InlineData("ana@")]
    [InlineData("ana@example")]
    [InlineData("ana@@example.com")]
    [InlineData("ana@example.")]
    [InlineData("ana perez@example.com")]
    public void Create_rejects_an_invalid_email(string? email)
    {
        var result = Collaborator.Create("Ana", email, "ES");

        Assert.Equal([CollaboratorErrors.InvalidEmail], result.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("E")]
    [InlineData("ESP")]
    [InlineData("E1")]
    [InlineData("ÉÑ")]
    public void Create_rejects_an_invalid_country(string? country)
    {
        var result = Collaborator.Create("Ana", "ana@example.com", country);

        Assert.Equal([CollaboratorErrors.InvalidCountry], result.Errors);
    }

    [Fact]
    public void Create_reports_every_invalid_field_at_once()
    {
        var result = Collaborator.Create("", "nope", "XYZ");

        Assert.Equal(
            [CollaboratorErrors.InvalidName, CollaboratorErrors.InvalidEmail, CollaboratorErrors.InvalidCountry],
            result.Errors);
    }

    [Fact]
    public void Create_starts_active()
    {
        var collaborator = Collaborator.Create("Ana", "ana@example.com", "ES").Value;

        Assert.True(collaborator.IsActive);
    }

    [Fact]
    public void Update_replaces_and_normalizes_every_field_keeping_the_id()
    {
        var collaborator = Collaborator.Create("Ana", "ana@example.com", "ES").Value;
        var id = collaborator.Id;

        var result = collaborator.Update("  Bea Gomez ", " Bea@Example.COM ", " ar ");

        Assert.True(result.IsSuccess);
        Assert.Equal(id, collaborator.Id);
        Assert.Equal("Bea Gomez", collaborator.Name);
        Assert.Equal("bea@example.com", collaborator.Email);
        Assert.Equal("AR", collaborator.CountryCode);
    }

    [Fact]
    public void Update_reports_every_invalid_field_and_changes_nothing()
    {
        var collaborator = Collaborator.Create("Ana", "ana@example.com", "ES").Value;

        var result = collaborator.Update("", "nope", "XYZ");

        Assert.Equal(
            [CollaboratorErrors.InvalidName, CollaboratorErrors.InvalidEmail, CollaboratorErrors.InvalidCountry],
            result.Errors);
        Assert.Equal("Ana", collaborator.Name);
        Assert.Equal("ana@example.com", collaborator.Email);
        Assert.Equal("ES", collaborator.CountryCode);
    }

    [Fact]
    public void Deactivate_is_idempotent()
    {
        var collaborator = Collaborator.Create("Ana", "ana@example.com", "ES").Value;

        collaborator.Deactivate();
        collaborator.Deactivate();

        Assert.False(collaborator.IsActive);
    }
}
