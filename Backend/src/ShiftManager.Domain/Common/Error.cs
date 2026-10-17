namespace ShiftManager.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, object?>? Extensions = null)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message, IReadOnlyDictionary<string, object?>? extensions = null) =>
        new(code, message, ErrorType.Conflict, extensions);

    // Extensions queda fuera de la igualdad: un diccionario se compara por referencia.
    public bool Equals(Error? other) =>
        other is not null && Code == other.Code && Message == other.Message && Type == other.Type;

    public override int GetHashCode() => HashCode.Combine(Code, Message, Type);
}
