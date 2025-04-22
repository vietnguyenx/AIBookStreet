using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIBookStreet.Services.Common
{
    public class DateTimeFormatConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
                
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                // Format DateTime without fractional seconds
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }
} 