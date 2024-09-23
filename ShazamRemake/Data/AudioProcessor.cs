using System.Security.Cryptography;
using NAudio.Wave;

public class AudioProcessor
{
    public List<AudioChunk> BreakIntoChunks(string filePath, int chunkDurationInSeconds)
    {
        List<AudioChunk> chunks = new List<AudioChunk>();

        using (var reader = new AudioFileReader(filePath))
        {
            int totalChunks = (int)(reader.TotalTime.TotalSeconds / chunkDurationInSeconds);
            for (int i = 0; i < totalChunks; i++)
            {
                var chunk = ExtractChunk(reader, i, chunkDurationInSeconds);
                chunks.Add(new AudioChunk { Data = chunk, Index = i });
            }
        }

        return chunks;
    }

    private byte[] ExtractChunk(AudioFileReader reader, int chunkIndex, int durationInSeconds)
    {
        int sampleRate = reader.WaveFormat.SampleRate;
        int channels = reader.WaveFormat.Channels;
        int bytesPerSecond = reader.WaveFormat.AverageBytesPerSecond;

        reader.CurrentTime = TimeSpan.FromSeconds(chunkIndex * durationInSeconds);
        var buffer = new byte[bytesPerSecond * durationInSeconds];
        reader.Read(buffer, 0, buffer.Length);

        return buffer;
    }

    public string HashFingerprint(byte[] chunkData)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return BitConverter.ToString(sha256.ComputeHash(chunkData)).Replace("-", "");
        }
    }
}

public class AudioChunk
{
    public byte[] Data { get; set; }
    public int Index { get; set; }
}
