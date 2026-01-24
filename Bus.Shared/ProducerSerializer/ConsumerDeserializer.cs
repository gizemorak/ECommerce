using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bus.Shared.ProducerSerializer
{
    public sealed class ConsumerDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull || data.IsEmpty)
            {
                Console.WriteLine($"ConsumerDeserializer: Received null or empty data");
                return default!;
            }

            // Check if data contains only null bytes or whitespace
            bool allNulls = true;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0 && !char.IsWhiteSpace((char)data[i]))
                {
                    allNulls = false;
                    break;
                }
            }

            if (allNulls)
            {
                Console.WriteLine($"ConsumerDeserializer: Data contains only null bytes or whitespace");
                return default!;
            }

            try
            {
               
                var jsonString = Encoding.UTF8.GetString(data).Trim('\0', ' ', '\t', '\n', '\r');
                
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    Console.WriteLine($"ConsumerDeserializer: JSON string is empty after trimming");
                    return default!;
                }

                // Validate it starts with JSON-like characters
                var trimmed = jsonString.TrimStart();
                if (trimmed.Length == 0 || (trimmed[0] != '{' && trimmed[0] != '['))
                {
                    Console.WriteLine($"ConsumerDeserializer: Data does not appear to be JSON. First char: '{(trimmed.Length > 0 ? trimmed[0] : '?')}' (byte: {(trimmed.Length > 0 ? (int)trimmed[0] : 0)})");
                    Console.WriteLine($"ConsumerDeserializer: First 100 bytes as hex: {BitConverter.ToString(data.Slice(0, Math.Min(100, data.Length)).ToArray())}");
                    return default!;
                }

                return JsonSerializer.Deserialize<T>(jsonString)!;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"ConsumerDeserializer: JSON deserialization failed: {ex.Message}");
     
                return default!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConsumerDeserializer: Unexpected error during deserialization: {ex.GetType().Name} - {ex.Message}");

                return default!;
            }
        }
    }
}
