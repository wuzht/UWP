using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Media.Animation;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyMediaPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaPlayer _mediaPlayer = new MediaPlayer();
        MediaTimelineController _mediaTimelineController = new MediaTimelineController();
        TimeSpan _duration;
        bool isMediaStart = false;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 420, Height = 480 });
            var mediaSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Boyce Avenue - Game of Thrones (Main Theme).mp3"));
            mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
            _mediaPlayer.Source = mediaSource;
            _mediaPlayer.CommandManager.IsEnabled = false;
            _mediaPlayer.TimelineController = _mediaTimelineController;
            _mediaPlayerElement.SetMediaPlayer(_mediaPlayer);
        }

        private void AppBarButton_Click_Play(object sender, RoutedEventArgs e)
        {
            if (!isMediaStart)
            {
                isMediaStart = true;
                Debug.WriteLine("start");

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += TimerClick;
                timer.Start();

                if (timeLine.Value == 0)
                    _mediaTimelineController.Start();
                else
                    _mediaTimelineController.Resume();
                MyPlayBtn.Icon = new SymbolIcon(Symbol.Pause);
                MyStoryBoard.Begin();
            }
            else if (_mediaTimelineController.State == MediaTimelineControllerState.Paused)
            {
                Debug.WriteLine("resume");
                _mediaTimelineController.Resume();
                MyPlayBtn.Icon = new SymbolIcon(Symbol.Pause);
                MyStoryBoard.Resume();
            }
            else if (_mediaTimelineController.State == MediaTimelineControllerState.Running)
            {
                Debug.WriteLine("pause");
                _mediaTimelineController.Pause();
                MyPlayBtn.Icon = new SymbolIcon(Symbol.Play);
                MyStoryBoard.Pause();
            }       
        }

        void TimerClick(object sender, object e)
        {
            timeLine.Value = ((TimeSpan)_mediaTimelineController.Position).TotalSeconds;
            // 视频播放完毕
            if (timeLine.Value == timeLine.Maximum)
            {
                Reset();
            }
        }

        void Reset()
        {
            isMediaStart = false;
            MyPlayBtn.Icon = new SymbolIcon(Symbol.Play);
            _mediaTimelineController.Position = TimeSpan.FromSeconds(0);
            _mediaTimelineController.Pause();
            MyStoryBoard.Stop();
        }

        private void AppBarButton_Click_Stop(object sender, RoutedEventArgs e)
        {
            _mediaTimelineController.Pause();
            Reset();
        }

        private async void AppBarButton_Click_OpenFile(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            filePicker.FileTypeFilter.Add(".mp4");
            filePicker.FileTypeFilter.Add(".mp3");

            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                var mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
                _mediaPlayer.Source = mediaSource;
                Reset();
                if (file.FileType == ".mp3")
                {
                    MyEllipse.Visibility = Visibility.Visible;
                }
                else
                {
                    MyEllipse.Visibility = Visibility.Collapsed;
                }
            }
        }
   
        private void AppBarButton_Click_FullScreen(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        private async void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            _duration = sender.Duration.GetValueOrDefault();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                timeLine.Minimum = 0;
                timeLine.Maximum = _duration.TotalSeconds;
                timeLine.StepFrequency = 1;
            });
        }
    }
}
