using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace iSchool.Infrastructure
{
    /// <summary>
    /// XXXX.SerializerSettings.Converters.Add(new CustomDateTimeConverter
    /// {
    ///     ReaderConverter = new IsoDateTimeConverter { DateTimeFormat = null },
    ///     WriterConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff" },
    /// });
    /// </summary>
    public class CustomDateTimeConverter : JsonConverter
    {
        public DateTimeConverterBase ReaderConverter { get; set; }
        public DateTimeConverterBase WriterConverter { get; set; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?) || (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReaderConverter.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            WriterConverter.WriteJson(writer, value, serializer);
        }
    }
}