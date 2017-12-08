using Conrad_RPi.Model;
using Conrad_RPi.Server;
using System;
using System.Threading;
using System.Threading.Tasks;
using TLIB.Model;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace Conrad_RPi
{
    public sealed partial class MainPage : Page
    {
        GpioController gpio;
        GpioPinWrapper PinLED1 = new GpioPinWrapper();
        GpioPinWrapper PinLED2 = new GpioPinWrapper();
        GpioPinWrapper PinTaster1 = new GpioPinWrapper();
        public MainPage()
        {
            InitializeComponent();
            ConfigureGPIO();
            server = new LEDHTTPServer();
            server.NewQueryAppeared += Server_NewQueryAppeared;
            server.Initialise();
        }

        private void Server_NewQueryAppeared(object sender, string e)
        {
            if (e == null || e == "")
            {
                return;
            }
            string message = e.ToLower();
            GpioPinValue newValue = GpioPinValue.High;
            GpioPinWrapper LED = null;
            if (message.Contains("on"))
            {
                newValue = GpioPinValue.High;
            }
            else if (message.Contains("off"))
            {
                newValue = GpioPinValue.Low;
            }
            if (message.Contains("red"))
            {
                LED = PinLED1;
            }
            else if (message.Contains("green"))
            {
                LED = PinLED2;
            }
            else if (message.Contains("white"))
            {
                return;
            }
            else if (message.Contains("white"))
            {
                return;
            }
            LED.Status = newValue;
        }

        LEDHTTPServer server;
    protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SharedAppModel.Instance.SetDependencies(Dispatcher);
            base.OnNavigatedTo(e);
        }
        void ConfigureGPIO()
        {
            gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                return;
            }
            PinLED1.Pin = gpio.OpenPin(4, GpioSharingMode.Exclusive);
            PinLED2.Pin = gpio.OpenPin(17, GpioSharingMode.Exclusive);
            PinTaster1.Pin = gpio.OpenPin(7, GpioSharingMode.Exclusive);
            PinLED1.Status = GpioPinValue.Low;
            PinLED2.Status = GpioPinValue.Low;
            PinLED1.Pin.SetDriveMode(GpioPinDriveMode.Output);
            PinLED2.Pin.SetDriveMode(GpioPinDriveMode.Output);
            PinTaster1.Pin.SetDriveMode(GpioPinDriveMode.InputPullDown);
            PinTaster1.Pin.ValueChanged += DayAction03;
        }

        private void DayAction03(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine(args.Edge);
            PinLED2.Status = args.Edge == GpioPinEdge.FallingEdge ? GpioPinValue.Low : GpioPinValue.High;
        }

        void DayAction01(object sender, RoutedEventArgs e)
        {
            PinLED1.Status = (sender as ToggleSwitch).IsOn ? GpioPinValue.High : GpioPinValue.Low;
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
