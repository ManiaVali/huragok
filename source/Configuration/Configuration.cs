using System.Text.Json;
using System.Text.Json.Serialization;
using FFMpegCore;
using Huragok.Utilities;

namespace Huragok.Configuration {
    // Disable warnings regarding variables never being assigned as they're assigned via JSON Deserialization
#pragma warning disable CS0649
    internal class ConfigurationObject {
        [JsonInclude]
        private string? ProjectPath_H2AMP { get; set; }
        [JsonInclude]
        private string? ProjectPath_H3 { get; set; }
        [JsonInclude]
        private string? ProjectPath_H3ODST { get; set; }
        [JsonInclude]
        private string? ProjectPath_HR { get; set; }
        [JsonInclude]
        private string? ProjectPath_H4 { get; set; }

        public required string MCCInstallPath { get; set; }

#if USING_BLAM_H3
        public string ProjectPath => this.ProjectPath_H3 ?? string.Empty;
#elif USING_BLAM_H3ODST
        public string ProjectPath => this.ProjectPath_H3ODST ?? string.Empty;
#elif USING_BLAM_HR
        public string ProjectPath => this.ProjectPath_HR ?? string.Empty;
#elif USING_BLAM_H4
        public string ProjectPath => this.ProjectPath_H4 ?? string.Empty;
#elif USING_BLAM_H2AMP
        public string ProjectPath => this.ProjectPath_H2AMP ?? string.Empty;
#endif

        // internal static ConfigurationObject Empty => new();
    }
#pragma warning restore CS0649

    internal static class ConfigurationReader {
        internal static string configFileLocation = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "config", "HuragokConfiguration.json"));
        private const string CONF_PARSE_ERR = "Configuration file parse error";

        private static ConfigurationObject? _configuration;
        internal static ConfigurationObject Configuration => _configuration ?? ReadConfiguration();

        private static ConfigurationObject ReadConfiguration() {
            if (!File.Exists(configFileLocation)) throw new FileNotFoundException($"Configuration file expected at `{configFileLocation}` but not found!");

            string jsonConfig = File.ReadAllText(configFileLocation);
            if (string.IsNullOrEmpty(jsonConfig)) throw new InvalidDataException($"Configuration file at `{configFileLocation}` appears to be empty!");
            var configurationObject = JsonSerializer.Deserialize<ConfigurationObject>(jsonConfig) ?? throw new JsonException($"Failed to deserialize configuration file.");

            if (string.IsNullOrWhiteSpace(configurationObject.ProjectPath))
                throw new ArgumentNullException($"{CONF_PARSE_ERR}: Key for `{GlobalConstants.ENGINE_PRETTY_NAME}` is missing or empty in configuration file.");

            if (!Directory.Exists(configurationObject.ProjectPath))
                throw new DirectoryNotFoundException($"{CONF_PARSE_ERR}: The editing kit location specified in key for `{GlobalConstants.ENGINE_PRETTY_NAME}` does not appear to exist.");

            if (string.IsNullOrWhiteSpace(configurationObject.MCCInstallPath))
                throw new ArgumentNullException($"{CONF_PARSE_ERR}: Key for MCC Install Path is missing or empty in configuration file.");

            if (!Directory.Exists(configurationObject.MCCInstallPath))
                throw new DirectoryNotFoundException($"{CONF_PARSE_ERR}: The MCC install location specified in configuration file does not appear to exist.");

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "utils", "ffmpeg")) });

            _configuration = configurationObject;
            return configurationObject;
        }
    }
}