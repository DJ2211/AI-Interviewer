using System;
using System.Collections.Generic;
using System.Text;

namespace AI.Interviewer.Services.Utilities
{
    public static class WavEncoder
    {
        /// <summary>
        /// Encode 32-bit float PCM samples (range -1.0 to 1.0) into a 16-bit mono WAV byte array.
        /// </summary>
        public static byte[] EncodeToWav(float[] samples, int sampleRate)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            short bitsPerSample = 16;
            short channels = 1;
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            short blockAlign = (short)(channels * (bitsPerSample / 8));
            int dataSize = samples.Length * (bitsPerSample / 8);

            // RIFF header
            writer.Write("RIFF"u8.ToArray());
            writer.Write(36 + dataSize);
            writer.Write("WAVE"u8.ToArray());

            // fmt subchunk
            writer.Write("fmt "u8.ToArray());
            writer.Write(16); // PCM subchunk size
            writer.Write((short)1); // PCM format
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            // data subchunk
            writer.Write("data"u8.ToArray());
            writer.Write(dataSize);

            foreach (var sample in samples)
            {
                short pcmSample = (short)(Math.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(pcmSample);
            }

            writer.Flush();
            return stream.ToArray();
        }
    }
}
