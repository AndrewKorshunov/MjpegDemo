using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows;
using WpfDemo.Model;
using MjpegLibrary;

namespace WpfDemo.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapImage currentFrame;
        private readonly IVideoStreamSource httpSource;
        private bool isConnecting;
        private readonly MjpegStreamReader mjpegReader;

        public MainViewModel()
        {
            LoadSelectedChannelCommand = new LoadChannelCommand(LoadSelectedChannel, () => !isConnecting);
            httpSource = new HttpVideoConnector();
            mjpegReader = new MjpegStreamReader(httpSource);
            
            PopulateChannels();
            SetupDefaultConfig();

            mjpegReader.Starting += () => this.IsConnecting = false;
            mjpegReader.PictureReady += () => 
                Application.Current.Dispatcher.InvokeAsync(() => DisplayPicture(mjpegReader.Frame));
        }

        public IEnumerable<VideoChannel> Channels { get; set; }
        public BitmapImage CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = value;
                OnPropertyChangedEvent("CurrentFrame");
            }
        }
        public VideoConfiguration DefaultConfig { get; private set; }
        public bool IsConnecting
        {
            get { return isConnecting; }
            private set
            {
                isConnecting = value;
                OnPropertyChangedEvent("IsConnecting");
            }
        }
        public ICommand LoadSelectedChannelCommand { get; set; }
        public VideoChannel SelectedChannel { get; set; }

        protected void OnPropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DisplayPicture(System.Drawing.Image picture)
        {
            var bitmapImage = ImageToBitmap(picture);
            this.CurrentFrame = bitmapImage;
        }

        private BitmapImage ImageToBitmap(System.Drawing.Image picture)
        {
            var bitmap = new BitmapImage();
            var memoryStream = new MemoryStream();
            picture.Save(memoryStream, picture.RawFormat);
            memoryStream.Seek(0, SeekOrigin.Begin);
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            return bitmap;
        }

        private void LoadSelectedChannel()
        {
            if (SelectedChannel == null)
                MessageBox.Show("Select channel first");
            else
            {
                this.IsConnecting = true;                
                httpSource.Setup(SelectedChannel, DefaultConfig);
                mjpegReader.Start();
            }
        }
        
        private void PopulateChannels()
        {
            var channels = new List<VideoChannel>();
            foreach (var channelsDictionary in XmlConfigurationParser.GetVideoChannels())
            {
                var channel = VideoChannel.LoadFromConfig(channelsDictionary);
                channels.Add(channel);
            }
            Channels = channels;
        }

        private void SetupDefaultConfig()
        {            
            var allConfigs = XmlConfigurationParser.GetVideoConfigs();
            DefaultConfig = new VideoConfiguration();
            DefaultConfig = VideoConfiguration.LoadFromConfig(allConfigs["High"]);
            DefaultConfig.Fps = 1;
        }
    }
}
