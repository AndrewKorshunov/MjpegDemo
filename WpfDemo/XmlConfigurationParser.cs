using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.Dynamic;
using System.Configuration;
using System.Xml.Serialization;

namespace DemoLibrary
{
    public static class XmlConfigurationParser
    {
        private static readonly string url;
        private static readonly XmlDocument xml;
                
        private static IEnumerable<Dictionary<string, string>> Channels;
        private static Dictionary<string, Dictionary<string, string>> Configs;
        private static Dictionary<string, string> Server;

        static XmlConfigurationParser()
        {
            url = ConfigurationManager.AppSettings["Scheme"] + @"://"
                + ConfigurationManager.AppSettings["ConfigurationAddress"]
                + "?login="
                + ConfigurationManager.AppSettings["Login"];
            xml = new XmlDocument();
            xml.Load(url);
        }

        static public Dictionary<string,string> GetVideoServer()
        {
            if (Server == null)
            {
                var serversNode = xml.GetElementsByTagName("ServerInfo").Item(0);
                var server = new Dictionary<string, string>();
                server["Id"] = serversNode.Attributes["Id"].Value;
                server["Name"] = serversNode.Attributes["Name"].Value;
                server["Url"] = serversNode.Attributes["Url"].Value;
                Server = server;
            }
            return Server;
        }

        static public IEnumerable<Dictionary<string, string>> GetVideoChannels()
        {
            if (Channels == null)
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
                Channels = channels;
            }
            return Channels;
        }

        // Dictionary<Dictionary> so you can acces config parameters by Config["High"]
        static public Dictionary<string, Dictionary<string, string>> GetVideoConfigs()
        {
            if (Configs == null)
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
                    config["Quality"] = quality;
                    configs[quality] = config;
                }
                Configs = configs;
            }
            return Configs;
        }
    }    
}