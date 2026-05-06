
using System.Text.RegularExpressions;
using Fmod5Sharp;
using Fmod5Sharp.FmodTypes;
using Huragok.Data.IntermediateFormats.Sound;
using Huragok.Configuration;

namespace Huragok.Utilities.Sound {
    // And the award for the most unreadable code goes to.. FSBExplorer!
    // TODO: Maybe add support for alternate language extraction?
    internal static class FSBExplorer {
        private static readonly string _hrFSBDir = Path.Combine(ConfigurationReader.Configuration.MCCInstallPath, "haloreach", "fmod", "pc");
        private static readonly string[] _hrFSBNames = ["english.fsb", "sfx.fsb"];

        private static readonly Dictionary<string, FmodSoundBank> _bankCache = new();

        private static string[] FSBFiles {
            get {
#if USING_BLAM_HR
                return _hrFSBNames.Select(hrFSBName => Path.Combine(_hrFSBDir, hrFSBName)).ToArray();
#endif
            }
        }

        // Don't enumerate all banks every single time, it takes FOREVER.
        private static List<FmodSample>? _allSamples;
        private static List<(string[] fileList, string fsbPath)>? _allFiles;
#pragma warning disable CS0618
        internal static List<FmodSample> AllSamples => _allSamples ?? EnumerateSoundBanks();
        internal static List<(string[] fileList, string fsbPath)> AllFiles => _allFiles ?? GetFSBFileList();
#pragma warning restore CS0618

        internal static (byte[] RawData, string Format, string samplePath) FindSample(string soundTagPath, string permSampleName) {
            var indexInBank = GetIndexInBank(soundTagPath, permSampleName)
                ?? throw new Exception($"Failed to get sound `{soundTagPath}::{permSampleName}` in sound banks.");

            var soundBank = GetBank(indexInBank.bankPath);
            var samples = soundBank.Samples;
            var finalSample = samples[indexInBank.index];

            if (finalSample is null || string.IsNullOrWhiteSpace(finalSample.Name))
                throw new Exception($"Found sample for `{soundTagPath}::{permSampleName}` in banks, but failed to extract it.");

            finalSample.RebuildAsStandardFileFormat(out byte[]? finalBytes, out string? fileExtension);

            if (finalBytes == null || fileExtension == null) {
                throw new Exception($"Failed to get sample for `{soundTagPath}::{permSampleName}` from FMOD bank; final bytes or extension was null!");
            }

            return (finalBytes, fileExtension, indexInBank.soundPathFromInfo);
        }

        internal static (byte[] RawData, string Format, string samplePath) FindSample(IF_SoundPermutation soundPermutation) {
            string sourceTagPath = Path.GetFullPath(Path.Combine("tags", soundPermutation.belongsToRange.belongsToTag.sourceTag.Path.RelativePath));
            return FindSample(sourceTagPath, soundPermutation.name);
        }

        [Obsolete($"Do not call {nameof(GetFSBFileList)}() directly, use {nameof(AllFiles)} or you are wasting time doing what's probably already been done.")]
        private static List<(string[] fileList, string fsbPath)> GetFSBFileList() {
            List<(string[], string)> outList = new();

            foreach (string fsbFile in FSBFiles) {
                string infoFile = Path.ChangeExtension(fsbFile, "fsb.info");

                // This is a binary file with some ASCII strings; requires a bit of work to decode into the format we need.

                string raw = File.ReadAllText(infoFile);                                // Read the raw text
                string cleaned = new(raw.Where(c => !char.IsControl(c)).ToArray());     // Strip out most unreadable characters
                string newlined = cleaned.Replace("data\\", "\n");                      // Remove the leading `data\` and add a newline.
                string cleanedMatchingExtension = Regex.Replace(                        // Remove everything between each file extension and next line.
                    newlined,
                    @"(\.(aif|wav|mp3|ogg|flac))(?!\r?\n|sound\\).+",
                    "$1",
                    RegexOptions.IgnoreCase
                );

                // Change all file extensions to a fake one, in this case, .sound, so that we can more easily search through them.
                string[] final = cleanedMatchingExtension
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) // Turn into a string array to loop through.
                    .Where(line => line.StartsWith("sound"))                            // Last minute filter; remove ANY lines that do not begin with "sound".
                    .Select(n => Path.ChangeExtension(n, null)).ToArray();              // And finally remove all extensions to weed out differences which may confuse the exporter.

                outList.Add((final, fsbFile));
            }

            _allFiles = outList;
            return outList;
        }

        private static (int index, string bankPath, string soundPathFromInfo)? GetIndexInBank(string soundTagPath, string permSampleName) {
            var fullList = AllFiles;
            
            bool badIndexWarn = false;
            foreach (var (fileList, fsbPath) in fullList) {
                for (int i = 0; i < fileList.Length; i++) {
                    string entry = fileList[i];
                    if (!entry.Contains(permSampleName)) continue;

                    string parent = Path.GetFileName(Path.GetDirectoryName(soundTagPath)) ?? "";
                    string parentOfParent = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(soundTagPath)) ?? "");
                    string entryName = Path.GetFileNameWithoutExtension(entry);

                    // If the file located at the index we found looks to be incorrect, check its nearest neighbors for better matches.
                    // This is kinda dumb. I'm still not sure what causes this; I suspect its a slight index-order problem creating during
                    //      GetFSBList's decoding of the FSB info file.
                    int bestMatchIndex = i;
                    if (entryName != permSampleName && !entryName.Contains(permSampleName)) {
                        const int CHECK_SURROUNDING_LENGTH = 5;
                        var surroundingRange = Enumerable.Range(
                            Math.Max(0, i - CHECK_SURROUNDING_LENGTH),
                            Math.Min(fileList.Length, CHECK_SURROUNDING_LENGTH * 2 + 1)
                        );

                        foreach (int idx in surroundingRange) {
                            string newEntry = fileList[idx];
                            string newEntryName = Path.GetFileNameWithoutExtension(newEntry);

                            if (newEntryName != permSampleName && !newEntryName.Contains(permSampleName))
                                continue;

                            if (!badIndexWarn) {
                                // Console.Error.WriteLine($"WARNING: While extracting audio permutation `{permSampleName}` on `{Path.GetFileName(soundTagPath)}`, it appeared that the wrong sample was initially selected for extraction. " +
                                //     $"{GlobalConstants.PROGRAM_NAME} attempted to correct this, and found a sample nearby which better matched. Verify that the sample extracted " +
                                //     $"contains the expected audio.");
                                badIndexWarn = true;
                            }
                            bestMatchIndex = idx;
                            entry = newEntry;
                            
                            break;
                        }
                    }

                    // After attempting to correct, if the sample is still wrong, give up.
                    if (fileList[bestMatchIndex] != permSampleName && !fileList[bestMatchIndex].Contains(permSampleName)) {
                        throw new Exception($"While extracting audio permutation `{permSampleName}` on `{Path.GetFileName(soundTagPath)}`, it appeared that the wrong sample was initially selected for extraction. " +
                            $"{GlobalConstants.PROGRAM_NAME} attempted to correct this, but could not find any samples nearby which better matched. The sound tag was not extracted.");
                    }

                    if (entry.Contains(parent) && entry.Contains(parentOfParent)) {
                        return (bestMatchIndex, fsbPath, fileList[bestMatchIndex]);
                    }
                }
            }
            return null;
        }

        private static FmodSoundBank GetBank(string path) {
            if (!_bankCache.TryGetValue(path, out var bank)) {
                byte[] bytes = File.ReadAllBytes(path);
                bank = FsbLoader.LoadFsbFromByteArray(bytes);
                _bankCache[path] = bank;
            }
            return bank;
        }

        [Obsolete($"Do not call {nameof(EnumerateSoundBanks)}() directly, use {nameof(AllSamples)} or you are wasting time doing what's probably already been done.")]
        private static List<FmodSample> EnumerateSoundBanks() {
            List<FmodSample> samples = new();
            foreach (string bankPath in FSBFiles) {
                var bank = GetBank(bankPath);

                samples = samples.Concat(bank.Samples).ToList();
            }
            _allSamples = samples;
            return samples;
        }
    }
}