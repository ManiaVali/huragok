
using NAudio.Wave;
using NVorbis;

namespace Huragok.Utilities.Sound {

    public class VorbisSampleProvider : ISampleProvider {
        private readonly VorbisReader vorbis;
        private readonly bool looping;

        public WaveFormat WaveFormat { get; }

        public double Progress => this.vorbis.TotalSamples > 0
            ? (double)this.vorbis.SamplePosition / this.vorbis.TotalSamples : 0f;

        public VorbisSampleProvider(Stream stream, bool loop = false) {
            this.vorbis = new VorbisReader(stream, false);
            this.looping = loop;

            this.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(
                this.vorbis.SampleRate,
                this.vorbis.Channels
            );
        }

        public int Read(float[] buffer, int offset, int count) {
            int totalRead = 0;

            while (totalRead < count) {
                int read = this.vorbis.ReadSamples(buffer, offset + totalRead, count - totalRead);

                if (read == 0) {
                    if (!this.looping) break;

                    // rewind seamlessly
                    this.vorbis.SamplePosition = 0;
                    continue;
                }

                totalRead += read;
            }

            return totalRead;
        }
    }

    public class VorbisSoundPlayer : IDisposable {
#pragma warning disable CS8618 // Shut up about nulls
        private WaveOutEvent output;
        private VorbisSampleProvider provider;
        private MemoryStream stream;

        private byte[] originalData;

        public double Progress => this.provider.Progress * 100;
        public int ProgressInteger => (int)Math.Round(this.Progress);
#pragma warning restore CS8618

        public void Load(byte[] vorbisBytes, bool looping = false) {
            this.originalData = vorbisBytes;

            this.stream = new MemoryStream(vorbisBytes);
            this.provider = new VorbisSampleProvider(this.stream, looping);

            this.output = new WaveOutEvent();
            this.output.Init(this.provider);
        }

        public void Play() => this.output?.Play();
        public void Pause() => this.output?.Pause();

        public void Reset(bool looping = false) {
            this.output?.Stop();

            // Reload
            this.Dispose();
            this.Load(this.originalData, looping);
            this.Play();
        }

        public PlaybackState State => this.output?.PlaybackState ?? PlaybackState.Stopped;

        public void Dispose() {
            GC.SuppressFinalize(this);

            this.output?.Dispose();
            this.stream?.Dispose();
        }
    }
}