namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Enum to specify the type of value stored in a property.
    /// </summary>
    public enum PropertyType
    {
        Long,
        Double,
        Bool,
        String
    }

    public static class PropertyTypeExtensions
    {
        public static bool IsNumber(this PropertyType @this)
        {
            return @this == PropertyType.Double
                || @this == PropertyType.Long;
        }
    }
}
