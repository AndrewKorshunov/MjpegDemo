using MjpegLibrary;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfDemo.Model;

namespace WpfDemo.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly HttpVideoConnector httpVideoSource;
        private readonly ImageSourceConverter imageConverter;
        private readonly MjpegStreamReader mjpegReader;
        private ImageSource currentFrame;
        private bool isConnecting;

        public MainViewModel()
        {
            // Button click will execute LoadSelectedChannel(), but only if channel is not loading right now
            LoadSelectedChannelCommand = new LoadChannelCommand(LoadSelectedChannel, () => !isConnecting);
            imageConverter = new ImageSourceConverter();
            httpVideoSource = new HttpVideoConnector();
            mjpegReader = new MjpegStreamReader(httpVideoSource);

            PopulateVideoChannels();
            SetupDefaultConfig();

            mjpegReader.Starting += () => this.IsConnecting = false;
            mjpegReader.PictureReady += () =>
                {
                    var bitmapImage = BytesToBitmapSource(mjpegReader.ImageBytes);
                    // Race condition - Application.Current == null when window is closing, but parsing thread returns new picture.
                    // Safety check, there should be another way to ensure safety.
                    //if (Application.Current != null)
                    //{
                    //Application.Current.Dispatcher.InvokeAsync(() => this.CurrentFrame = bitmapImage);
                    //}
                    this.CurrentFrame = bitmapImage;
                };
        }

        public IEnumerable<VideoChannel> Channels { get; private set; }
        public ImageSource CurrentFrame
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

        private ImageSource BytesToBitmapSource(byte[] imageBytes)
        {
            var bitmap = (ImageSource)imageConverter.ConvertFrom(imageBytes);
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
                VideoChannel channel = VideoChannel.LoadFromConfig(channelsDictionary);
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
