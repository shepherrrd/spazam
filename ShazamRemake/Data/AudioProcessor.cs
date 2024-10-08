﻿using NAudio.Wave;
using System.Security.Cryptography;
using System.Linq;

public class AudioProcessor
{
    public List<AudioChunk> BreakIntoChunks(Stream audioStream, int chunkDurationInSeconds)
    {
        List<AudioChunk> chunks = new List<AudioChunk>();

        // Use MediaFoundationReader to read different types of audio files
        using (var reader = new MediaFoundationReader(""))
        {
            int bytesPerSecond = reader.WaveFormat.AverageBytesPerSecond;
            int chunkSizeInBytes = bytesPerSecond * chunkDurationInSeconds;

            byte[] buffer = new byte[chunkSizeInBytes];
            int bytesRead;
            int index = 0;

            // Read the stream in chunks
            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] chunkData = new byte[bytesRead];
                System.Buffer.BlockCopy(buffer, 0, chunkData, 0, bytesRead);

                chunks.Add(new AudioChunk { Data = chunkData, Index = index++ });
            }
        }

        return chunks;
    }
    public List<AudioChunk> BreakIntoChunks(string filePath, int chunkDurationInSeconds)
{
        List<AudioChunk> chunks = new List<AudioChunk>();

        // Use MediaFoundationReader to read different types of audio files
        using (var reader = new MediaFoundationReader(filePath))
        {
            int bytesPerSecond = reader.WaveFormat.AverageBytesPerSecond;
            int chunkSizeInBytes = bytesPerSecond * chunkDurationInSeconds;

            byte[] buffer = new byte[chunkSizeInBytes];
            int bytesRead;
            int index = 0;

            // Read the stream in chunks
            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] chunkData = new byte[bytesRead];
                System.Buffer.BlockCopy(buffer, 0, chunkData, 0, bytesRead);

                chunks.Add(new AudioChunk { Data = chunkData, Index = index++ });
            }
        }

        return chunks;
    }


    private byte[] ExtractChunk(AudioFileReader reader, int chunkIndex, int durationInSeconds)
    {
        int bytesPerSecond = reader.WaveFormat.AverageBytesPerSecond;
        reader.CurrentTime = TimeSpan.FromSeconds(chunkIndex * durationInSeconds);
        var buffer = new byte[bytesPerSecond * durationInSeconds];
        reader.Read(buffer, 0, buffer.Length);
        return buffer;
    }

    public List<(double, double)> ExtractKeyPoints(byte[] chunkData)
    {
        // Use FFT to generate the frequency domain representation of the audio chunk.
        // Implement an FFT or use an audio library to get the frequency spectrum.

        // Simulate key frequency peaks from FFT result
        // Key points represent frequency peaks at certain times in the spectrogram
        List<(double frequency, double time)> keyPoints = new List<(double, double)>
        {
            (300, 1.2), (450, 1.5), (900, 2.0)  // Example peaks: frequency (Hz) and time (seconds)
        };

        // You'll need to replace this simulation with actual FFT and peak detection logic
        return keyPoints;
    }

    public string HashKeyPoints(List<(double frequency, double time)> keyPoints)
    {
        var keyPointsString = string.Join(",", keyPoints.Select(kp => $"{kp.frequency}:{kp.time}"));
        using (SHA256 sha256 = SHA256.Create())
        {
            return BitConverter.ToString(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(keyPointsString))).Replace("-", "");
        }
    }
}


public class AudioChunk
{
    public byte[] Data { get; set; }
    public int Index { get; set; }
}
