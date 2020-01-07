using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TagLib;
using Color = System.Windows.Media.Color;

namespace CSharp_firstApp
{
    static class Player
    {
        public static WaveOutEvent outputDevice;
        public static AudioFileReader audioFile;
        public static MainWindow MW;
        public static void Setup(MainWindow main)
        {
            MW = main;
            MW.volumeSlider.Value = 1;
            outputDevice = new WaveOutEvent();
            outputDevice.Volume = 0.1f;
        }

        public static int currSongId;
        public static ListViewItem previtem;
        public static Brush previtemBG;
        public static void Play(Song song)
        {
            string path = song.Path;
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
            }
            if (audioFile == null)
            {
                audioFile = new AudioFileReader(path);
                outputDevice.Init(audioFile);
            }
            outputDevice.Play();
            MW.playbackstopped = false;
            MW.playerPlayButtonImage.Source = new BitmapImage(new Uri(@"Resources/appbar.control.pause.png", UriKind.Relative));
            SongProgressUpdater();

            if (song.Title != null)
                MW.songNameLabel.Content = song.Title;
            else
                MW.songNameLabel.Content = song.FileName;


            if (song.Duration.TotalSeconds != 0)
                MW.songLengthLabel.Content = song.DurationString;
            else
            {
                System.Timers.Timer tmr = new System.Timers.Timer();
                tmr.Interval = 1000;
                tmr.Enabled = true;
                tmr.Elapsed += (sender, EventArgs) =>
                {
                    if (song.Duration.TotalSeconds != 0)
                    {
                        MW.Dispatcher.Invoke(() =>
                        {
                            MW.songLengthLabel.Content = song.DurationString;
                        });
                        tmr.Stop();
                    }
                };
            }
            if (song.Cover != null)
            {
                IPicture pic = song.Cover;
                MemoryStream memstrm = new MemoryStream(pic.Data.Data);
                memstrm.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = memstrm;
                bitmap.EndInit();

                MW.playerCoverImage.Source = bitmap;
            }
            else
                MW.playerCoverImage.Source = new BitmapImage(new Uri(@"Resources/cd_icon.jpg", UriKind.Relative));

            if (previtem != null)
                previtem.Background = new System.Windows.Media.SolidColorBrush(Color.FromRgb(255, 255, 255));
           //     previtem.Background = previtemBG;
            ListViewItem item = MW.listview_playlist.ItemContainerGenerator.ContainerFromIndex(MW.currlistitemindex) as ListViewItem;
            previtemBG = item.Background;
            MessageBox.Show(item.Background.ToString());
            previtem = item;
            item.Background = new System.Windows.Media.SolidColorBrush(Color.FromRgb(200, 200, 200));
            currSongId = ((Song)item.Content).id;

        }
        
        public static void Stop()
        {
            MW.playbackstopped = true;
            MW.playerPlayButtonImage.Source = new BitmapImage(new Uri(@"Resources/appbar.control.play.png", UriKind.Relative));
            outputDevice.Stop();
            outputDevice.Dispose();
            outputDevice = null;
            if(audioFile != null)
                audioFile.Dispose();
            audioFile = null;
            try { updatetmr.Stop(); }
            catch (NullReferenceException) { }
            MW.songProgressLabel.Content = "00:00";
            MW.songLengthLabel.Content = "00:00";
            MW.songProgressSlider.Value = 0;
            MW.songNameLabel.Content = "Song - Title";
            MW.playerCoverImage.Source = new BitmapImage(new Uri(@"Resources/cd_icon.jpg", UriKind.Relative));
        }
        public static void Pause()
        {
            outputDevice.Stop();
            MW.playbackstopped = true;
            MW.playerPlayButtonImage.Source = new BitmapImage(new Uri("Resources/appbar.control.play.png", UriKind.Relative));
        }
        public static void Resume()
        {
            outputDevice.Play();
            MW.playbackstopped = false;
            MW.playerPlayButtonImage.Source = new BitmapImage(new Uri("Resources/appbar.control.pause.png", UriKind.Relative));
        }
        public static void Next()
        {
            if (shuffle == true)
            {
                Random random = new Random();
                int randomindex;
                do
                    randomindex = random.Next(0, MW.listview_playlist.Items.Count);
                while (randomindex == MW.currlistitemindex);
                Song song = (Song)(MW.listview_playlist.ItemContainerGenerator.ContainerFromIndex(randomindex) as ListViewItem).Content;
                MW.currlistitemindex = randomindex;
                Stop();
                Play(song);
            }
            else
            {
                try
                {
                    MW.currlistitemindex += 1;
                    int numOfItems = MW.listview_playlist.Items.Count;

                    if (MW.currlistitemindex == numOfItems)
                        MW.currlistitemindex = 0;
                    Song song = (Song)(MW.listview_playlist.ItemContainerGenerator.ContainerFromIndex(MW.currlistitemindex) as ListViewItem).Content;
                    Stop();
                    Play(song);
                }
                catch (NullReferenceException) { }
            }
        }
        public static void Previous()
        {
            try
            {
                MW.currlistitemindex -= 1;
                int numOfItems = MW.listview_playlist.Items.Count;

                if (MW.currlistitemindex < 0)
                    MW.currlistitemindex = numOfItems - 1;
                Song song = (Song)(MW.listview_playlist.ItemContainerGenerator.ContainerFromIndex(MW.currlistitemindex) as ListViewItem).Content;
                Stop();
                Play(song);
            }
            catch (NullReferenceException) { }
        }
        private static bool shuffle = false;
        public static void ToggleShuffle()
        {
            if (shuffle == false)
            {
                shuffle = true;
                MW.playerShuffleButtonImage.Source = new BitmapImage(new Uri("Resources/appbar.shuffle.green.png", UriKind.Relative));
            }
            else
            {
                shuffle = false;
                MW.playerShuffleButtonImage.Source = new BitmapImage(new Uri("Resources/appbar.shuffle.png", UriKind.Relative));
            }
        }
        private static bool repeatplaylist = false;
        public static void ToggleRepeat()
        {
            if (repeatplaylist == false)
            {
                repeatplaylist = true;
                MW.playerRepeatButtonImage.Source = new BitmapImage(new Uri("Resources/appbar.repeat.green.png", UriKind.Relative));
            }
            else
            {
                repeatplaylist = false;
                MW.playerRepeatButtonImage.Source = new BitmapImage(new Uri("Resources/appbar.repeat.png", UriKind.Relative));
            }
        }
        public static void SetPlaybackPosition(double value)
        {
            audioFile.CurrentTime = TimeSpan.FromSeconds(value);
        }
        public static void SongEnded()
        {
            Pause();
            if (MW.listview_playlist.Items.Count > 1)
            {
                if ((MW.currlistitemindex + 1) == MW.listview_playlist.Items.Count)
                {
                    if (repeatplaylist == true || shuffle == true)
                        Next();
                }
                else
                    Next();
            }
        }

        public static System.Timers.Timer updatetmr;
        public static void SongProgressUpdater()
        {
            double songdur = Player.audioFile.TotalTime.TotalSeconds;
            if (songdur != 0)
            {
                updatetmr = new System.Timers.Timer();
                updatetmr.Interval = 1000;
                updatetmr.Enabled = true;
                updatetmr.Elapsed += (sender, EventArgs) =>
                {
                    if (MW.playbackstopped == false)
                    {
                        TimeSpan songpassedtimespan = Player.audioFile.CurrentTime;
                        double songpassed = songpassedtimespan.TotalSeconds;
                        double progress = (songpassed * 10) / songdur;
                        try
                        {
                            MW.Dispatcher.Invoke(() =>
                            {
                                MW.songProgressSlider.Value = progress;
                                if (songpassedtimespan.Hours >= 1)
                                    MW.songProgressLabel.Content = songpassedtimespan.ToString(@"hh\:mm\:ss");
                                else
                                    MW.songProgressLabel.Content = songpassedtimespan.ToString(@"mm\:ss");
                                if (MW.songProgressSlider.Value == 10)
                                {
                                    SongEnded();
                                }
                            });
                        }
                        catch (TaskCanceledException) { }
                    }

                };
            }
            else
            {
                System.Timers.Timer tmr = new System.Timers.Timer();
                tmr.Interval = 1000;
                tmr.Enabled = true;
                tmr.Elapsed += (sender, EventArgs) =>
                {
                    SongProgressUpdater();
                    tmr.Stop();
                };
            }
        }
    }
}