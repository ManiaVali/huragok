
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Huragok.Serializer {
    internal enum DataSerializationFormat {
        JSON,
        YAML
    }
    internal static class DataSerializer {

        private static readonly JsonSerializerOptions jsonSerializerOptions = new() {
            WriteIndented = true,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private static readonly ISerializer yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(
                DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections
            )
            .Build();

        internal static void Serialize(Stream stream, object serializingObject, DataSerializationFormat serializationFormat) {
            switch (serializationFormat) {
                case DataSerializationFormat.JSON:
                    JsonSerializer.Serialize(stream, serializingObject, jsonSerializerOptions);
                    break;

                case DataSerializationFormat.YAML:
                    using (var writer = new StreamWriter(stream, leaveOpen: true)) {
                        yamlSerializer.Serialize(writer, serializingObject);
                        writer.Flush();
                    }
                    break;

                default:
                    throw new ArgumentException($"Invalid format");
            }
        }
    }
}