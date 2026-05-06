using System.CommandLine;
using System.Reflection;
using System.Text.RegularExpressions;
using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;
using Huragok.Utilities.Sound;

namespace Huragok.Commands.Debug {
    internal static class ProbeDisagree {
        internal static Command Register() {
            // Command Setup
            var cmd = new Command(
                name: "probe-index-disagree",
                description: "Tests a bank and its info list for disagreements."
            );

            var bankArg = ArgsAndOpts.BankPath;
            cmd.AddArgument(bankArg);

            // Command Handler
            cmd.SetHandler(ctx => {
                string bankPath = ctx.ParseResult.GetValueForArgument(bankArg);
                CompareFSBandInfo(bankPath);
            });

            return cmd;
        }

        private static void CompareFSBandInfo(string bankPath) {
            var fmodBytes = File.ReadAllBytes(bankPath);
            var bankFile = FsbLoader.LoadFsbFromByteArray(fmodBytes);
            var samples = bankFile.Samples;

            var infoFile = Path.ChangeExtension(bankPath, "fsb.info");
            var method = typeof(FSBExplorer).GetMethod(
                "TryReadInfoFile",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            string[] result = (string[]?)method!.Invoke(null, new object[] { infoFile })
                              ?? Array.Empty<string>();

            int bankCount = samples.Count;
            int infoCount = result.Length;
            int minCount = Math.Min(bankCount, infoCount);

            // Compare overlapping range
            for (int i = 0; i < minCount; i++) {
                string bankName = samples[i].Name;

                string infoName = Path.GetFileName(result[i]);

                if (!string.Equals(bankName, infoName, StringComparison.Ordinal)) {
                    Logger.Warning(
                        $"Bank and info disagree at index {i}. Bank had '{bankName}' but info has '{infoName} ({result[i]})'!"
                    );
                }
            }

            // Extra entries in bank
            if (bankCount > infoCount) {
                for (int i = infoCount; i < bankCount; i++) {
                    Logger.Warning(
                        $"Extra bank entry at {i}: '{samples[i].Name}'"
                    );
                }
            }

            // Extra entries in info
            if (infoCount > bankCount) {
                for (int i = bankCount; i < infoCount; i++) {
                    Logger.Warning(
                        $"Extra info entry at {i}: '{Path.GetFileName(result[i])}'"
                    );
                }
            }
        }
    }
}