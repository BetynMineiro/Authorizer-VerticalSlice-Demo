using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Authorizer.Common
{
    public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                DateTimeOffset.ParseExact(reader.GetString(),
                    "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFZ", CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(dateTimeValue.ToString(
                    "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFZ", CultureInfo.InvariantCulture));
    }


}
