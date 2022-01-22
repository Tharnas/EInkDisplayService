using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EInkService.Converter
{
    public class UnixTimeConverter : JsonConverter<DateTime>
    {
        public bool? IsFormatInSeconds { get; set; } = null;
        private static readonly long _unixMinSeconds = DateTimeOffset.MinValue.ToUnixTimeSeconds() - DateTimeOffset.UnixEpoch.ToUnixTimeSeconds(); // -62_135_596_800
        private static readonly long _unixMaxSeconds = DateTimeOffset.MaxValue.ToUnixTimeSeconds() - DateTimeOffset.UnixEpoch.ToUnixTimeSeconds(); // 253_402_300_799


        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetInt64(out var time))
            {
                // if 'IsFormatInSeconds' is unspecified, then deduce the correct type based on whether it can be represented in the allowed .net DateTime range
                if (IsFormatInSeconds.GetValueOrDefault(true) && time > _unixMinSeconds && time < _unixMaxSeconds)
                    return DateTimeOffset.FromUnixTimeSeconds(time).LocalDateTime;
                return DateTimeOffset.FromUnixTimeMilliseconds(time).LocalDateTime;
            }

            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
