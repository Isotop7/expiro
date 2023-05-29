using System;
using Newtonsoft.Json;

namespace isgood.Models.Converters.Product;

public class ProductScannedAtConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            throw new JsonSerializationException();
        }

        isgood.Models.Product p = (isgood.Models.Product)value;

        writer.WriteStartObject();

        writer.WritePropertyName("barcode");
        writer.WriteValue(p.Barcode);
        writer.WritePropertyName("scannedAt");
        writer.WriteValue(p.ScannedAt);

        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
    }

    public override bool CanRead
    {
        get { return false; }
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(isgood.Models.Product);
    }
}