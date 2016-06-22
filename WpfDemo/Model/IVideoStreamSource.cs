using System;
using System.IO;
using System.Threading.Tasks;
using MjpegLibrary;

namespace WpfDemo.Model
{
    interface IVideoStreamSource : IStreamSource
    {
        void Setup(VideoChannel channel, VideoConfiguration config);
    }
}
