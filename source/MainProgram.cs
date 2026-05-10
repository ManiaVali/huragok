// Huragok (C) ManiaVali, 2026
// Is it bad that I feel like every project I do is the worst code I've ever written?

global using static Huragok.MainProgram;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Huragok.Configuration;
using Huragok.Utilities;
using Huragok.Utilities.Serializer;
using CommonArgsAndOpts = Huragok.Commands.Base.ArgsAndOpts;

namespace Huragok {
    internal static class MainProgram {
        internal static string originalWorkingDirectory = string.Empty;
        internal static DataSerializationFormat defaultSerializationFormat;
        internal static LoggingLevel globalLogLevel;

        /// <summary>
        /// CLI entry point
        /// </summary>
        private static async Task<int> Main(string[] args) {
            RootCommand rootCmd = new($"Helper program for extracting and converting data from the Halo engine into formats other programs can understand.\n {GlobalConstants.ENGINE_PRETTY_NAME} build.");
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

            var configOption = CommonArgsAndOpts.ConfigFile;
            rootCmd.AddOption(configOption);

            var serializerFmtOption = CommonArgsAndOpts.SerializerFormat;
            rootCmd.AddOption(serializerFmtOption);

            var logLevelOption = CommonArgsAndOpts.LogLevel;
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
                "json" => DataSerializationFormat.JSON,
                "yaml" => DataSerializationFormat.YAML,
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


        // internal static void Panic(string panicMessage, sbyte exitCode = 1) {
        //     if (!string.IsNullOrWhiteSpace(panicMessage))
        //         Logger.Error("Fatal error: " + panicMessage, fatal: true);

        //     Environment.Exit(exitCode);
        // }
    }
}