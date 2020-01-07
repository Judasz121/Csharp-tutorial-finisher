using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_firstApp
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyname)
        {
            try
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyname));
            }
            catch (NullReferenceException) { }
        }
    }
}