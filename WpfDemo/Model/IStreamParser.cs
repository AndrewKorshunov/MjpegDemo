using System;
using System.IO;

namespace WpfDemo.Model
{
    interface IStreamParser
    {
        event Action InvalidStream;
        event Action<byte[]> PacketFound;

        bool IsParsing { get; }

        void StartParsing(Stream stream);
        void StopParsing();
    }
}
