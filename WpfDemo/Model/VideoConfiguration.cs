using System;
using System.Collections.Generic;

namespace WpfDemo.Model
{
    public class VideoConfiguration
    {
        public int Fps { get; set; }
        public int ResolutionX { get; set; }
        public int ResolutionY { get; set; }
        
        public static VideoConfiguration LoadFromConfig(IDictionary<string, string> videoConfig)
        {
            var config = new VideoConfiguration();
            config.ResolutionX = int.Parse(videoConfig["Width"]);
            config.ResolutionY = int.Parse(videoConfig["Height"]);
            config.Fps = int.Parse(videoConfig["Fps"]);
            return config;
        }
    }
}
