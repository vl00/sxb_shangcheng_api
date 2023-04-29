using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace iSchool.Infrastructure
{
    /// <summary>
    /// DateTime json to Timestamp ms
    /// </summary>
    public class DateTimeToTimestampJsonConverter : JsonConverter
    {
        public DateTimeToTimestampJsonConverter() : this(DateTimeKind.Local)
        { }

        public DateTimeToTimestampJsonConverter(DateTimeKind? readToDateTimeKind) 
        {
            ReadToDateTimeKind = readToDateTimeKind;
        }

        public DateTimeKind? ReadToDateTimeKind { get; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?) 
                || objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.Value?.ToString();
            if (string.IsNullOrEmpty(str) && (objectType == typeof(DateTimeOffset?) || objectType == typeof(DateTime?)))
            {
                return null;
            }
            switch (objectType)
            {
                case Type ty when ty == typeof(DateTimeOffset) || ty == typeof(DateTimeOffset?):
                    {
                        if (long.TryParse(str, out var timestampMs))
                        {
                            var time = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
                            return FxReadToDateTimeKind(time);
                        }
                        if (DateTimeOffset.TryParse(str, out var v))
                        {
                            return FxReadToDateTimeKind(v);
                        }
                        return FxReadToDateTimeKind(default);
                    }
                case Type ty when ty == typeof(DateTime) || ty == typeof(DateTime?):
                    {
                        if (long.TryParse(str, out var timestampMs))
                        {
                            var time = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
                            return FxReadToDateTimeKind2(time);
                        }
                        if (DateTimeOffset.TryParse(str, out var v))
                        {
                            return FxReadToDateTimeKind2(v);
                        }
                        return FxReadToDateTimeKind2(default);
                    }
                default:
                    return null;
            }
        }

        DateTimeOffset FxReadToDateTimeKind(in DateTimeOffset v)
        {
            return ReadToDateTimeKind == null ? v
                : ReadToDateTimeKind == DateTimeKind.Utc ? v.ToUniversalTime()
                : v.ToLocalTime();
        }

        DateTime FxReadToDateTimeKind2(in DateTimeOffset v)
        {
            return ReadToDateTimeKind == null ? v.LocalDateTime
                : ReadToDateTimeKind == DateTimeKind.Utc ? v.UtcDateTime
                : v.LocalDateTime;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (value)
            {
                case DateTimeOffset dateTimeOffset:
                    writer.WriteValue(dateTimeOffset.ToUnixTimeMilliseconds());
                    break;

                case DateTime dateTime:
                    {
                        writer.WriteValue(new DateTimeOffset(dateTime.ToUniversalTime(), TimeSpan.Zero).ToUnixTimeMilliseconds());
                        
                        //writer.WriteValue(Convert.ToInt64(new TimeSpan(dateTime.ToUniversalTime().Ticks - DateTime.UnixEpoch.Ticks).TotalMilliseconds));
                    }
                    break;

                case long _:
                case int _:
                case ulong _:
                case uint _:
                    writer.WriteValue(value);
                    break;

                case string str when long.TryParse(str, out var _long):
                    writer.WriteValue(_long);
                    break;

                case null:
                default:
                    writer.WriteUndefined();
                    break;
            }
        }
    }
}