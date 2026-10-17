namespace ShiftManager.Domain.Common;

public class Result
{
    protected Result(IReadOnlyList<Error> errors) => Errors = errors;

    public bool IsSuccess => Errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<Error> Errors { get; }

    public Error? Error => Errors.Count > 0 ? Errors[0] : null;

    public static Result Success() => new([]);

    public static Result Failure(Error error) => new([error]);

    public static Result Failure(IEnumerable<Error> errors) => new(NonEmpty(errors));

    protected static IReadOnlyList<Error> NonEmpty(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        return list.Count > 0
            ? list
            : throw new ArgumentException("A failure needs at least one error.", nameof(errors));
    }
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base([]) => _value = value;

    private Result(IReadOnlyList<Error> errors) : base(errors)
    {
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed result has no value.");

    public static Result<T> Success(T value) => new(value);

    public new static Result<T> Failure(Error error) => new([error]);

    public new static Result<T> Failure(IEnumerable<Error> errors) => new(NonEmpty(errors));

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
