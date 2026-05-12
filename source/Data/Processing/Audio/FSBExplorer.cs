
using System.Text.RegularExpressions;
using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;
using Huragok.Application.Configuration;
using Huragok.Application.Logging;
using Huragok.Data.RuntimeFormats;

namespace Huragok.Data.Processing.Audio;
// TODO: Maybe add support for alternate language extraction?
internal static class FSBExplorer {
    private static readonly string _hrFSBDir = Path.Combine(ConfigurationReader.Configuration.MCCInstallPath, "haloreach", "fmod", "pc");
    private static readonly string[] _hrFSBNames = ["english.fsb", "sfx.fsb"];

    private static readonly Dictionary<string, FmodSoundBank> bankCache = new();

    #region Search
    internal static (FmodSample sample, string samplePath) FindInBanks(SoundPermutation lookingForPermutation) {
        foreach (string FSB in _hrFSBNames) {
            string fsbPath = Path.Combine(_hrFSBDir, FSB);
            var candidate = FindInBank(fsbPath, lookingForPermutation);

            if (candidate != null)
                return ((FmodSample sample, string samplePath))candidate;
        }
        throw new FileNotFoundException($"Could not find {lookingForPermutation.name} in any sound bank; it may not exist.");
    }

    private static (FmodSample sample, string samplePath)? FindInBank(string bankPath, SoundPermutation lookingForPermutation) {
        var bankMap = BuildBankMap(bankPath);
        string soundTagPath = lookingForPermutation.belongsToRange.belongsToTag.sourceTag.Path.RelativePath;

        // Forgive null, because these paths cannot possibly be null unless Bungie put tags in the root of the drive.
        string parentOfTag = Path.GetDirectoryName(soundTagPath)!;
        string parentOfParent = Path.GetDirectoryName(parentOfTag)!;

        foreach (var kv in bankMap) {
            int index = kv.Key;
            var sample = kv.Value.sample;
            string originalPath = kv.Value.infoFilePath;

            if (!originalPath.Contains(parentOfTag) || !originalPath.Contains(parentOfParent)) continue;
            Logger.Debug($"Considering sample #{index} because sample path {originalPath} contains sound tag parent {Path.GetFileName(parentOfTag)} and its parent {Path.GetFileName(parentOfParent)}.");

            if (!sample.Name!.Contains(lookingForPermutation.name, StringComparison.OrdinalIgnoreCase)) continue;
            Logger.Debug($"Still considering sample #{index} because sample name {sample.Name} contains permutation name {lookingForPermutation.name}.");

            return (sample, Path.ChangeExtension(originalPath, null));
        }

        return null;
    }
    #endregion

    #region Bank Handling
    private static FmodSoundBank GetBank(string path) {
        if (bankCache.TryGetValue(path, out var bank))
            return bank;

        byte[] bytes = File.ReadAllBytes(path);
        var newBank = FsbLoader.LoadFsbFromByteArray(bytes);

        bankCache.Add(path, newBank);

        return newBank;
    }

    internal static Dictionary<int, (FmodSample sample, string infoFilePath)> BuildBankMap(string bankPath) {
        var outDict = new Dictionary<int, (FmodSample sample, string infoFilePath)>();

        var bank = GetBank(bankPath);
        string bankInfoPath = Path.ChangeExtension(bankPath, "fsb.info");
        string[] infoPaths = TryReadInfoFile(bankInfoPath);

        if (bank.Samples.Count != infoPaths.Length)
            throw new InvalidDataException($"Bank sample count does not match info file! ({bank.Samples.Count} vs {infoPaths.Length})");

        for (int i = 0; i < bank.Samples.Count; i++) {
            outDict.Add(i, (bank.Samples[i], infoPaths[i]));
        }

        return outDict;
    }
    #endregion

    #region Info File Processing
    internal static string[] TryReadInfoFile(string infoFilePath) {
        string raw = File.ReadAllText(infoFilePath);                            // Read the raw text
        string cleaned = new(raw.Where(c => !char.IsControl(c)).ToArray());     // Strip out most unreadable characters
        string newlined = cleaned.Replace("data\\", "\n");                      // Remove the leading `data\` and add a newline.
        string cleanedMatchingExtension = Regex.Replace(                        // Remove everything between each file extension and next line.
            newlined,
            @"(\.(aif|wav|mp3|ogg|flac))(?!\r?\n|sound\\).+",
            "$1",
            RegexOptions.IgnoreCase
        );

        string[] final = cleanedMatchingExtension                               // Change all file extensions to a fake one, in this case, .sound, so that we can more easily search through them.
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) // Turn into a string array to loop through.
            .Where(line => line.StartsWith("sound"))                            // Last minute filter; remove ANY lines that do not begin with "sound".
            .Select(n => Path.ChangeExtension(n, null)).ToArray();              // And finally remove all extensions to weed out differences which may confuse the exporter.

        return final;
    }
    #endregion

}