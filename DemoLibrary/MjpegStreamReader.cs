using System;
using System.IO;
using System.Drawing;

namespace MjpegLibrary
{
    public class MjpegStreamReader
    {
        public event Action PictureReady = () => { };
        public event Action Starting = () => { };

        private Stream mjpegStream;
        private readonly IStreamParser streamParser;
        private readonly IStreamSource streamSource;

        public MjpegStreamReader(IStreamSource streamSource)
        {
            this.streamSource = streamSource;
            this.streamParser = new MjpegStreamParser("--myboundary\r\n"); // Bou

            this.streamParser.InvalidStream += () =>
                {
                    // if stream is broken or stopped, dispose it and get new one
                    mjpegStream.Dispose();
                    StartNewStreamParsing();
                };
            this.streamParser.PacketFound += (packetBytes) =>
                {
                    Frame = MjpegPacket.GetImageFromPacket(packetBytes);
                    PictureReady();
                };
        }

        public Image Frame { get; private set; }

        public void Start()
        {
            if (streamParser.IsParsing)
            {
                // StopParsing will eventually dispose old stream and get new one
                streamParser.StopParsing();
            }
            else
            {
                StartNewStreamParsing();
            }
        }

        private async void StartNewStreamParsing()
        {
            mjpegStream = await streamSource.GetStreamAsync().ConfigureAwait(false);
            Starting();
            streamParser.StartParsing(mjpegStream);
        }
    }
}
