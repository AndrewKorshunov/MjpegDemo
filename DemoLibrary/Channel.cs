using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLibrary
{
    public class VideoServer
    {
        //public Dictionary<string, VideoChannel> Channels { get; set; }

        public string Id { get; set; }        
        public string Login { get; private set; }
        public string Name { get; set; }
        public string VideoSourceUrl { get; set; } // demo.com/mobile

        public VideoServer()
        {
            //Channels = new Dictionary<string, VideoChannel>();
            this.Login = "root";
        }

        public void LoadFromConfig(dynamic server)
        {
            this.Id = server["Id"];
            this.Name = server["Name"];
            this.VideoSourceUrl = server["Url"];
        }
    }

    public class VideoChannel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public VideoServer Server { get; set; }

        public void AttachToServer(VideoServer server)
        {
            //server.Channels[this.Id] = this;
            this.Server = server;            
        }

        public void LoadFromConfig(dynamic videoChannel)
        {
            this.Id = videoChannel.Id;
            this.Name = videoChannel.Name;
        }
    }
    
    public class VideoConfiguration
    {
        public int ResolutionX { get; set; }
        public int ResolutionY { get; set; }
        public int Fps { get; set; }

        public void LoadFromConfig(dynamic videoConfig)
        {
            this.ResolutionX = int.Parse(videoConfig["Width"]);
            this.ResolutionY = int.Parse(videoConfig["Height"]);
            this.Fps = int.Parse(videoConfig["FpsLimit"]);
        }
    }
}
