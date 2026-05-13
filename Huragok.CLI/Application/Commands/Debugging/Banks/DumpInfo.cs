#if DEBUG
using System.CommandLine;
using System.Reflection;
using Huragok.Data.Processing.Audio;

namespace Huragok.Application.Commands.Debug;

internal static class DumpFMODInfo {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "dump-fmod-info",
            description: "Prints the entire info list for a sound bank."
        );

        var bankArg = Arguments.BankPath;
        cmd.Add(bankArg);

        // Command Handler
        cmd.SetAction(ctx => {
            string bankPath = ctx.GetRequiredValue(bankArg);
            DumpFSBInfoContent(bankPath);
        });

        return cmd;
    }

    private static void DumpFSBInfoContent(string bankPath) {
        string infoFile = Path.ChangeExtension(bankPath, "fsb.info");

        var method = typeof(FSBExplorer).GetMethod("TryReadInfoFile", BindingFlags.NonPublic | BindingFlags.Static);
        string[] result = (string[]?)method!.Invoke(null, new object[] { infoFile }) ?? Array.Empty<string>();

        for (int i = 0; i < result.Length; i++) {
            Console.WriteLine($"#{i}: {result[i]}");
        }
    }
}

#endif