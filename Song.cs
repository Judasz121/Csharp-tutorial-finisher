using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagLib;

namespace CSharp_firstApp
{
    public class Song : ObservableObject
    {

        public readonly int id;
        private static int prevId = -1;
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }

        private TimeSpan duration;
        public TimeSpan Duration
        {
            get { return duration; }
            set {
                duration = value;
                OnPropertyChanged("DurationString");
            }
        }
        public string DurationString
        {
            get
            {
                if (duration.Hours >= 1)
                    return duration.ToString(@"hh\:mm\:ss");
                else
                    return duration.ToString(@"mm\:ss"); 
            }
            set {  }
        }
        public string Genre { get; set; }
        public string Album { get; set; }
        public IPicture Cover { get; set; }
        public long Size { get; set; }
        private string sizestring;
        public string SizeString {
            get
            {
                if (sizestring != null)
                    return sizestring;
                else
                    return "0B";
            }
            set
            {
                sizestring = value;
                OnPropertyChanged("SizeString");
            }
        }
   

        public Song()
        {
            id = prevId + 1;
            prevId = id;
        }
        public Song(string P, string FN, string T, TimeSpan D, string G, string A, IPicture C)
        {
            Path = P;
            FileName = FN;
            Title = T;
            Duration = D;
            Genre = G;
            Album = A;
            Cover = C;

            id = prevId + 1;
            prevId = id;
        }

        
    }
}
