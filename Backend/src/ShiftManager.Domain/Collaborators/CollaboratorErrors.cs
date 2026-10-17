using ShiftManager.Domain.Common;

namespace ShiftManager.Domain.Collaborators;

public static class CollaboratorErrors
{
    public static readonly Error InvalidName = Error.Validation(
        "collaborator.name", $"Name is required and must be at most {Collaborator.NameMaxLength} characters.");

    public static readonly Error InvalidEmail = Error.Validation(
        "collaborator.email", "Email is not a valid address.");

    public static readonly Error InvalidCountry = Error.Validation(
        "collaborator.country", "Country must be an ISO 3166-1 alpha-2 code, for example ES.");

    public static readonly Error UnknownCountry = Error.Validation(
        "collaborator.country-unknown", "Country is not supported.");

    public static readonly Error EmailTaken = Error.Conflict(
        "collaborator.email-taken", "Another collaborator already uses this email.");

    public static readonly Error NotFound = Error.NotFound(
        "collaborator.not-found", "Collaborator was not found.");
}
