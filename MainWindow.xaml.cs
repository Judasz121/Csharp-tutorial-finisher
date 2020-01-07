using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using NAudio.Wave;
using NAudio;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using TagLib;

namespace CSharp_firstApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Player.Setup(this);
            BackgroundWork.Setup(this);

        }
        public int currlistitemindex = 0;
        public bool playbackstopped = true;
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        System.Timers.Timer dtmr = new System.Timers.Timer();
        public void DebugTimer(int interval, string message)
        {
            dtmr.Interval = interval;
            dtmr.Enabled = true;
            dtmr.Elapsed += (sender, EventArgs) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    songNameLabel.Content = message;
                });
            };
           
        }
        public void DebugTimer(int interval)
        {
            string watchaWantBoii;
            dtmr.Interval = interval;
            dtmr.Enabled = true;
            dtmr.Elapsed += (sender, EventArgs) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (Player.updatetmr.Enabled)
                            watchaWantBoii = " timer enabled";
                        else
                            watchaWantBoii = (string)" timer disabled";

                        songNameLabel.Content = watchaWantBoii.ToString();
                    }
                    catch (NullReferenceException) { watchaWantBoii = "there no such timer boi"; }
                });
            };

        }



        public void Listview_Playlist_ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                listview_playlist.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            listview_playlist.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));

            System.Timers.Timer tmr = new System.Timers.Timer();
            tmr.Interval = 1;
            tmr.Enabled = true;
            tmr.Elapsed += (sender1, EventArgs) =>
            {
                if (listview_playlist.Items.Count != 0 && Player.audioFile != null)
                {
                    int numOfIndexes = listview_playlist.Items.Count - 1;
                    bool done = false;
                    int i = 0;
                    while (done == false)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                         //   listview_playlist.UpdateLayout();
                            var item = listview_playlist.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                            int id = ((Song)item.Content).id;
                            if (id == Player.currSongId)
                            {
                                item.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                                Player.previtem = item;
                                done = true;
                            }
                            i++;
                        });
                    }
                }
                tmr.Stop();
            };
        }

        public ObservableCollection<Song> playlist = new ObservableCollection<Song>();
        public void Listview_playlist_DragDrop(object sender, DragEventArgs e)
        {
            BackgroundWork.PlaylistNewSongsHandler(sender, e);
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                    BackgroundWork.bgw1.CancelAsync();
                while (listview_playlist.SelectedItems.Count > 0)
                {
                    Song song = listview_playlist.SelectedItems[0] as Song;
                    int index = listview_playlist.ItemContainerGenerator.IndexFromContainer(listview_playlist.ItemContainerGenerator.ContainerFromItem(song));
                    if (index == currlistitemindex)
                        Player.Stop();

                    if (song != null)
                        playlist.Remove(song);
                }
             /*   System.Timers.Timer tmr = new System.Timers.Timer();
                tmr.Interval = 1000;
                tmr.Enabled = true;
                tmr.Elapsed += (e1, EventArgs1) =>
                {*/
                    BackgroundWork.CalcListviewSongsDurAndSize(listview_playlist);
              //      tmr.Enabled = false;
             //   };
            }
        }


        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            currlistitemindex = listview_playlist.ItemContainerGenerator.IndexFromContainer(item);
            //    Song song = item.DataContext as Song;\
            Song song = (Song)item.Content;
            if (Player.audioFile != null)
            {
                Player.Stop();
            }
            Player.Play(song);
        }

        private void PlayerPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if(playbackstopped == false)
            {
                Player.Pause();
            }
            else
            {
                if (Player.audioFile == null)
                {
                    try
                    {
                        currlistitemindex = listview_playlist.SelectedIndex;
                        var item = listview_playlist.ItemContainerGenerator.ContainerFromIndex(listview_playlist.SelectedIndex) as ListViewItem;
                        Song song = (Song)item.Content;
                        Player.Play(song);
                    }
                    catch (IndexOutOfRangeException) { }
                    catch (NullReferenceException) { }
                }
                else
                    Player.Resume();
            }
        }
        private void PlayerNextButton_Click(object sender, RoutedEventArgs e){ Player.Next(); }
        private void PlayerPreviousButton_Click(object sender, RoutedEventArgs e){ Player.Previous(); }
        private void PlayerShuffleButton_Click(object sender, RoutedEventArgs e){ Player.ToggleShuffle(); }
        private void PlayerRepeatButton_Click(object sender, RoutedEventArgs e){ Player.ToggleRepeat(); }

        System.Timers.Timer voltmr;
        private void VolumeSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Player.outputDevice != null)
            {
                voltmr = new System.Timers.Timer();
                voltmr.Interval = 50;
                voltmr.Start();
                voltmr.Elapsed += (sender1, EventArgs) =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        float vol = (float)volumeSlider.Value / 10;
                        Player.outputDevice.Volume = vol;
                    });
                };
            }
        }
        private void VolumeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.audioFile != null)
            {
                voltmr.Stop();
                float vol = (float)volumeSlider.Value / 10;
                Player.outputDevice.Volume = vol;
            }
        }
        System.Timers.Timer mouseDtmr;
        private void SongProgressSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Player.audioFile != null)
            {
                mouseDtmr = new System.Timers.Timer();
                mouseDtmr.Interval = 50;
                mouseDtmr.Enabled = true;
                mouseDtmr.Elapsed += (sender1, EventArgs) =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        try { Player.updatetmr.Enabled = false; }
                        catch (NullReferenceException) { }
                        double songdur = Player.audioFile.TotalTime.TotalSeconds;
                        double progress = songProgressSlider.Value;
                        double songpassed = (progress * songdur) / 10;
                        TimeSpan songpassedtimespan = TimeSpan.FromSeconds(songpassed);

                        if (songpassedtimespan.Hours >= 1)
                            songProgressLabel.Content = TimeSpan.FromSeconds(songpassed).ToString(@"hh\:mm\:ss");
                        else
                            songProgressLabel.Content = TimeSpan.FromSeconds(songpassed).ToString(@"mm\:ss");
                    });
                };
            }
        }

        private void SongProgressSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.audioFile != null)
            {
                try{ mouseDtmr.Stop(); }
                catch (NullReferenceException) { }
                double songdur = Player.audioFile.TotalTime.TotalSeconds;
                double progress = songProgressSlider.Value;

                double songpassed = (progress * songdur) / 10;
                Player.SetPlaybackPosition(songpassed);
                Player.updatetmr.Enabled = true;
            }
        }
       
    }
}

