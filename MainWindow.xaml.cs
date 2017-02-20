using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Enumeration;

namespace Draconyx
{
    public partial class MainWindow : Window
    {
        DeviceWatcher _watcher;

        public ObservableCollection<LiveBluetoothDevice> Devices { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Devices = new ObservableCollection<LiveBluetoothDevice>();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _watcher = DeviceInformation.CreateWatcher(

                // Bluetooth LE support cut due to API instability
                //@"(System.Devices.Aep.ProtocolId:=""{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}""
                //    OR System.Devices.Aep.ProtocolId:=""{bb7bb05e-5972-42b5-94fc-76eaa7084d49}"")
                //    AND System.Devices.Aep.IsPaired:=true",

                @"System.Devices.Aep.ProtocolId:=""{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}""
                    AND System.Devices.Aep.IsPaired:=true",
                new string[] {
                    "System.Devices.Aep.ProtocolId",
                    "System.Devices.Aep.DeviceAddress",
                    "System.Devices.Aep.SignalStrength"
                },
                DeviceInformationKind.AssociationEndpoint);

            _watcher.Added += DeviceAdded;
            _watcher.Removed += DeviceRemoved;
            _watcher.Updated += DeviceUpdated;
            _watcher.Stopped += WatcherStopped;
            _watcher.Start();
        }

        private void WatcherStopped(DeviceWatcher sender, object args)
        {
            Dispatcher.Invoke(() =>
            {
                Devices.Clear();
            });

            _watcher.Start();
        }

        private void DeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            var device = Devices.Where(d => d.Id == args.Id).FirstOrDefault();
            if (device != null)
            {
                device.Update(args);
            }
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            var device = Devices.Where(d => d.Id == args.Id).FirstOrDefault();
            if (device != null)
            {
                Dispatcher.Invoke(() =>
                {
                    Devices.Remove(device);
                });
            }
        }

        private void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            Dispatcher.Invoke(() =>
            {
                Devices.Add(new LiveBluetoothDevice(args));
            });
        }
    }
}
