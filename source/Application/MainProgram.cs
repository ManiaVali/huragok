// Huragok (C) ManiaVali, 2026
// Is it bad that I feel like every project I do is the worst code I've ever written?

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
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
    private static async Task<int> Main(string[] args) {
        RootCommand rootCmd = new($"Helper program for extracting and converting data from the Halo engine into formats other programs can understand.\n {Constants.ENGINE_PRETTY_NAME} build.");
        originalWorkingDirectory = Environment.CurrentDirectory;

#if !USING_BLAM_HR
                Panic($"Engine variant `{Huragok.Utilities.GlobalConstants.EnginePrettyName}` not yet supported.");
#endif

        rootCmd.AddCommand(Commands.Serialize.Base.Register());
        rootCmd.AddCommand(Commands.Export.Base.Register());
        rootCmd.AddCommand(Commands.Preview.Base.Register());
#if DEBUG
        rootCmd.AddCommand(Commands.Debug.Base.Register());
#endif

        var configOption = Arguments.ConfigFile;
        rootCmd.AddOption(configOption);

        var serializerFmtOption = Arguments.SerializerFormat;
        rootCmd.AddOption(serializerFmtOption);

        var logLevelOption = Arguments.LogLevel;
        rootCmd.AddOption(logLevelOption);

        var parseResult = rootCmd.Parse(args);
        string configArgPath = parseResult.GetValueForOption(configOption) ?? "";

        string logLevelString = parseResult.GetValueForOption(logLevelOption) ?? "info";
        globalLogLevel = logLevelString.ToLower() switch {
            "debug" => LoggingLevel.Debug,
            "info" => LoggingLevel.Info,
            "warning" => LoggingLevel.Warning,
            "error" => LoggingLevel.Error,
            _ => throw new ArgumentException($"Invalid log level: {logLevelString}")
        };

        string fmt = parseResult.GetValueForOption(serializerFmtOption) ?? "json";
        defaultSerializationFormat = fmt?.ToLower() switch {
            "json" => SerializationFormat.JSON,
            "yaml" => SerializationFormat.YAML,
            _ => throw new ArgumentException($"Invalid serialization format: {defaultSerializationFormat}")
        };

        if (!string.IsNullOrEmpty(configArgPath)) ConfigurationReader.configFileLocation = configArgPath;

        var builder = new CommandLineBuilder(rootCmd)
            .UseDefaults()
            .UseExceptionHandler((ex, context) => {
                Logger.Error("Fatal error: " + ex.Message, fatal: true);
#if DEBUG
                Logger.Debug("BEGIN STACK TRACE");
                if (globalLogLevel <= LoggingLevel.Debug)
                    Console.WriteLine(ex);
                Logger.Debug("END OF STACK TRACE");
#endif
                context.ExitCode = 1;
            });

        return await builder.Build().InvokeAsync(args);
    }
}