using System.Diagnostics.CodeAnalysis;

namespace Beblang;

public class Result<TOk, TError> : IEquatable<Result<TOk, TError>>
{
    private readonly bool _isOk;
    private readonly TOk? _result;
    private readonly TError? _error;
    
    private Result(TOk? result, TError? error, bool isOk)
    {
        _result = result;
        _error = error;
        _isOk = isOk;
    }
    
    public static Result<TOk, TError> Ok(TOk result) => new(result, default, isOk: true);
    public static Result<TOk, TError> Error(TError error) => new(default, error, isOk: false);
    
    public bool IsOk([NotNullWhen(true)] out TOk? result)
    {
        result = _result;
        return _isOk;
    }
    
    public bool IsError([NotNullWhen(true)] out TError? error, [NotNullWhen(false)] out TOk? result)
    {
        error = _error;
        result = _result;
        return !_isOk;
    }
    
    public static implicit operator Result<TOk, TError>(TOk result) => Ok(result);
    public static implicit operator Result<TOk, TError>(TError result) => Error(result);
    
    public static bool operator ==(Result<TOk, TError> left, Result<TOk, TError> right) => Equals(left, right);
    public static bool operator !=(Result<TOk, TError> left, Result<TOk, TError> right) => !(left == right);

    public bool Equals(Result<TOk, TError>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _isOk == other._isOk && EqualityComparer<TOk?>.Default.Equals(_result, other._result) && EqualityComparer<TError?>.Default.Equals(_error, other._error);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Result<TOk, TError>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_isOk, _result, _error);
    }
}