using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Conrad_RPi
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        #region GPIO Test
        GpioPin pin;
        GpioController gpio;
        private readonly int LEDPIN = 4;

        private void DayAction01(object sender, RoutedEventArgs e)
        {
            pin.Write(LEDSwitch.IsOn ? GpioPinValue.High : GpioPinValue.Low);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // load default GpioController
            gpio = GpioController.GetDefault();

            //set the connection to the gpio pin
            pin = gpio.OpenPin(LEDPIN);

            //set Value for the Pin to High = LED off
            pin.Write(GpioPinValue.High);

            //config the pin for output
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }
        #endregion
    }
}
