using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows;
using MjpegLibrary;
using WpfDemo.Model;

namespace WpfDemo.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly HttpVideoConnector httpVideoSource;
        private readonly MjpegStreamReader mjpegReader;
        private BitmapImage currentFrame;
        private bool isConnecting;

        public MainViewModel()
        {
            // Button click will execute LoadSelectedChannel(), but only if channel is not loading right now
            LoadSelectedChannelCommand = new LoadChannelCommand(LoadSelectedChannel, () => !isConnecting);
            httpVideoSource = new HttpVideoConnector();
            mjpegReader = new MjpegStreamReader(httpVideoSource);

            PopulateVideoChannels();
            SetupDefaultConfig();

            mjpegReader.Starting += () => this.IsConnecting = false;
            mjpegReader.PictureReady += () =>
                {
                    // Race condition - Application.Current == null when window is closing, but parsing thread returns new picture.
                    // Safety check, there should be another way to ensure safety.
                    if (Application.Current != null) 
                        Application.Current.Dispatcher.InvokeAsync(() => DisplayPicture(mjpegReader.Frame));
                };
        }

        public IEnumerable<VideoChannel> Channels { get; private set; }
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
        public ICommand LoadSelectedChannelCommand { get; private set; }
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
            // MjpegLibrary providing an image which can't be used in WPF Image control,
            // so changing image type to right one is required.
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
                httpVideoSource.Setup(SelectedChannel, DefaultConfig);
                mjpegReader.Start();
            }
        }

        private void PopulateVideoChannels()
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
            string defaultQuality = System.Configuration.ConfigurationManager.AppSettings["DefaultQuality"];
            var allConfigs = XmlConfigurationParser.GetVideoConfigs();            
            DefaultConfig = new VideoConfiguration();
            DefaultConfig = VideoConfiguration.LoadFromConfig(allConfigs[defaultQuality]);
        }
    }
}
