using System;
using System.ComponentModel;
using System.Timers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace Draconyx
{
    public class LiveBluetoothDevice : INotifyPropertyChanged
    {
        DeviceInformation _deviceInformation;
        public event PropertyChangedEventHandler PropertyChanged;
        public Timer _signalStrengthTimer;

        public LiveBluetoothDevice(DeviceInformation deviceInformation)
        {
            _deviceInformation = deviceInformation;
            _signalStrengthTimer = new Timer(TimeSpan.FromSeconds(3).TotalMilliseconds);
            _signalStrengthTimer.Elapsed += TimerElapsed;
            _signalStrengthTimer.AutoReset = false;
            _signalStrengthTimer.Start();
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var protocolId = _deviceInformation.Properties["System.Devices.Aep.ProtocolId"] as Guid?;
            if (protocolId.HasValue)
            {
                if (protocolId.Value == Guid.Parse("{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}"))
                {
                    using (var device = await BluetoothDevice.FromIdAsync(_deviceInformation.Id))
                    {
                        await device.GetRfcommServicesAsync(BluetoothCacheMode.Uncached);
                    }
                }
                
                // API is unreliable/broken, cutting Bluetooth LE support

                //else
                //{
                //    using (var device = await BluetoothLEDevice.FromIdAsync(_deviceInformation.Id))
                //    {
                //        // Do something to force a connection
                //    }
                //}

                OnPropertyChanged("SignalStrength");
                OnPropertyChanged("IsPresent");
                OnPropertyChanged("LastUpdated");
            }

            _signalStrengthTimer.Start();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Update(DeviceInformationUpdate deviceInformationUpdate)
        {
            _deviceInformation.Update(deviceInformationUpdate);

            OnPropertyChanged("Name");
            OnPropertyChanged("Id");
            OnPropertyChanged("SignalStrength");
            OnPropertyChanged("LastUpdated");
            OnPropertyChanged("IsPresent");
        }

        public string Name
        {
            get
            {
                return string.IsNullOrWhiteSpace(_deviceInformation.Name) ? "[null]" : _deviceInformation.Name;
            }
        }

        public string Id
        {
            get
            {
                return _deviceInformation.Id;
            }
        }

        public int? SignalStrength
        {
            get
            {
                return (int?)_deviceInformation.Properties["System.Devices.Aep.SignalStrength"];
            }
        }

        public DateTime LastUpdated
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}
