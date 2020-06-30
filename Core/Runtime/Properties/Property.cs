using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

// ReSharper disable Unity.RedundantFormerlySerializedAsAttribute

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Struct of this type store a property value and its current type.
    /// </summary>
    [Serializable]
    public struct Property : IEquatable<Property>,
        IEqualityComparer<Property>,
        IComparable<Property>,
        IDictionaryConvertible
    {
        /// <summary>
        ///     Stored value's type.
        /// </summary>
        [field: SerializeField]
        [field: FormerlySerializedAs("m_Type")]
        public PropertyType type { get; internal set; }

        /// <summary>
        ///     Stored built-in value type.
        ///     Relevant only if this property isn't a <see cref="PropertyType.String" /> type.
        /// </summary>
        [SerializeField]
        internal CoalescedValueType valueType;

        /// <summary>
        ///     Stored long value.
        ///     Relevant only if this property is a <see cref="PropertyType.Long" /> type.
        /// </summary>
        internal long longValue
        {
            get => valueType.longValue;
            set => valueType.longValue = value;
        }

        /// <summary>
        ///     Stored double value.
        ///     Relevant only if this property is a <see cref="PropertyType.Double" /> type.
        /// </summary>
        internal double doubleValue
        {
            get => valueType.doubleValue;
            set => valueType.doubleValue = value;
        }

        /// <summary>
        ///     Stored bool value.
        ///     Relevant only if this property is a <see cref="PropertyType.Bool" /> type.
        /// </summary>
        internal bool boolValue
        {
            get => valueType.boolValue;
            set => valueType.boolValue = value;
        }

        /// <summary>
        ///     Stored string value.
        ///     Relevant only if this property is a <see cref="PropertyType.String" /> type.
        /// </summary>
        [SerializeField]
        internal string stringValue;

        /// <summary>
        ///     Explicitly casts this <see cref="Property" /> into an integer.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The integer value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown if the stored value is outside int boundaries.
        /// </exception>
        public int AsInt()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return Convert.ToInt32(longValue);
                case PropertyType.Double:
                    return Convert.ToInt32(doubleValue);

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into an integer, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property" /> into a long.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The long value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown if the stored value is outside long boundaries.
        /// </exception>
        public long AsLong()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue;
                case PropertyType.Double:
                    return Convert.ToInt64(doubleValue);

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into an integer, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property" /> into a float.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The float value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public float AsFloat()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue;
                case PropertyType.Double:
                    return (float)doubleValue;

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into a float, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property" /> into a double.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The double value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public double AsDouble()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue;
                case PropertyType.Double:
                    return doubleValue;
                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into a float, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property" /> into a bool.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The bool value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        ///     Thrown if the stored type isn't compatible with the requested type.
        /// </exception>
        public bool AsBool()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue != 0;
                case PropertyType.Double:
                    return Math.Abs(doubleValue) > double.Epsilon;
                case PropertyType.Bool:
                    return boolValue;

                default:
                    throw new InvalidCastException(
                        $"Cannot cast this {nameof(Property)} instance into a bool, because it contains a {type} value.");
            }
        }

        /// <summary>
        ///     Explicitly casts this <see cref="Property" /> into a string.
        ///     Handle convertible types properly.
        /// </summary>
        /// <returns>
        ///     The string value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if the stored type isn't supported.
        /// </exception>
        public string AsString()
        {
            switch (type)
            {
                case PropertyType.Long:
                    return longValue.ToString();
                case PropertyType.Double:
                    return doubleValue.ToString();
                case PropertyType.Bool:
                    return boolValue.ToString();
                case PropertyType.String:
                    return stringValue;

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(type));
            }
        }

        /// <summary>
        ///     Compare the two given operands for equality.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if both operands have the same property type and value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if the stored type isn't supported.
        /// </exception>
        public static bool operator ==(Property a, Property b)
        {
            if (a.type != b.type)
                return false;

            switch (a.type)
            {
                case PropertyType.Long:
                    return a.longValue == b.longValue;
                case PropertyType.Double:
                    return Math.Abs(a.doubleValue - b.doubleValue) <= double.Epsilon;
                case PropertyType.Bool:
                    return a.boolValue == b.boolValue;
                case PropertyType.String:
                    return a.stringValue == b.stringValue;

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands for inequality.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if both operands have different property type or value.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if the stored type isn't supported.
        /// </exception>
        public static bool operator !=(Property a, Property b)
        {
            if (a.type != b.type)
                return true;

            switch (a.type)
            {
                case PropertyType.Long:
                    return a.longValue != b.longValue;
                case PropertyType.Double:
                    return Math.Abs(a.doubleValue - b.doubleValue) > double.Epsilon;
                case PropertyType.Bool:
                    return a.boolValue != b.boolValue;
                case PropertyType.String:
                    return a.stringValue != b.stringValue;

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Add the two given operands into a new <see cref="Property" />.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     A new <see cref="Property" /> resulting from the addition of both operands.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static Property operator +(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                {
                    var result = a.longValue + b.AsLong();

                    return result;
                }

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                {
                    var result = a.doubleValue + b.AsDouble();

                    return result;
                }

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                case PropertyType.String:
                {
                    var result = a.stringValue + b.AsString();

                    return result;
                }

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Subtract the two given operands into a new <see cref="Property" />.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     A new <see cref="Property" /> resulting from the subtraction of both operands.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static Property operator -(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                {
                    var result = a.longValue - b.AsLong();

                    return result;
                }

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                {
                    var result = a.doubleValue - b.AsDouble();

                    return result;
                }

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands if they are numbers.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if <paramref name="a" />'s value is smaller than <paramref name="b" />'s;
        ///     false otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static bool operator <(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long:
                    return a.longValue < b.longValue;
                case PropertyType.Long
                    when b.type == PropertyType.Double:
                    return a.AsDouble() < b.doubleValue;

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                    return a.doubleValue < b.AsDouble();

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands if they are numbers.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if <paramref name="a" />'s value is greater than <paramref name="b" />'s;
        ///     false otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static bool operator >(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long:
                    return a.longValue > b.longValue;
                case PropertyType.Long
                    when b.type == PropertyType.Double:
                    return a.AsDouble() > b.doubleValue;

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                    return a.doubleValue > b.AsDouble();

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands if they are numbers.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if <paramref name="a" />'s value is smaller or equal to <paramref name="a" />'s;
        ///     false otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static bool operator <=(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long:
                    return a.longValue <= b.longValue;
                case PropertyType.Long
                    when b.type == PropertyType.Double:
                    return a.AsDouble() <= b.doubleValue;

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                    return a.doubleValue <= b.AsDouble();

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <summary>
        ///     Compare the two given operands if they are numbers.
        /// </summary>
        /// <param name="a">
        ///     Left operand.
        /// </param>
        /// <param name="b">
        ///     Right operand.
        /// </param>
        /// <returns>
        ///     True if <paramref name="a" />'s value is greater or equal to <paramref name="b" />'s;
        ///     false otherwise.
        /// </returns>
        /// <exception cref="NotSupportedException">
        ///     Thrown if one of the operands' type isn't supported.
        /// </exception>
        public static bool operator >=(Property a, Property b)
        {
            switch (a.type)
            {
                case PropertyType.Long
                    when b.type == PropertyType.Long:
                    return a.longValue >= b.longValue;
                case PropertyType.Long
                    when b.type == PropertyType.Double:
                    return a.AsDouble() >= b.doubleValue;

                case PropertyType.Double
                    when b.type == PropertyType.Long
                    || b.type == PropertyType.Double:
                    return a.doubleValue >= b.AsDouble();

                case PropertyType.Long:
                case PropertyType.Double:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(b.type));

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(a.type));
            }
        }

        /// <inheritdoc cref="AsInt" />
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        public static implicit operator int(Property value) => value.AsInt();

        /// <summary>
        ///     Construct a <see cref="Property" /> from the given <paramref name="value" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property" />.
        /// </param>
        public static implicit operator Property(int value)
        {
            return new Property
            {
                type = PropertyType.Long,
                longValue = value
            };
        }

        /// <inheritdoc cref="AsLong" />
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        public static implicit operator long(Property value) => value.AsLong();

        /// <summary>
        ///     Construct a <see cref="Property" /> from the given <paramref name="value" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property" />.
        /// </param>
        public static implicit operator Property(long value)
        {
            return new Property
            {
                type = PropertyType.Long,
                longValue = value
            };
        }

        /// <inheritdoc cref="AsFloat" />
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        public static implicit operator float(Property value) => value.AsFloat();

        /// <summary>
        ///     Construct a <see cref="Property" /> from the given <paramref name="value" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property" />.
        /// </param>
        public static implicit operator Property(float value)
        {
            return new Property
            {
                type = PropertyType.Double,
                doubleValue = value
            };
        }

        /// <inheritdoc cref="AsDouble" />
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        public static implicit operator double(Property value) => value.AsDouble();

        /// <summary>
        ///     Construct a <see cref="Property" /> from the given <paramref name="value" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property" />.
        /// </param>
        public static implicit operator Property(double value)
        {
            return new Property
            {
                type = PropertyType.Double,
                doubleValue = value
            };
        }

        /// <inheritdoc cref="AsBool" />
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        public static implicit operator bool(Property value) => value.AsBool();

        /// <summary>
        ///     Construct a <see cref="Property" /> from the given <paramref name="value" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property" />.
        /// </param>
        public static implicit operator Property(bool value)
        {
            return new Property
            {
                type = PropertyType.Bool,
                boolValue = value
            };
        }

        /// <inheritdoc cref="AsString" />
        /// <param name="value">
        ///     The value to cast.
        /// </param>
        public static implicit operator string(Property value) => value.AsString();

        /// <summary>
        ///     Construct a <see cref="Property" /> from the given <paramref name="value" />.
        /// </summary>
        /// <param name="value">
        ///     The value to cast into <see cref="Property" />.
        /// </param>
        public static implicit operator Property(string value)
        {
            return new Property
            {
                type = PropertyType.String,
                stringValue = value
            };
        }

        public bool Equals(Property other) => this == other;

        public override bool Equals(object obj) => obj is Property other && this == other;

        public bool Equals(Property x, Property y) => x.Equals(y);

        public override int GetHashCode()
        {
            //Generated with Rider.
            unchecked
            {
                var hashCode = (int)type;
                hashCode = (hashCode * 397) ^ valueType.GetHashCode();
                hashCode = (hashCode * 397) ^ (stringValue != null ? stringValue.GetHashCode() : 0);
                return hashCode;
            }
        }

        public int GetHashCode(Property obj) => obj.GetHashCode();

        public int CompareTo(Property other)
        {
            if (type != other.type)
            {
                if ((int)type < (int)other.type)
                    return -1;

                return 1;
            }

            switch (type)
            {
                case PropertyType.Long:
                case PropertyType.Double:
                case PropertyType.Bool:
                    return valueType.CompareTo(other.valueType);

                case PropertyType.String:
                    return stringValue.CompareTo(other.stringValue);

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(type));
            }
        }

        /// <inheritdoc />
        public override string ToString() => AsString();

        /// <inheritdoc cref="IDictionaryConvertible.ToDictionary" />
        public Dictionary<string, object> ToDictionary()
        {
            var dataDictionary = new Dictionary<string, object>(2)
            {
                [nameof(type)] = (int)type
            };

            if (type == PropertyType.String)
                dataDictionary.Add(nameof(stringValue), stringValue);
            else
            {
                //We only need to serialize the greatest value.
                dataDictionary.Add(nameof(valueType), longValue);
            }

            return dataDictionary;
        }

        /// <inheritdoc cref="IDictionaryConvertible.FillFromDictionary" />
        public void FillFromDictionary(Dictionary<string, object> rawDictionary)
        {
            type = default;
            valueType = default;
            stringValue = default;

            if (rawDictionary.TryGetValue(nameof(type), out var rawType))
            {
                type = (PropertyType)Convert.ToInt32(rawType);
            }

            if (rawDictionary.TryGetValue(nameof(valueType), out var rawValueType))
            {
                valueType = Convert.ToInt64(rawValueType);
            }

            if (rawDictionary.TryGetValue(nameof(stringValue), out var rawString))
            {
                stringValue = Convert.ToString(rawString);
            }
        }

        /// <summary>
        ///     Create a new <see cref="Property" /> instance by parsing the given
        ///     <paramref name="rawPropertyType" /> and <paramref name="rawValue" />.
        /// </summary>
        /// <param name="rawPropertyType">
        ///     Property type to parse.
        /// </param>
        /// <param name="rawValue">
        ///     Property value to parse.
        /// </param>
        /// <param name="property">
        ///     Created <see cref="Property" /> if the parsing was successful.
        /// </param>
        /// <returns>
        ///     Return true if the given strings could be parsed into a valid <see cref="Property" />;
        ///     return false otherwise.
        /// </returns>
        public static bool TryParse(string rawPropertyType, string rawValue, out Property property)
        {
            if (!Enum.TryParse(rawPropertyType, true, out PropertyType propertyType))
            {
                property = default;

                return false;
            }

            switch (propertyType)
            {
                case PropertyType.Long:
                {
                    if (int.TryParse(rawValue, out var intValue))
                    {
                        property = intValue;

                        return true;
                    }

                    break;
                }

                case PropertyType.Double:
                {
                    if (float.TryParse(rawValue, out var floatValue))
                    {
                        property = floatValue;

                        return true;
                    }

                    break;
                }

                case PropertyType.Bool:
                {
                    if (bool.TryParse(rawValue, out var boolValue))
                    {
                        property = boolValue;

                        return true;
                    }

                    break;
                }

                case PropertyType.String:
                {
                    property = rawValue;

                    return true;
                }

                default:
                    throw new NotSupportedException(GetUnsupportedTypeErrorMessage(propertyType));
            }

            property = default;

            return false;
        }

        static string GetUnsupportedTypeErrorMessage(PropertyType propertyType)
            => $"The property type \"{propertyType}\" isn't supported.";
    }
}
