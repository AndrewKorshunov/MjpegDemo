using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoLibrary;

namespace WpfDemo.Model
{
    class MjpegVideo
    {
        public IList<VideoChannel> Channels { get; private set; }

        public MjpegVideo()
        {
            Channels = new List<VideoChannel>();
        }
    }
}
