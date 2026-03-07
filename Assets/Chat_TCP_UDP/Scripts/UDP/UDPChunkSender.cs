using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UDPChunkSender
{
    const int CHUNK_SIZE = 1000;

    private int messageCounter = 0;

    public List<byte[]> CreateChunks(byte[] data)
    {
        List<byte[]> packets = new List<byte[]>();

        int messageId = messageCounter++;
        int totalChunks = Mathf.CeilToInt((float)data.Length / CHUNK_SIZE);

        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * CHUNK_SIZE;
            int size = Mathf.Min(CHUNK_SIZE, data.Length - offset);

            byte[] chunk = new byte[size];
            Buffer.BlockCopy(data, offset, chunk, 0, size);

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(-1);           
                writer.Write(messageId);
                writer.Write(i);
                writer.Write(totalChunks);
                writer.Write(chunk.Length);
                writer.Write(chunk);

                packets.Add(ms.ToArray());
            }
        }

        return packets;
    }

    public bool NeedsChunking(byte[] data)
    {
        return data.Length > CHUNK_SIZE;
    }
}