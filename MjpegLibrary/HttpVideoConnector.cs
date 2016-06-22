using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DemoLibrary
{
    public class HttpVideoConnector
    {
        private WebClient webClient;

        public HttpVideoConnector()
        {
            webClient = new WebClient();
        }

        public Stream Connect(VideoChannel videoChannel, VideoConfiguration config)
        {
            string address = BuildAddress(videoChannel, config);
            if (webClient.ResponseHeaders!=null)  // already open?
            {
            }
            return webClient.OpenRead(address);
        }

        public Task<Stream> ConnectTaskAsync(VideoChannel videoChannel, VideoConfiguration config)
        {
            string address = BuildAddress(videoChannel, config);
            return webClient.OpenReadTaskAsync(address);
        }

        private string BuildAddress(VideoChannel videoChannel, VideoConfiguration config)
        {
            /*
            var sb = new StringBuilder();
            sb.Append(videoChannel.Server.VideoSourceUrl);
            sb.Append("mobile?");
            sb.Append("login=" + videoChannel.Server.Login);
            sb.Append("&channelid=" + videoChannel.Id);
            sb.Append("&resolutionX=" + videoChannel.SelectedConfig.ResolutionX);
            sb.Append("&resolutionY=" + videoChannel.SelectedConfig.ResolutionY);
            sb.Append("&fps=" + videoChannel.SelectedConfig.Fps);
            var output = sb.ToString();
            return output; 
             */
            string hostname = @"http://"+videoChannel.Server.VideoSourceUrl.Remove(videoChannel.Server.VideoSourceUrl.Length - 5);
            string querry = "/mobile?";
            querry += "login=" + videoChannel.Server.Login;
            querry += "&channelid=" + videoChannel.Id;
            querry += "&resolutionX=" + config.ResolutionX;
            querry += "&resolutionY=" + config.ResolutionY;
            querry += "&fps=" + config.Fps;
            return hostname + querry;
        }
    }
}
