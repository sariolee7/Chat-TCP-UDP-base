using System.Collections.Generic;
using System.IO;

public class UDPChunkReceiver
{
    private class ChunkBuffer
    {
        public int TotalChunks;
        public byte[][] Chunks;
        public int ReceivedCount;
    }

    private Dictionary<int, ChunkBuffer> chunkBuffers =
        new Dictionary<int, ChunkBuffer>();

    public byte[] HandlePacket(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            int marker = reader.ReadInt32();

            if (marker != -1)
            {
                return data;
            }

            int messageId = reader.ReadInt32();
            int chunkIndex = reader.ReadInt32();
            int totalChunks = reader.ReadInt32();
            int length = reader.ReadInt32();

            byte[] chunkData = reader.ReadBytes(length);

            if (!chunkBuffers.ContainsKey(messageId))
            {
                chunkBuffers[messageId] = new ChunkBuffer
                {
                    TotalChunks = totalChunks,
                    Chunks = new byte[totalChunks][],
                    ReceivedCount = 0
                };
            }

            ChunkBuffer buffer = chunkBuffers[messageId];

            if (buffer.Chunks[chunkIndex] == null)
            {
                buffer.Chunks[chunkIndex] = chunkData;
                buffer.ReceivedCount++;
            }

            if (buffer.ReceivedCount == buffer.TotalChunks)
            {
                using (MemoryStream full = new MemoryStream())
                {
                    for (int i = 0; i < buffer.TotalChunks; i++)
                        full.Write(buffer.Chunks[i], 0, buffer.Chunks[i].Length);

                    chunkBuffers.Remove(messageId);

                    return full.ToArray();
                }
            }
        }

        return null;
    }
}