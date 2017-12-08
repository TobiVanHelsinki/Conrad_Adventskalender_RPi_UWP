using Conrad_RPi.Model;
using Conrad_RPi.Server;
using System;
using System.Linq;
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
        LEDHTTPServer server;
        GpioController gpio;
        GpioPinWrapper PinLED1 = new GpioPinWrapper();
        GpioPinWrapper PinLED2 = new GpioPinWrapper();
        GpioPinWrapper PinLED3 = new GpioPinWrapper();
        GpioPinWrapper PinTaster1 = new GpioPinWrapper();
        public MainPage()
        {
            InitializeComponent();
            ConfigureGPIO();
            server = new LEDHTTPServer();
            server.NewQueryAppeared += Server_NewQueryAppeared;
            server.Initialise();
        }
        void Server_NewQueryAppeared(object sender, string e)
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
            PinLED3.Pin = gpio.OpenPin(24, GpioSharingMode.Exclusive);
            PinTaster1.Pin = gpio.OpenPin(7, GpioSharingMode.Exclusive);
            PinLED1.Status = GpioPinValue.Low;
            PinLED2.Status = GpioPinValue.Low;
            PinLED3.Status = GpioPinValue.Low;
            PinLED1.Pin.SetDriveMode(GpioPinDriveMode.Output);
            PinLED2.Pin.SetDriveMode(GpioPinDriveMode.Output);
            PinLED3.Pin.SetDriveMode(GpioPinDriveMode.Output);
            PinTaster1.Pin.SetDriveMode(GpioPinDriveMode.InputPullDown);
            PinTaster1.Pin.ValueChanged += (x,y)=>Entprellen(Treshhold, DayAction04_Pin,x, y);
            PinTaster1.Pin.ValueChanged += (x,y)=>Entprellen(Treshhold, DayAction05_Pin,x, y);
            //PinTaster1.Pin.ValueChanged += DayAction04_Pin;
            //PinTaster1.Pin.ValueChanged += DayAction05_Pin;
        }
        DateTime lastActionTime = new DateTime(0);
        static long Treshhold = 70 * 1000 * 10;
        private void Entprellen(long treshhold, Action<GpioPin, GpioPinValueChangedEventArgs> action, GpioPin x, GpioPinValueChangedEventArgs y)
        {
            //System.Diagnostics.Debug.WriteLine("Treshhold  " + treshhold);
            //System.Diagnostics.Debug.WriteLine("comparsion " + Math.Abs(DateTime.Now.Ticks - lastActionTime.Ticks));
            if (Math.Abs(DateTime.Now.Ticks - lastActionTime.Ticks) < treshhold)
            {
                return;
            }
            lastActionTime = DateTime.Now;
            //System.Diagnostics.Debug.WriteLine("Action ");
            action(x, y);
        }
        #region day actions
        void DayAction01(object sender, RoutedEventArgs e)
        {
            PinLED1.Status = (sender as ToggleSwitch).IsOn ? GpioPinValue.High : GpioPinValue.Low;
        }
        CancellationTokenSource CancelTokenSourceDay02;
        async void DayAction02(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleSwitch).IsOn)
            {
                CancelTokenSourceDay02 = new CancellationTokenSource();
                await Task.Run(() => Blink(PinLED1, 1, CancelTokenSourceDay02.Token), CancelTokenSourceDay02.Token);
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                await Task.Run(() => Blink(PinLED2, 1, CancelTokenSourceDay02.Token), CancelTokenSourceDay02.Token);
            }
            else
            {
                CancelTokenSourceDay02.Cancel();
            }
        }
        bool AllowDayAction_04 = false;
        void DayAction04(object sender, RoutedEventArgs e)
        {
            AllowDayAction_04 = (sender as ToggleSwitch).IsOn;
        }
        void DayAction04_Pin(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (AllowDayAction_04)
            {
                PinLED2.Status = args.Edge == GpioPinEdge.FallingEdge ? GpioPinValue.Low : GpioPinValue.High;
            }
        }
        bool AllowDayAction_05 = false;
        void DayAction05(object sender, RoutedEventArgs e)
        {
            AllowDayAction_05 = (sender as ToggleSwitch).IsOn;
        }
        void DayAction05_Pin(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (AllowDayAction_05)
            {
                if (GpioPinEdge.RisingEdge == args.Edge)
                {
                    PinLED1.Status = PinLED1.Status == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;
                    PinLED2.Status = PinLED1.Status == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;
                }
            }
        }
        CancellationTokenSource CancelTokenSourceDay08;
        async void DayAction08(object sender, RoutedEventArgs e)
        {
            TextBox box = ((sender as ToggleSwitch).Parent as StackPanel).Children.Where(x => (x as FrameworkElement).Name == "Day08Input").First() as TextBox;
            UInt16 pwm_time = 0;
            if (box == null || !UInt16.TryParse(box.Text, out pwm_time))
            {
                pwm_time = 5;
            }
            if (pwm_time > 10)
            {
                pwm_time = 5;
            }

            if ((sender as ToggleSwitch).IsOn)
            {
                CancelTokenSourceDay08 = new CancellationTokenSource();
                await Task.Run(() => PWM(PinLED3, pwm_time, CancelTokenSourceDay08.Token), CancelTokenSourceDay08.Token);
            }
            else
            {
                CancelTokenSourceDay08.Cancel();
            }
        }
        #endregion
        //CancellationTokenSource source;
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
        static async void PWM(GpioPinWrapper Pin, uint time, CancellationToken cancellationToken)
        {
            Pin.Status = GpioPinValue.High;
            int pwm_time_high = (int)time;
            int pwm_time_low = (int)(10 - time);
            while (!cancellationToken.IsCancellationRequested)
            {
                Pin.Pin.Write(GpioPinValue.High);
                await Task.Delay(pwm_time_high);
                Pin.Pin.Write(GpioPinValue.Low);
                await Task.Delay(pwm_time_low);
            }
            Pin.Status = GpioPinValue.Low;
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
