using ShiftManager.Domain.Common;

namespace ShiftManager.Domain.Collaborators;

public sealed class Collaborator
{
    public const int NameMaxLength = 200;
    public const int EmailMaxLength = 254;
    public const int CountryCodeLength = 2;

    private Collaborator(Guid id, string name, string email, string countryCode)
    {
        Id = id;
        Name = name;
        Email = email;
        CountryCode = countryCode;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Email { get; }

    public string CountryCode { get; }

    public static Result<Collaborator> Create(string? name, string? email, string? countryCode)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        var normalizedEmail = email?.Trim().ToLowerInvariant() ?? string.Empty;
        var normalizedCountry = countryCode?.Trim().ToUpperInvariant() ?? string.Empty;

        var errors = new List<Error>();

        if (normalizedName.Length is 0 or > NameMaxLength)
        {
            errors.Add(CollaboratorErrors.InvalidName);
        }

        if (!IsValidEmail(normalizedEmail))
        {
            errors.Add(CollaboratorErrors.InvalidEmail);
        }

        if (!IsAlpha2Code(normalizedCountry))
        {
            errors.Add(CollaboratorErrors.InvalidCountry);
        }

        return errors.Count > 0
            ? Result<Collaborator>.Failure(errors)
            : new Collaborator(Guid.NewGuid(), normalizedName, normalizedEmail, normalizedCountry);
    }

    private static bool IsValidEmail(string email)
    {
        if (email.Length is 0 or > EmailMaxLength || email.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var at = email.IndexOf('@');
        var dotAfterAt = email.IndexOf('.', Math.Max(at, 0));

        return at > 0
            && at == email.LastIndexOf('@')
            && dotAfterAt > at + 1
            && !email.EndsWith('.');
    }

    private static bool IsAlpha2Code(string code) =>
        code.Length == CountryCodeLength && code.All(char.IsAsciiLetterUpper);
}
