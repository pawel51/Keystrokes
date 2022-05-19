using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected int __MOD = 3; // in release can be lower (performance of plots)

        public int MOD
        {
            get { return __MOD; }
            set { __MOD = value; OnPropertyChanged(nameof(MOD)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
