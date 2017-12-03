using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Conrad_RPi.Model
{
    class GpioPinWrapper :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public GpioPin Pin;
        public GpioPinValue Status
        {
            get
            {
                return Pin.Read();
            }
            set
            {
                Pin.Write(value);
                NotifyPropertyChanged();
            }
        }
        public bool BoolStatus
        {
            get
            {
                return Pin.Read() == GpioPinValue.High ? true : false;
            }
            set
            {
                Pin.Write(value == true ? GpioPinValue.High : GpioPinValue.Low);
                NotifyPropertyChanged();
            }
        }
        

    }
}
