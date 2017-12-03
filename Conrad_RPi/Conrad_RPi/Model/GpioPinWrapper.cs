using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace Conrad_RPi.Model
{
    class GpioPinWrapper :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        GpioPin _Pin;
        public GpioPin Pin
        {
            get
            {
                return _Pin;
            }
            set
            {
                if (_Pin != value)
                {
                    _Pin = value;
                    NotifyPropertyChanged();
                    _Pin.ValueChanged += _Pin_ValueChanged;
                }
            }
        }

        //
        // Zusammenfassung:
        //     Tritt auf, wenn der Wert des allgemeinen E/A (GPIO)-Pins geändert wird, entweder
        //     aufgrund eines externen Auslöseimpulses, wenn der Pin als Eingabe konfiguriert
        //     ist, oder wenn ein Wert auf den Pin geschrieben wird, wenn der Pin als Ausgabe
        //     konfiguriert ist.
        public event TypedEventHandler<GpioPin, GpioPinValueChangedEventArgs> ValueChanged;
        private void _Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            ValueChanged?.Invoke(sender, args);
        }

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

        public GpioPinDriveMode DriveMode
        {
            get
            {
                return Pin.GetDriveMode();
            }
            set
            {
                Pin.SetDriveMode(value);
                NotifyPropertyChanged();
            }
        }

        public GpioSharingMode SharingMode => Pin.SharingMode;
        public int PinNumber => Pin.PinNumber;
        public TimeSpan DebounceTimeout => Pin.DebounceTimeout;
        public void Dispose() => Pin.Dispose();
        public bool IsDriveModeSupported(GpioPinDriveMode m) => Pin.IsDriveModeSupported(m);
    }
}
