using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RocketbankTestApp.ViewModels
{
    public class BaseVM: INotifyPropertyChanged
    {
        private bool busy = false;

        public bool Busy
        {
            get 
            {
                return busy;
            }
            set 
            {
                busy = value;
                risePropertyChanged();
            }
        }

        CancellationTokenSource cts;

        public void Cancel()
        {
            try
            {
                if (cts!=null)
                {
                    cts.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {

            }
        }

        protected void risePropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler!=null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
