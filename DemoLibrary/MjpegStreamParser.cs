using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DemoLibrary
{
    public class MjpegStreamPacketParser
    {
        private readonly string packetDelimiter;
        //private Stream mjpegStream;

        public MjpegPacket CurrentPacket { get; private set; }
        public event Action PacketFound;
        public event Action InvalidStream;
        public event Action Stopped;
        public Stream mjpegStream;

        private bool stopped = false;

        public MjpegStreamPacketParser(string packetDelimiter = "--myboundary")
        {
            this.packetDelimiter = packetDelimiter;

            this.InvalidStream += () => { };
        }

        public void BadParsePackets(Stream mjpegStream) // bad parser
        {
            var boundary = new byte[] { 45, 45, 109, 121, 98, 111, 117, 110, 100, 97, 114, 121, 13, 10 };
            int boundaryIndex = 0;
            var packetBytes = new List<byte>();
            bool delimiterFound = false;

            //bool onetime = false;

            while (true)
            {
                if (!mjpegStream.CanRead) // stream closed?
                {
                    InvalidStream();
                    return;
                }

                int rint = mjpegStream.ReadByte();

                if (rint == -1) // stream is broken?
                {
                    InvalidStream();
                    return;
                }

                if (delimiterFound)
                    packetBytes.Add((byte)rint);

                if (rint == boundary[boundaryIndex]) // boundary started?
                    boundaryIndex++;
                else
                    boundaryIndex = 0;

                if (boundaryIndex == boundary.Length) // found boundary
                {
                    if (delimiterFound) // It will skip first --myboundary
                    {
                        var withoutSecondBoundary = packetBytes.Take(packetBytes.Count - boundary.Length).ToArray();

                        this.CurrentPacket = new MjpegPacket(withoutSecondBoundary.ToArray()); // bad parser
                        PacketFound();
                        packetBytes.Clear();
                    }
                    delimiterFound = true;
                    boundaryIndex = 0;
                }
            }
        }

        public void ParseHeaderSlowPacketFast(Stream mjpegStream) // Working new parser, bad code quality??
        {
            this.mjpegStream = mjpegStream;
            var contentBytes = Encoding.Default.GetBytes("Content-Length: ");

            int estimatedHeaderSize = 256;
            var buffer = new byte[estimatedHeaderSize];
            
            // in one cycle this parses whole packet
            while (!stopped) // Cancellation?
            {
                if (!mjpegStream.CanRead) // stream closed?
                {
                    InvalidStream();
                    return;
                }
                int bytesRead = mjpegStream.Read(buffer, 0, estimatedHeaderSize);
                if (bytesRead != estimatedHeaderSize) { }
                if (bytesRead == -1) // stream is broken?
                {
                    InvalidStream();
                    return;
                }
                int contentIndex = 0; // Current byte of content found in buffer
                int contentLength = 0; // Length of image in current packet
                int startOfImage = 0;
                for (int i = 0; i < estimatedHeaderSize; i++) // look for Content-Length to know image size
                {
                    if (contentIndex == contentBytes.Length) // found content in buffer
                    {
                        string contentLngth = Encoding.Default.GetString(buffer.Skip(i).TakeWhile(x => x != '\r').ToArray());
                        contentLength = int.Parse(contentLngth);
                        startOfImage = i + contentLngth.Length + 4; // 4 from two crlf at the end of header
                        break;
                    }
                    if (buffer[i] == contentBytes[contentIndex]) // content started?
                        contentIndex++;
                    else
                        contentIndex = 0;
                }               
                var imageBytes = new byte[contentLength];
                Array.Copy(buffer, startOfImage, imageBytes, 0, buffer.Length - startOfImage);
                contentLength -= buffer.Length - startOfImage; // Current buffer already has part of image

                while (contentLength != 0) // how to improve this???
                {
                    if (!mjpegStream.CanRead) 
                    {
                        InvalidStream();
                        return;
                    }
                    int imageBytesRead = mjpegStream.Read(imageBytes, imageBytes.Length-contentLength, contentLength);
                    contentLength -= imageBytesRead;
                }
                this.CurrentPacket = new MjpegPacket(imageBytes);
                PacketFound();
                //System.Threading.Thread.Sleep(10);
            }

            if (stopped) // stopped = true, if code is here
            {                
                stopped = false;
                mjpegStream.Dispose();
                mjpegStream = null;
                Stopped();
            }
        }

        public void ParseWholePacketsByBuffer(Stream mjpegStream) // try new parser
        {
            var boundary = new byte[] { 45, 45, 109, 121, 98, 111, 117, 110, 100, 97, 114, 121, 13, 10 };
            int boundaryIndex = 0;

            int bufferSize = 1024 * 4;
            var buffer = new byte[bufferSize];
            
            while (true) // Cancellation?
            {
                if (!mjpegStream.CanRead) // stream closed?
                {
                    InvalidStream();
                    return;
                }
                int bytesRead = mjpegStream.Read(buffer, 0, bufferSize);
                if (bytesRead != bufferSize) { }
                if (bytesRead == -1) // stream is broken?
                {
                    InvalidStream();
                    return;
                }
                var strin = Encoding.Default.GetString(buffer);
                
                int startOfPacket = 0;
                for (int i = 0; i < bufferSize; i++) // look for Content-Length to know image size
                {
                    if (boundaryIndex == boundary.Length) // found content in buffer
                    {
                        startOfPacket = i;
                    }

                    if (buffer[i] == boundary[boundaryIndex]) // content started?                    
                        boundaryIndex++;                    
                    else                    
                        boundaryIndex = 0;                    
                }
            }
        }

        public void StopParsing()
        {
            stopped = true;
        }
    }

    public class MjpegPacket
    {
        public List<string> Header { get; private set; }
        public Image Picture { get; private set; }        

        private readonly byte[] packetBytes;

        public MjpegPacket(byte[] packetBytes)
        {
            this.packetBytes = packetBytes;
            this.Picture = ParsePacketBytes(this.packetBytes);
        }
        
        private Image ParsePacketBytes(byte[] packetBytes) // Need full parser
        {
            var jpegStart = new byte[] { 255, 216 };
            int startOfJpeg = FindSubArray(packetBytes, jpegStart, 0, packetBytes.Length);
            var imageBytes = new byte[packetBytes.Length - startOfJpeg];
            Array.Copy(packetBytes, startOfJpeg, imageBytes, 0, imageBytes.Length);
            var ms = new MemoryStream(imageBytes);
            var image = Image.FromStream(ms);
            //PacketParsed();
            return image;
        }

        private int FindSubArray(byte[] array, byte[] needle, int startIndex, int sourceLength)
        {
            int needleLen = needle.Length;
            int index;

            while (sourceLength >= needleLen)
            {
                // find needle's starting element
                index = Array.IndexOf(array, needle[0], startIndex, sourceLength - needleLen + 1);
                // if we did not find even the first element of the needls, then the search is failed
                if (index == -1)
                    return -1;
                int i, p;
                // check for needle
                for (i = 0, p = index; i < needleLen; i++, p++)
                {
                    if (array[p] != needle[i])
                    {
                        break;
                    }
                }
                if (i == needleLen)
                {
                    // needle was found
                    return index;
                }
                // continue to search for needle
                sourceLength -= (index - startIndex + 1);
                startIndex = index + 1;
            }
            return -1;
        }
    }
}
