using Conrad_RPi.Model;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Conrad_RPi
{
    public sealed partial class MainPage : Page
    {
        GpioController gpio;
        GpioPinWrapper PinLED1 = new GpioPinWrapper();
        GpioPinWrapper PinLED2 = new GpioPinWrapper();
        public MainPage()
        {
            InitializeComponent();
            ConfigureGPIO();
        }
        void ConfigureGPIO()
        {
            gpio = GpioController.GetDefault();
            PinLED1.Pin = gpio?.OpenPin(4);
            PinLED2.Pin = gpio?.OpenPin(17);
            PinLED1.Status = GpioPinValue.Low;
            PinLED2.Status = GpioPinValue.Low;
            PinLED1.Pin?.SetDriveMode(GpioPinDriveMode.Output);
            PinLED2.Pin?.SetDriveMode(GpioPinDriveMode.Output);
        }

        void DayAction01(object sender, RoutedEventArgs e)
        {
            //PinLED1.Pin.Write((sender as ToggleSwitch).IsOn ? GpioPinValue.High : GpioPinValue.Low);
        }
        CancellationTokenSource source;
        async void DayAction02(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleSwitch).IsOn)
            {
                source = new CancellationTokenSource();
                await Task.Run(() => Blink(PinLED1, 1, source.Token), source.Token);
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                await Task.Run(() => Blink(PinLED2, 1, source.Token), source.Token);
            }
            else
            {
                source.Cancel();
            }
        }

        static async void Blink(GpioPinWrapper Pin, int WaitTime, CancellationToken cancellationToken)
        {
            bool CurrentValue = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                Pin.Status = CurrentValue ? GpioPinValue.High : GpioPinValue.Low;
                CurrentValue = !CurrentValue;
                await Task.Delay(TimeSpan.FromSeconds(WaitTime));
            }
        }
    }
    public class io_GpioPinValue : IValueConverter
    {
        #region IValueConverter Members 
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((GpioPinValue)value) == GpioPinValue.High ? true : false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? GpioPinValue.High : GpioPinValue.Low;
        }
        #endregion
    }
}
