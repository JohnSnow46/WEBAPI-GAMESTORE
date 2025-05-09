using Newtonsoft.Json;

#pragma warning disable CA1050 // Declare types in namespaces
public class NewtonsoftUppercaseGuidConverter : JsonConverter
#pragma warning restore CA1050 // Declare types in namespaces
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Guid) || objectType == typeof(Guid?);
    }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    {
#pragma warning disable CS8604 // Possible null reference argument.
        return reader.TokenType == JsonToken.Null ? null : Guid.Parse((string)reader.Value);
#pragma warning restore CS8604 // Possible null reference argument.
    }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(((Guid)value).ToString().ToUpperInvariant());
        }
    }
}