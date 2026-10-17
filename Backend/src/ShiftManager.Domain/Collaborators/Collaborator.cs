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

    public string Name { get; private set; }

    public string Email { get; private set; }

    public string CountryCode { get; private set; }

    public bool IsActive { get; private set; } = true;

    public static Result<Collaborator> Create(string? name, string? email, string? countryCode)
    {
        var fields = Normalize(name, email, countryCode);

        return fields.IsFailure
            ? Result<Collaborator>.Failure(fields.Errors)
            : new Collaborator(Guid.NewGuid(), fields.Value.Name, fields.Value.Email, fields.Value.CountryCode);
    }

    public Result Update(string? name, string? email, string? countryCode)
    {
        var fields = Normalize(name, email, countryCode);

        if (fields.IsFailure)
        {
            return Result.Failure(fields.Errors);
        }

        Name = fields.Value.Name;
        Email = fields.Value.Email;
        CountryCode = fields.Value.CountryCode;
        return Result.Success();
    }

    public void Deactivate() => IsActive = false;

    private static Result<Fields> Normalize(string? name, string? email, string? countryCode)
    {
        var fields = new Fields(
            name?.Trim() ?? string.Empty,
            email?.Trim().ToLowerInvariant() ?? string.Empty,
            countryCode?.Trim().ToUpperInvariant() ?? string.Empty);

        var errors = new List<Error>();

        if (fields.Name.Length is 0 or > NameMaxLength)
        {
            errors.Add(CollaboratorErrors.InvalidName);
        }

        if (!IsValidEmail(fields.Email))
        {
            errors.Add(CollaboratorErrors.InvalidEmail);
        }

        if (!IsAlpha2Code(fields.CountryCode))
        {
            errors.Add(CollaboratorErrors.InvalidCountry);
        }

        return errors.Count > 0 ? Result<Fields>.Failure(errors) : fields;
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

    private readonly record struct Fields(string Name, string Email, string CountryCode);
}
