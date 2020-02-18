using System;
using System.Runtime.InteropServices;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    using StatValueType = StatDefinition.StatValueType;

    /// <summary>
    /// This struct standardizes how stat values are serialized.
    /// </summary>
    /// <remarks>
    /// For fields that share the same memory space, only the bigger type will be serialized
    /// to avoid conflicting serialization and minimize the serialization size.
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    struct StatUnion
    {
        [FieldOffset(0)]
        public StatValueType type;

        [FieldOffset(sizeof(StatValueType))]
        public int intValue;

        [NonSerialized]
        [FieldOffset(sizeof(StatValueType))]
        public float floatValue;

        public static implicit operator int(StatUnion value) => value.intValue;

        public static implicit operator StatUnion(int value)
            => new StatUnion
            {
                type = StatValueType.Int,
                intValue = value
            };

        public static implicit operator float(StatUnion value) => value.floatValue;

        public static implicit operator StatUnion(float value)
            => new StatUnion
            {
                type = StatValueType.Float,
                floatValue = value
            };
    }
}
