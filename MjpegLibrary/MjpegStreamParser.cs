using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MjpegLibrary
{
    public class MjpegStreamParser : IStreamParser
    {
        public event Action InvalidStream = () => { };
        public event Action<byte[]> PacketFound = (bytes) => { };
        
        private readonly int bufferSize;
        private readonly string packetDelimiter;
        private bool shouldStop;

        public MjpegStreamParser(string packetDelimiter)
        {
            this.packetDelimiter = packetDelimiter;
            this.bufferSize = 4096; 
        }
        
        public bool IsParsing { get; private set; }

        public async void StartParsing(Stream mjpegStream)
        {
            // In this algorithm I assume that buffer always have either 1 or 0 packet delimiters in it,
            // and delimiter will always be at start of buffer, since I've seen only such packets.
            shouldStop = false;
            IsParsing = true;
            int bytesRead = 0;
            byte[] boundary = Encoding.UTF8.GetBytes(packetDelimiter); // Should get encoding from webResponse
            byte[] buffer = new byte[bufferSize];
            var packetBytes = new List<byte>();

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
                if (bytesRead == -1) // end of stream
                {                    
                    break;
                }

                int boundaryStartsAt = FindStartOfSubArray(buffer, boundary);
                if (boundaryStartsAt == 0 && packetBytes.Count != 0) // start of new packet found in buffer
                {
                    PacketFound(packetBytes.ToArray());
                    packetBytes.Clear();
                }
                packetBytes.AddRange(buffer.Take(bytesRead));
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
            int startIndex = 0; // searching in array from this index
            int startOfSubarrayIndex; // first element of subarray found at
            while (leftToCheck >= subArray.Length)
            {
                startOfSubarrayIndex = Array.IndexOf(array, subArray[0], startIndex, leftToCheck - subArray.Length + 1);
                if (startOfSubarrayIndex == -1)
                    return -1;

                int i, p;
                // check for needle
                for (i = 0, p = startOfSubarrayIndex; i < subArray.Length; i++, p++)
                {
                    if (array[p] != subArray[i])
                    {
                        break;
                    }
                }
                if (i == subArray.Length)
                {
                    return startOfSubarrayIndex;
                }
                leftToCheck -= (startOfSubarrayIndex - startIndex + 1);
                startIndex = startOfSubarrayIndex + 1;
            }
            return -1;
        }
    }
}
