using System;
using System.IO;
using System.Drawing;

namespace WpfDemo.Model
{
    class MjpegHandler
    {
        public event Action PictureReady = () => { };        

        private readonly IStreamParser streamParser;
        private readonly IStreamSource streamSource;
        private Stream mjpegStream;
        
        public MjpegHandler(IStreamSource streamSource)
        {
            this.streamSource = streamSource;
            streamParser = new MjpegStreamParser("--myboundary\r\n");

            streamParser.InvalidStream += () =>
                {
                    // if stream is broken or stopped, dispose it and get new one
                    mjpegStream.Dispose();
                    StartNewStreamParsing();
                };

            streamParser.PacketFound += (packetBytes) =>
                {
                    // Not asynchronous jpeg parser
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
            streamParser.StartParsing(mjpegStream);
        }
    }
}
