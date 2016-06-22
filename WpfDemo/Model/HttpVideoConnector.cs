using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;
using MjpegLibrary;

namespace WpfDemo.Model
{
    public class HttpVideoConnector : IStreamSource
    {
        private readonly WebClient webClient;
        private string address;

        public HttpVideoConnector()
        {
            this.webClient = new WebClient();
        }

        public Stream GetStream()
        {
            return webClient.OpenRead(address);
        }

        public Task<Stream> GetStreamAsync()
        {            
            return webClient.OpenReadTaskAsync(address);
        }

        public void Setup(VideoChannel channel, VideoConfiguration config)
        {
            this.address = BuildAddress(channel, config);
        }

        private string BuildAddress(VideoChannel videoChannel, VideoConfiguration config)
        {
            // Still faster than stringBuilder
            string scheme = ConfigurationManager.AppSettings["Scheme"]; // protocol
            string login = ConfigurationManager.AppSettings["Login"];

            string hostname = scheme + @"://" + videoChannel.ServerUrl;
            string querry = "/mobile?"
                + "login=" + login
                + "&channelid=" + videoChannel.Id
                + "&resolutionX=" + config.ResolutionX
                + "&resolutionY=" + config.ResolutionY
                + "&fps=" + config.Fps;

            return hostname + querry;
        }
    }
}
