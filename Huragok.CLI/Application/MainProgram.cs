// Huragok (C) ManiaVali, 2026
// Is it bad that I feel like every project I do is the worst code I've ever written?

using System.CommandLine;
using Huragok.Application.Commands;
using Huragok.Application.Configuration;
using Huragok.Application.Logging;
using Huragok.Data.Serialization;

namespace Huragok.Application;

internal static class MainProgram {
    internal static string originalWorkingDirectory = string.Empty;
    internal static SerializationFormat defaultSerializationFormat;
    internal static LoggingLevel globalLogLevel;

    /// <summary>
    /// CLI entry point
    /// </summary>
    private static int Main(string[] args) {
        try {
            RootCommand rootCmd = new($"Helper program for extracting and converting data from the Halo engine into formats other programs can understand.\n {Constants.ENGINE_PRETTY_NAME} build.") {
                Commands.Serialize.Base.Register(),
                Commands.Export.Base.Register(),
                Commands.Preview.Base.Register(),
                #if DEBUG
                Commands.Debug.Base.Register(),
                #endif
                Arguments.ConfigFile,
                Arguments.SerializerFormat,
                Arguments.LogLevel
            };

            originalWorkingDirectory = Environment.CurrentDirectory;

#if !USING_BLAM_HR
            throw new Exception($"Engine variant `{Huragok.Application.Constants.EnginePrettyName}` not yet supported.");
#endif

            var parseResult = rootCmd.Parse(args);

            string logLevelString = parseResult.GetRequiredValue(Arguments.LogLevel);
            globalLogLevel = logLevelString.ToLower() switch {
                "debug" => LoggingLevel.Debug,
                "info" => LoggingLevel.Info,
                "warning" => LoggingLevel.Warning,
                "error" => LoggingLevel.Error,
                _ => throw new ArgumentException($"Invalid log level: {logLevelString}")
            };

            string fmt = parseResult.GetRequiredValue(Arguments.SerializerFormat);
            defaultSerializationFormat = fmt?.ToLower() switch {
                "json" => SerializationFormat.JSON,
                "yaml" => SerializationFormat.YAML,
                _ => throw new ArgumentException($"Invalid serialization format: {defaultSerializationFormat}")
            };

            string configArgPath = parseResult.GetRequiredValue(Arguments.ConfigFile);
            if (!string.IsNullOrEmpty(configArgPath)) ConfigurationReader.configFileLocation = configArgPath;

            InvocationConfiguration config = new() {
                EnableDefaultExceptionHandler = false
            };

            return parseResult.Invoke(config);
        } catch (Exception ex) {
            Logger.Error("Fatal error: " + ex.Message, fatal: true);
#if DEBUG
            Logger.Debug("BEGIN STACK TRACE");
            if (globalLogLevel <= LoggingLevel.Debug)
                Console.WriteLine(ex);
            Logger.Debug("END OF STACK TRACE");
#endif
            return 1;
        }
    }
}