
using NAudio.Wave;
using NVorbis;

namespace Huragok.Utilities.Sound {
    public class SoundPlayer : IDisposable {
        private WaveOutEvent output;
        private BufferedWaveProvider provider;
        private VorbisReader vorbis;
        private MemoryStream stream;

        private byte[] originalData;

        public void Load(byte[] vorbisBytes, float bufferLength = 30) {
            this.originalData = vorbisBytes;

            this.stream = new MemoryStream(vorbisBytes);
            this.vorbis = new VorbisReader(this.stream, false);

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(
                this.vorbis.SampleRate,
                this.vorbis.Channels);

            this.provider = new BufferedWaveProvider(waveFormat) {
                BufferDuration = TimeSpan.FromSeconds(bufferLength + 5)
            };

            this.output = new WaveOutEvent();
            this.output.Init(this.provider);

            this.FillBuffer();
        }

        private void FillBuffer() {
            float[] readBuffer = new float[4096];
            byte[] byteBuffer = new byte[readBuffer.Length * sizeof(float)];

            int samplesRead;
            while ((samplesRead = this.vorbis.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0) {
                Buffer.BlockCopy(readBuffer, 0, byteBuffer, 0, samplesRead * sizeof(float));
                this.provider.AddSamples(byteBuffer, 0, samplesRead * sizeof(float));
            }
        }

        public void Play() => this.output?.Play();
        public void Pause() => this.output?.Pause();

        public void Reset() {
            this.output?.Stop();

            // Reload
            this.Dispose();
            this.Load(this.originalData);
            this.Play();
        }

        public PlaybackState State => this.output?.PlaybackState ?? PlaybackState.Stopped;

        public void Dispose() {
            GC.SuppressFinalize(this);

            this.output?.Dispose();
            this.vorbis?.Dispose();
            this.stream?.Dispose();
        }
    }
}