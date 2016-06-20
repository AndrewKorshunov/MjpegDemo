using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoLibrary;

namespace WpfDemo.Model
{
    class MjpegDemoModel
    {
        public IList<VideoChannel> Channels { get; private set; }

        public MjpegDemoModel()
        {
            Channels = new List<VideoChannel>();
        }
    }
}
