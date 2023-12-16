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
        Array,
        Void,
    };
    
    public record ArrayInfo(DataType OfType, int Size);

    private readonly PrimitiveType _type;
    private readonly ArrayInfo? _arrayInfo;

    private DataType(PrimitiveType type)
    {
        _type = type;
    }

    private DataType(ArrayInfo arrayInfo)
    {
        _type = PrimitiveType.Array;
        _arrayInfo = arrayInfo;
    }

    public bool IsArray([NotNullWhen(true)] out ArrayInfo? arrayInfo)
    {
        if (_arrayInfo is null)
        {
            arrayInfo = null;
            return false;
        }

        arrayInfo = _arrayInfo;
        return true;
    }
    
    public static DataType Integer { get; } = new(PrimitiveType.Integer);
    public static DataType Real { get; } = new(PrimitiveType.Real);
    public static DataType String { get; } = new(PrimitiveType.String);
    public static DataType Boolean { get; } = new(PrimitiveType.Boolean);
    public static DataType Void { get; } = new(PrimitiveType.Void);
    public static DataType Array(DataType ofType, int size)
    {
        return new DataType(new ArrayInfo(ofType, size));
    }

    public override string ToString()
    {
        return _arrayInfo switch
        {
            null => _type switch
            {
                PrimitiveType.Integer => "INTEGER",
                PrimitiveType.Real => "REAL",
                PrimitiveType.String => "STRING",
                PrimitiveType.Boolean => "BOOLEAN",
                PrimitiveType.Void => "VOID",
                PrimitiveType.Array => throw new InvalidOperationException("Array does not contain array info"),
                _ => throw new NotSupportedException()
            },
            _ => $"ARRAY {_arrayInfo.Size} OF {_arrayInfo.OfType}"
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
        return _type == other._type && Equals(_arrayInfo, other._arrayInfo);
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
        return HashCode.Combine((int)_type, _arrayInfo);
    }
}