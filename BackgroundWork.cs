using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CSharp_firstApp
{
     static class BackgroundWork
     {
        private static MainWindow MW;
        public static void Setup(MainWindow main) { MW = main; }

     //   public static List<Song> playlist = new List<Song>();
        public static string[] filesPaths;
        public static async Task PlaylistNewSongsHandler(object sender, DragEventArgs e)
        {
            List<Task<Song>> songtasks = new List<Task<Song>>();

            filesPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string path in filesPaths)
            {
                songtasks.Add(SongDataHandlerAsync(path));
            }
            var results = await Task.WhenAll(songtasks);
            foreach( var item in results)
            {
                MW.playlist.Add(item);
            }
            MW.listview_playlist.ItemsSource = MW.playlist;
            MW.listview_playlist.Items.SortDescriptions.Clear();
            MW.listview_playlist.Items.Refresh();





            CalcListviewSongsDurAndSize(MW.listview_playlist);
        }
        private static async Task<Song> SongDataHandlerAsync(string path)
        {
            FileInfo fi = new FileInfo(path);
            TagLib.File tagFile = TagLib.File.Create(path);
            Song song = new Song();

            song.Title = tagFile.Tag.Title;
            song.Path = path;
            song.FileName = fi.Name;
            // song.SizeString = "something" <- interestingly, this code takes a lot of time to execute
            song.Size = fi.Length;
            try
            {
                song.Cover = tagFile.Tag.Pictures[0];
            }
            catch (System.IndexOutOfRangeException)
            {
                song.Cover = null;
            }
            return song;
        }










        private static ListView listviewpublic { get; set; }
        private static readonly string[] suffixes ={ "B", "KB", "MB", "GB", "TB", "PB" };
        public static BackgroundWorker bgw1 = new BackgroundWorker();
        public static bool bgw1_paused = false;
        public static void CalcListviewSongsDurAndSize(ListView listview)
        {
            
            bgw1.WorkerReportsProgress = true;
            bgw1.DoWork += bgw1_DoWork;
            bgw1.ProgressChanged += bgw1_ProgressChanged;
            bgw1.RunWorkerCompleted += bgw1_RunWorkerCompleted;
            bgw1.WorkerSupportsCancellation = true;
            listviewpublic = listview;
           try
           {
                bgw1.RunWorkerAsync(listview);
           }
            catch (System.Runtime.InteropServices.COMException){ }
            catch (System.InvalidOperationException) { }

        }
        private static void bgw1_DoWork(object sender, DoWorkEventArgs e)
        {
            ListView listview = (ListView)e.Argument;
            foreach (Song song in listview.ItemsSource)
            {
                if (song.Duration.Milliseconds == 0)
                {
                    song.Duration = new AudioFileReader(song.Path).TotalTime;

                    long bytes = song.Size;
                    int counter = 0;
                    decimal number = (decimal)bytes;
                    while (Math.Round(number / 1024) >= 1)
                    {
                        number = number / 1024;
                        counter++;
                    }
                    song.SizeString = string.Format("{0:n1}{1}", number, suffixes[counter]);
                }
                if (bgw1.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                while (bgw1_paused)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
        private static void bgw1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
        private static void bgw1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
    }
}
