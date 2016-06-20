using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoLibrary;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows;
using System.Configuration;

namespace WpfDemo.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapImage curFrame;

        public IList<VideoChannel> Channels { get; set; }
        public VideoChannel SelectedChannel { get; set; }        
        public BitmapImage CurrentFrame
        {
            get
            {
                return curFrame;
            }
            set
            {
                curFrame = value;
                OnPropertyChangedEvent("CurrentFrame");
            }        
        }
        public ICommand LoadSelectedChannelCommand { get; set; }

        private MjpegStreamPacketParser parser;

        public MainViewModel()
        {
            LoadSelectedChannelCommand = new LoadCommand(obj => LoadSelectedChannel());
            parser = new MjpegStreamPacketParser();
            parser.PacketFound += () =>
                {
                    Application.Current.Dispatcher.InvokeAsync(()=>{
                    var bitmapImage = ImageToBitmap(parser.CurrentPacket.Picture);
                    this.CurrentFrame = bitmapImage;}); // HACK
                };

            Channels = new List<VideoChannel>();

            var server = new VideoServer();
            server.LoadFromConfig(XmlConfigurationParser.GetVideoServer());
            foreach (var tt in XmlConfigurationParser.GetVideoChannels())
            {
                Channels.Add(new VideoChannel() { Name = tt["Name"], Id = tt["Id"], Server = server });
            }
        }

        private void LoadSelectedChannel()
        {
            if (SelectedChannel == null)
                MessageBox.Show("Select channel first");
            else
            {
                var stream = new HttpVideoConnector().Connect(SelectedChannel, new VideoConfiguration() { ResolutionX = 120, ResolutionY = 90 });
                if (parser.mjpegStream != null) // already running
                {
                    parser.StopParsing();
                    parser.Stopped += () =>
                    {
                        Task.Run(() => parser.ParseHeaderSlowPacketFast(stream));                        
                    };
                }
                else
                {
                    Task.Run(() => parser.ParseHeaderSlowPacketFast(stream)); // HACK HACK HACK
                }
            }
        }

        private BitmapImage ImageToBitmap(System.Drawing.Image picture)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            picture.Save(memoryStream, picture.RawFormat);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();

            return bitmap;
        }

        protected void OnPropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
