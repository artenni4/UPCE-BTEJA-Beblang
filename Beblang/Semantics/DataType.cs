using System.Diagnostics.CodeAnalysis;

namespace Beblang.Semantics;

public class DataType : IEquatable<DataType>
{
    private enum PrimitiveType
    {
        Integer,
        Real,
        String,
        Boolean,
        Array
    };

    private readonly IReadOnlyList<PrimitiveType> _type;

    private DataType(PrimitiveType type)
    {
        _type = new[] { type };
    }
    
    private DataType(IReadOnlyList<PrimitiveType> type)
    {
        _type = type;
    }

    public bool IsArray([NotNullWhen(true)] out DataType? ofType)
    {
        if (_type.Count == 1)
        {
            ofType = null;
            return false;
        }

        ofType = new DataType(_type.Skip(1).ToArray());
        return true;
    }
    
    public static DataType Integer { get; } = new(PrimitiveType.Integer);
    public static DataType Real { get; } = new(PrimitiveType.Real);
    public static DataType String { get; } = new(PrimitiveType.String);
    public static DataType Boolean { get; } = new(PrimitiveType.Boolean);
    public static DataType Array(DataType ofType)
    {
        if (ofType._type.Take(..^1).Any(t => t != PrimitiveType.Array))
        {
            throw new ArgumentException("Array definition must contain primitive type in the last position");
        }
        return new DataType(ofType._type.Prepend(PrimitiveType.Array).ToArray());
    }

    public override string ToString()
    {
        return _type switch
        {
            { Count: 1 } => _type[0] switch
            {
                PrimitiveType.Integer => "integer",
                PrimitiveType.Real => "real",
                PrimitiveType.String => "string",
                PrimitiveType.Boolean => "boolean",
                PrimitiveType.Array => throw new InvalidOperationException("Array must have at least one type"),
                _ => throw new NotSupportedException()
            },
            _ => $"array of {new DataType(_type.Skip(1).ToArray())}"
        };
    }

    public static bool operator ==(DataType left, DataType right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DataType left, DataType right)
    {
        return !(left == right);
    }

    public bool Equals(DataType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _type.Equals(other._type);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DataType)obj);
    }

    public override int GetHashCode()
    {
        return _type.GetHashCode();
    }
}