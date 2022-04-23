namespace VTS.Models{
    /// <summary>
    /// Interface for providing a JSON Serializer/Deserializer implementation.
    /// </summary>
    public interface IJsonUtility
    {
        /// <summary>
        /// Deserializes a JSON string into an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <returns>The deserialized object of the specified type.</returns>
        T FromJson<T>(string json);
        /// <summary>
        /// Converts an object into a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialized.</param>
        /// <returns>The serialized JSON string.</returns>
        string ToJson(object obj);
    }
}
