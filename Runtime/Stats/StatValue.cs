using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This struct standardizes how stat values are serialized.
    /// </summary>
    /// <remarks>
    /// For fields that share the same memory space, only the bigger type will be serialized
    /// to avoid conflicting serialization and minimize the serialization size.
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct StatValue : IEquatable<StatValue>, IDictionaryConvertible
    {
        /// <summary>
        /// Stat value type ('Int' or 'Float').
        /// </summary>
        [SerializeField]
        [FieldOffset(0)]
        internal StatValueType m_Type;

        /// <summary>
        /// Integer value, if stat is of 'Int' type.
        /// </summary>
        [SerializeField]
        [FieldOffset(sizeof(StatValueType))]
        internal int m_IntValue;

        /// <summary>
        /// Float value, if stat is of 'Float' type.
        /// </summary>
        [NonSerialized]
        [FieldOffset(sizeof(StatValueType))]
        internal float m_FloatValue;

        /// <summary>
        /// Stat value type ('Int' or 'Float').
        /// </summary>
        public StatValueType type => m_Type;

        /// <summary>
        /// Compare 2 StatValues for equality.
        /// </summary>
        /// <param name="a">lhs.</param>
        /// <param name="b">rhs.</param>
        /// <returns>True if both StatValues are identical.</returns>
        public static bool operator ==(StatValue a, StatValue b)
        {
            return a.m_Type == b.m_Type
                && (a.m_Type == StatValueType.Int && a.m_IntValue == b.m_IntValue)
                || (a.m_Type == StatValueType.Float && a.m_FloatValue == b.m_FloatValue);
        }

        /// <summary>
        /// Compare 2 StatValues for inequality.
        /// </summary>
        /// <param name="a">lhs.</param>
        /// <param name="b">rhs.</param>
        /// <returns>True if both StatValues are not identical.</returns>
        public static bool operator !=(StatValue a, StatValue b)
        {
            return a.m_Type != b.m_Type
                || (a.m_Type == StatValueType.Int && a.m_IntValue != b.m_IntValue)
                || (a.m_Type == StatValueType.Float && a.m_FloatValue != b.m_FloatValue);
        }

        /// <summary>
        /// Add 2 StatValues together.
        /// </summary>
        /// <param name="a">lhs.</param>
        /// <param name="b">rhs.</param>
        /// <returns>StatValue based upon the addition of both StatValues.</returns>
        /// <exception cref="InvalidCastException">If StatValue is of invalid or unhandled type.</exception>
        public static StatValue operator +(StatValue a, StatValue b)
        {
            switch (a.m_Type)
            {
                case StatValueType.Int:
                    {
                        var bValue = (int)b;
                        long result = (long)a.m_IntValue + bValue;

                        if (result < int.MinValue) return int.MinValue;
                        else if (result > int.MaxValue) return int.MaxValue;
                        return (int)result;
                    }
                case StatValueType.Float:
                    {
                        var bValue = (float)b;
                        double result = (double)a.m_FloatValue + bValue;

                        if (result < float.MinValue) return float.MinValue;
                        else if (result > float.MaxValue) return float.MaxValue;
                        return (float)result;
                    }
            }

            throw new InvalidCastException();
        }

        /// <summary>
        /// Subtract StatValue from another StatValue.
        /// </summary>
        /// <param name="a">lhs.</param>
        /// <param name="b">rhs.</param>
        /// <returns>StatValue based upon subtracting a StatValue from another StatValue.</returns>
        /// <exception cref="InvalidCastException">If StatValue is of invalid or unhandled type.</exception>
        public static StatValue operator -(StatValue a, StatValue b)
        {
            switch (a.m_Type)
            {
                case StatValueType.Int:
                    {
                        var bValue = (int)b;
                        long result = (long)a.m_IntValue - bValue;

                        if (result < int.MinValue) return int.MinValue;
                        else if (result > int.MaxValue) return int.MaxValue;
                        return (int)result;
                    }
                case StatValueType.Float:
                    {
                        var bValue = (float)b;
                        double result = (double)a.m_FloatValue - bValue;

                        if (result < float.MinValue) return float.MinValue;
                        else if (result > float.MaxValue) return float.MaxValue;
                        return (float)result;
                    }
            }

            throw new InvalidCastException();
        }

        /// <summary>
        /// The integer value for this stat if its type is 'Int'.
        /// </summary>
        /// <param name="value">StatValue to retrieve integer value from.</param>
        public static implicit operator int(StatValue value)
        {
            if(value.m_Type == StatValueType.Int)
            {
                return value.m_IntValue;
            }

            throw new InvalidCastException
                ($"Cannot cast this {nameof(StatValue)} instance into an integer, because it contains a {value.m_Type} value.");
        }

        /// <summary>
        /// Construct a StatValue from an integer.
        /// </summary>
        /// <param name="value">integer value to place into StatValue.</param>
        public static implicit operator StatValue(int value) => new StatValue
            {
                m_Type = StatValueType.Int,
                m_IntValue = value
            };

        /// <summary>
        /// The float value for this stat if its type is 'Float' else int value cast to a float.
        /// </summary>
        /// <param name="value">StatValue to retrieve float value from.</param>
        public static implicit operator float(StatValue value)
        {
            if (value.m_Type == StatValueType.Float)
            {
                return value.m_FloatValue;
            }

            throw new InvalidCastException
                ($"Cannot cast this {nameof(StatValue)} instance into a float, because it contains a {value.m_Type} value.");
        }

        /// <summary>
        /// Construct a StatValue from an float.
        /// </summary>
        /// <param name="value">float value to place into StatValue.</param>
        public static implicit operator StatValue(float value) => new StatValue
            {
                m_Type = StatValueType.Float,
                m_FloatValue = value
            };

        public override int GetHashCode() => m_IntValue;

        public override bool Equals(object obj) => (obj is StatValue other) ? this == other : false;

        bool IEquatable<StatValue>.Equals(StatValue other) => this == other;

        /// <inheritdoc/>
        public override string ToString()
        {
            switch (m_Type)
            {
                case StatValueType.Int: return m_IntValue.ToString();
                case StatValueType.Float: return m_FloatValue.ToString();
                default: return null;
            } 
        }

        /// <inheritdoc cref="IDictionaryConvertible.ToDictionary"/>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                [nameof(m_Type)] = (int)m_Type,

                //We only need to serialize the greatest value.
                [nameof(m_IntValue)] = m_IntValue,
            };
        }

        /// <inheritdoc cref="IDictionaryConvertible.FillFromDictionary"/>
        public void FillFromDictionary(Dictionary<string, object> rawDictionary)
        {
            m_Type = default;
            m_IntValue = default;

            if (rawDictionary.TryGetValue(nameof(type), out var rawType))
            {
                m_Type = (StatValueType)Convert.ToInt64(rawType);
            }

            if (rawDictionary.TryGetValue(nameof(m_IntValue), out var rawValue))
            {
                m_IntValue = Convert.ToInt32(rawValue);
            }
        }

        /// <summary>
        /// Explicitly casts this <see cref="StatValue"/> into an integer, doing
        /// the necessary conversion if necessary, but throwing an
        /// <see cref="InvalidCastException"/> if not possible.
        /// </summary>
        /// <returns>The integer value.</returns>
        public int AsInt()
        {
            switch (m_Type)
            {
                case StatValueType.Int: return m_IntValue;
                case StatValueType.Float: return (int)m_FloatValue;
                default:
                    throw new InvalidCastException
                        ($"Cannot cast this {nameof(StatValue)} instance into a integer, because it contains a {m_Type} value.");
            }
        }

        /// <summary>
        /// Explicitly casts this <see cref="StatValue"/> into a float, doing
        /// the necessary conversion if necessary, but throwing an
        /// <see cref="InvalidCastException"/> if not possible.
        /// </summary>
        /// <returns>The float value.</returns>
        public float AsFloat()
        {
            switch (m_Type)
            {
                case StatValueType.Int: return m_IntValue;
                case StatValueType.Float: return m_FloatValue;
                default:
                    throw new InvalidCastException
                        ($"Cannot cast this {nameof(StatValue)} instance into a float, because it contains a {m_Type} value.");
            }
        }
    }
}