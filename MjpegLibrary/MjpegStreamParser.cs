using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MjpegLibrary
{
    public class MjpegStreamParser : IStreamParser
    {
        public event Action InvalidStream = () => { };
        public event Action<byte[]> PacketFound = (bytes) => { };

        private readonly byte[] delimiterBytes;
        private readonly byte[] buffer;
        private readonly int bufferSize;
        private readonly List<byte> packetBytes;
        private volatile bool shouldStop; // volitile because it will be accessed from different threads

        public MjpegStreamParser(string packetDelimiter)
        {
            this.delimiterBytes = Encoding.UTF8.GetBytes(packetDelimiter); // Could get encoding from webResponse
            this.bufferSize = 4096;
            this.buffer = new byte[bufferSize];
            this.packetBytes = new List<byte>();
        }

        public bool IsParsing { get; private set; }

        public async void StartParsing(Stream mjpegStream)
        {
            // In this algorithm I assume that buffer always have either 1 or 0 packet delimiters in it,
            // and delimiter will always be at start of buffer, since I've seen only such packets.
            this.shouldStop = false;
            this.IsParsing = true;
            int bytesRead = 0;
            this.packetBytes.Clear();

            while (!shouldStop)
            {
                try
                {
                    bytesRead = await mjpegStream.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);
                }
                catch // Stream exceptions?
                {
                    break;
                }
                if (bytesRead == 0) // end of stream
                    break;

                int boundaryStartsAt = FindStartOfSubArray(buffer, delimiterBytes);
                if (boundaryStartsAt == 0 && packetBytes.Count != 0) // start of new packet found in buffer
                {
                    PacketFound(packetBytes.ToArray());
                    packetBytes.Clear();
                }
                // Moving actual bytes from buffer to packetBytes
                var partialPacket = new byte[bytesRead];
                Array.Copy(buffer, partialPacket, bytesRead);
                packetBytes.AddRange(partialPacket);
            }
            // shouldStop == true or stream is broken at this point, so new stream is needed
            this.IsParsing = false;
            InvalidStream();
        }

        public void StopParsing()
        {
            this.shouldStop = true;
        }

        private int FindStartOfSubArray(byte[] array, byte[] subArray)
        {
            int leftToCheck = array.Length; // indexes of array left to check
            int searchStartIndex = 0; // searching in array from this index
            while (leftToCheck >= subArray.Length)
            {
                // first element of subarray found in array at
                int startOfSubArray = Array.IndexOf(array, subArray[0], searchStartIndex, leftToCheck - subArray.Length + 1);
                if (startOfSubArray == -1)
                    return -1;

                int i = 0;
                for (; i < subArray.Length; i++) // I assume that subarray.Length < array.Length
                {
                    if (array[startOfSubArray + i] != subArray[i])
                        break;
                }
                if (i == subArray.Length)
                    return startOfSubArray;

                searchStartIndex = startOfSubArray + 1;
                leftToCheck = array.Length - searchStartIndex;
            }
            return -1;
        }
    }
}
