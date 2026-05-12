#if DEBUG
using System.CommandLine;
using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;

namespace Huragok.Application.Commands.Debug;

internal static class EnumerateBank {
    internal static Command Register() {
        // Command Setup
        var cmd = new Command(
            name: "enumerate-bank",
            description: "Prints the entire sample list for a sound bank."
        );

        var bankArg = Arguments.BankPath;
        cmd.Add(bankArg);

        // Command Handler
        cmd.SetAction(ctx => {
            string bankPath = ctx.GetRequiredValue(bankArg);
            EnumerateSamples(bankPath);
        });

        return cmd;
    }

    private static void EnumerateSamples(string bankPath) {
        byte[] fmodBytes = File.ReadAllBytes(bankPath);
        var bankFile = FsbLoader.LoadFsbFromByteArray(fmodBytes);
        var samples = bankFile.Samples;

        for (int i = 0; i < samples.Count; i++) {
            Console.WriteLine($"#{i}: name: {samples[i].Name}, freq: {samples[i].Metadata.Frequency} Hz, length: {GetLengthSeconds(samples[i])} seconds");
        }

        static double GetLengthSeconds(FmodSample sample) {
            return (double)sample.Metadata.SampleCount / sample.Metadata.Frequency;
        }
    }
}
#endif