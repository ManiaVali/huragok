#if DEBUG
using System.CommandLine;
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
            var bankMap = FSBExplorer.BuildBankMap(bankPath);

            bool anyDisagree = false;
            foreach (int key in bankMap.Keys) {
                var (sample, infoFilePath) = bankMap[key];

                string sampleName = sample.Name!;
                string filePathName = Path.GetFileName(infoFilePath);

                if (sampleName != filePathName) {
                    Logger.Warning($"Index disagreement at #{key}: sample name `{sampleName}` disagrees with info file `{filePathName}`");
                    anyDisagree = true;
                }
            }

            if (!anyDisagree)
                Logger.Message($"{Path.GetFileName(bankPath)}: no index disagreements.");
        }
    }
}
#endif