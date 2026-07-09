using NAudio;
using NAudio.Wave;

namespace AI.Interviewer.Console;

public class ConvertToWav
{
    public static Stream GetWhisperCompatibleWavStream(string mp3FilePath)
    {
        var targetFormat = new WaveFormat(16000, 16, 1);
        var outputStream = new MemoryStream();

        using (var reader = new AudioFileReader(mp3FilePath))
        {
            using (var resampler = new MediaFoundationResampler(reader, targetFormat))
            {
                resampler.ResamplerQuality = 60;
                WaveFileWriter.WriteWavFileToStream(outputStream, resampler);
            }
        }
        outputStream.Position = 0;
        return outputStream;
    }
}
