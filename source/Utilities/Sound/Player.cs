
using NAudio.Wave;
using NVorbis;

namespace Huragok.Utilities.Sound {
    public class SoundPlayer {
        // Setting the buffer length from the sound tag's length means the ENTIRE SAMPLE is buffered at once.
        public static void PlayVorbis(byte[] vorbisBytes, float bufferLength = 30) {
            using var ms = new MemoryStream(vorbisBytes);
            using var vorbis = new VorbisReader(ms, false);

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(
                vorbis.SampleRate,
                vorbis.Channels);

            var provider = new BufferedWaveProvider(waveFormat);

            using var output = new WaveOutEvent();
            output.Init(provider);
            output.Play();

            float[] readBuffer = new float[4096];
            byte[] byteBuffer = new byte[readBuffer.Length * sizeof(float)];

            provider.BufferDuration = TimeSpan.FromSeconds(bufferLength + 5);

            int samplesRead;
            while ((samplesRead = vorbis.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0) {
                Buffer.BlockCopy(readBuffer, 0, byteBuffer, 0, samplesRead * sizeof(float));
                provider.AddSamples(byteBuffer, 0, samplesRead * sizeof(float));
            }

            // wait until playback finishes
            while (output.PlaybackState == PlaybackState.Playing) {
                Thread.Sleep(100);
            }
        }
    }
}