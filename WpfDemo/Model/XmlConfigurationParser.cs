using System;
using System.Collections.Generic;
using System.Xml;
using System.Configuration;

namespace WpfDemo.Model
{
    public static class XmlConfigurationParser
    {
        private static readonly string url;
        private static readonly XmlDocument xml;

        private static IEnumerable<Dictionary<string, string>> cachedChannels;
        private static Dictionary<string, Dictionary<string, string>> cachedConfigs;
        private static Dictionary<string, string> cachedServer;

        static XmlConfigurationParser()
        {
            url = ConfigurationManager.AppSettings["Scheme"] + @"://"
                + ConfigurationManager.AppSettings["ConfigurationAddress"]
                + "?login=" + ConfigurationManager.AppSettings["Login"];
            xml = new XmlDocument();
            xml.Load(url);
        }

        public static IEnumerable<Dictionary<string, string>> GetVideoChannels()
        {
            if (cachedChannels == null)
            {
                var channels = new List<Dictionary<string, string>>();
                var channelsNode = xml.GetElementsByTagName("Channels").Item(0);
                foreach (XmlNode channelInfo in channelsNode.ChildNodes)
                {
                    var channel = new Dictionary<string, string>();
                    channel["Id"] = channelInfo.Attributes["Id"].Value;
                    channel["Name"] = channelInfo.Attributes["Name"].Value;
                    channel["ServerUrl"] = GetVideoServer()["Url"];
                    channels.Add(channel);
                }
                cachedChannels = channels;
            }
            return cachedChannels;
        }

        // Dictionary<Dictionary> so you can acces config parameters by Config["High"]
        public static Dictionary<string, Dictionary<string, string>> GetVideoConfigs()
        {
            if (cachedConfigs == null)
            {
                var configs = new Dictionary<string, Dictionary<string, string>>();
                var configsNode = xml.GetElementsByTagName("Resolutions").Item(0);
                foreach (XmlNode configNode in configsNode.ChildNodes)
                {
                    var config = new Dictionary<string, string>();
                    config["Width"] = configNode.Attributes["Width"].Value;
                    config["Height"] = configNode.Attributes["Height"].Value;
                    config["Fps"] = configNode.Attributes["FpsLimit"].Value;
                    string quality = configNode.Attributes["Type"].Value;
                    config["Type"] = quality;
                    configs[quality] = config;
                }
                cachedConfigs = configs;
            }
            return cachedConfigs;
        }

        public static Dictionary<string, string> GetVideoServer()
        {
            if (cachedServer == null)
            {
                var serversNode = xml.GetElementsByTagName("ServerInfo").Item(0);
                var server = new Dictionary<string, string>();
                server["Id"] = serversNode.Attributes["Id"].Value;
                server["Name"] = serversNode.Attributes["Name"].Value;
                server["Url"] = serversNode.Attributes["Url"].Value;
                cachedServer = server;
            }
            return cachedServer;
        }
    }
}