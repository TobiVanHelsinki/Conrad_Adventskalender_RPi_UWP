using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Conrad_RPi
{
    public sealed partial class MainPage : Page
    {
        GpioController gpio;
        GpioPin PinLED1;
        GpioPin PinLED2;
        public MainPage()
        {
            InitializeComponent();
            ConfigureGPIO();
        }
        void ConfigureGPIO()
        {
            gpio = GpioController.GetDefault();
            PinLED1 = gpio.OpenPin(4);
            PinLED2 = gpio.OpenPin(17);
            PinLED1.Write(GpioPinValue.High);
            PinLED2.Write(GpioPinValue.High);
            PinLED1.SetDriveMode(GpioPinDriveMode.Output);
            PinLED2.SetDriveMode(GpioPinDriveMode.Output);
        }

        void DayAction01(object sender, RoutedEventArgs e)
        {
            PinLED1.Write((sender as ToggleSwitch).IsOn ? GpioPinValue.High : GpioPinValue.Low);
        }

        void DayAction02(object sender, RoutedEventArgs e)
        {
            PinLED2.Write((sender as ToggleSwitch).IsOn ? GpioPinValue.High : GpioPinValue.Low);
        }
    }
}
