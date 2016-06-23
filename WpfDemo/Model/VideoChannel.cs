using System;
using System.Collections.Generic;

namespace WpfDemo.Model
{
    public class VideoChannel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ServerUrl { get; set; }

        public static VideoChannel LoadFromConfig(IDictionary<string, string> videoChannel)
        {
            var channel = new VideoChannel();
            channel.Id = videoChannel["Id"];
            channel.Name = videoChannel["Name"];
            channel.ServerUrl = videoChannel["ServerUrl"];
            return channel;            
        }
    }
}
