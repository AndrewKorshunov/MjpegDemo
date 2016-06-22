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
using WpfDemo.Model;

namespace WpfDemo.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MjpegHandler streamHandler;
        private readonly HttpVideoConnector connector;
        private BitmapImage currentFrame;
        private bool isConnecting;

        public MainViewModel()
        {
            LoadSelectedChannelCommand = new LoadChannelCommand(() => LoadSelectedChannel());
            Channels = new List<VideoChannel>();
            connector = new HttpVideoConnector();
            streamHandler = new MjpegHandler(connector);
            PopulateChannels();

            streamHandler.PictureReady +=
                () => Application.Current.Dispatcher.InvokeAsync(() => DisplayPicture(streamHandler.Frame));
        }
                
        public bool IsConnecting
        {
            get { return isConnecting; }
            private set
            {
                isConnecting = value;
                OnPropertyChangedEvent("IsConnecting");
            }
        }
        public IList<VideoChannel> Channels { get; set; }
        public BitmapImage CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = value;
                OnPropertyChangedEvent("CurrentFrame");
            }
        }
        public VideoChannel SelectedChannel { get; set; }
        public ICommand LoadSelectedChannelCommand { get; set; }

        private void LoadSelectedChannel()
        {
            if (SelectedChannel == null)
                MessageBox.Show("Select channel first");
            else
            {
                var allConfigs = XmlConfigurationParser.GetVideoConfigs();
                var videoConfig = VideoConfiguration.LoadFromConfig(allConfigs["High"]);
                videoConfig.Fps = 1;
                connector.Setup(SelectedChannel, videoConfig);
                streamHandler.Start();
            }
        }

        private void DisplayPicture(System.Drawing.Image picture)
        {
            var bitmapImage = ImageToBitmap(picture);
            this.CurrentFrame = bitmapImage;
        }

        private BitmapImage ImageToBitmap(System.Drawing.Image picture)
        {
            var bitmap = new BitmapImage();
            var memoryStream = new System.IO.MemoryStream();
            picture.Save(memoryStream, picture.RawFormat);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            return bitmap;
        }

        private void PopulateChannels()
        {
            foreach (var channelsDictionary in XmlConfigurationParser.GetVideoChannels())
            {
                var channel = VideoChannel.LoadFromConfig(channelsDictionary);
                Channels.Add(channel);
            }
        }

        protected void OnPropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
