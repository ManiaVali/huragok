// Huragok and SubanUnity (C) ManiaVali, 2026
// Is it bad that I feel like every project I do is the worst code I've ever written?

// This is a helper program which works alongside its UnityEngine component, SubanUnity, to import data from the Halo engine.
// Because ManagedBlam is a hybrid DLL, Unity cannot load it natively, which is why this exists at all.
// Huragok is not meant to be run directly, and will often produce Unity-specific data.

using System.CommandLine;
using Huragok.Serializer;
using Huragok.Configuration;
using Huragok.Utilities;
using CommonArgsAndOpts = Huragok.Commands.Base.ArgsAndOpts;

namespace Huragok {
    internal static class MainProgram {
        internal static string originalWorkingDirectory = string.Empty;
        internal static DataSerializationFormat defaultSerializationFormat;

        private static int Main(string[] args) {
            try {
                RootCommand rootCmd = new($"Helper program for extracting and converting data from the Halo engine into formats other programs can understand.\n{GlobalConstants.ENGINE_PRETTY_NAME} build.");
                originalWorkingDirectory = Environment.CurrentDirectory;

#if !USING_BLAM_HR
                throw new NotImplementedException($"Engine variant `{Huragok.Utilities.GlobalConstants.EnginePrettyName}` not yet supported.");
#endif

                rootCmd.AddCommand(Commands.Serialize.Base.Register());
                rootCmd.AddCommand(Commands.Export.Base.Register());

                var configOption = CommonArgsAndOpts.ConfigFile;
                rootCmd.AddOption(configOption);

                var serializerFmtOption = CommonArgsAndOpts.SerializerFormat;
                rootCmd.AddOption(serializerFmtOption);

                var parseResult = rootCmd.Parse(args);
                string configArgPath = parseResult.GetValueForOption(configOption) ?? "";

                string fmt = parseResult.GetValueForOption(serializerFmtOption) ?? "json";
                defaultSerializationFormat = fmt?.ToLower() switch {
                    "json" => DataSerializationFormat.JSON,
                    "yaml" => DataSerializationFormat.YAML,
                    _ => throw new ArgumentException($"Invalid serialization format: {defaultSerializationFormat}")
                };

                if (!string.IsNullOrEmpty(configArgPath)) ConfigurationReader.configFileLocation = configArgPath;

                return rootCmd.Invoke(args);
            } catch (Exception ex) {
                Console.Error.WriteLine($"{GlobalConstants.PROGRAM_NAME} has encountered a fatal error: {ex.Message}");
                return 1;
            }
        }
    }
}