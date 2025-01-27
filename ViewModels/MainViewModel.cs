using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Threading;
using System.Windows.Input;
using LibreHardwareMonitor.Hardware;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Printing.IndexedProperties;
using System.Windows;
using ResourceMonitor.Models;


namespace ResourceMonitor.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private string _currentTime;
        private string _currentDate;

        private readonly Timer _timer;
        private Computer _computer;

        private float _cpuUsage;

        private float _ramUsage;
        private float _usedRam;
        private float _totalRam;

        public float RamUsage
        {
            get => _ramUsage;
            set
            {
                _ramUsage = value;
                OnPropertyChanged();
            }
        }

        public float UsedRam
        {
            get => _usedRam;
            set
            {
                _usedRam = value;
                OnPropertyChanged();
            }
        }

        public float TotalRam
        {
            get => _totalRam;
            set
            {
                _totalRam = value;
                OnPropertyChanged();
            }
        }

        private float _gpuUsage;
        private float _gpuTemperature;
        private float _gpuMemoryUsed;
        private float _gpuMemoryTotal;
        private string _gpuName;
        public float GpuUsage
        {
            get => _gpuUsage;
            set
            {
                _gpuUsage = value;
                OnPropertyChanged();
            }
        }

        public float GpuTemperature
        {
            get => _gpuTemperature;
            set
            {
                _gpuTemperature = value;
                OnPropertyChanged();
            }
        }

        public float GpuMemoryUsed
        {
            get => _gpuMemoryUsed;
            set
            {
                _gpuMemoryUsed = value;
                OnPropertyChanged();
            }
        }

        public float GpuMemoryTotal
        {
            get => _gpuMemoryTotal;
            set
            {
                _gpuMemoryTotal = value;
                OnPropertyChanged();
            }
        }

        public string GpuName
        {
            get => _gpuName;
            set
            {
                _gpuName = value;
                OnPropertyChanged();
            }
        }



        private ObservableCollection<CpuCoreModel> _cpuCores;
        private ObservableCollection<DriveInfo> _drives;
        

        private bool _isHardwareInitialized = false;


        private bool _isAlwaysOnTop = true;  // defaults to true

        public bool IsAlwaysOnTop
        {
            get => _isAlwaysOnTop;
            set
            {
                _isAlwaysOnTop = value;
                OnPropertyChanged();
                // Raise an event that the window needs to update
                AlwaysOnTopChanged?.Invoke(this, value);
            }
        }


        private bool IsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged();
            }
        }

        public float CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<CpuCoreModel> CpuCores
        {
            get => _cpuCores;
            set
            {
                _cpuCores = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DriveInfo> Drives
        {
            get => _drives;
            set
            {
                _drives = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _cpuCores = new ObservableCollection<CpuCoreModel>();
            InitializeHardwareMonitoring();
            _timer = new Timer(UpdateStats, null, 0, 1000); // Update every second
        }

        private void InitializeHardwareMonitoring()
        {
            try
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsNetworkEnabled = true
                };

                if (!IsAdministrator())
                {
                    Debug.WriteLine("Running without admin rights - some features may be limited");
                }

                _computer.Open();
                _isHardwareInitialized = true;
                Debug.WriteLine("Hardware monitoring started successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Hardware monitoring initialization error: {ex.Message}");
                MessageBox.Show($"Some features may be limited. Error: {ex.Message}\n\nTry running as administrator for full functionality.",
                    "Limited Functionality",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void UpdateStats(object state)
        {
            try
            {
                if (!_isHardwareInitialized) return;

                UpdateTime(null);
                UpdateCpuStats();
                UpdateRamStats();
                UpdateGpuStats();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Stats update error: {ex.Message}");
            }
        }

        private void UpdateCpuStats()
        {
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    float totalCpuLoad = 0;
                    var coreLoads = new Dictionary<string, float>();

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load)
                        {
                            if (sensor.Name.Contains("CPU Total"))
                            {
                                totalCpuLoad = sensor.Value ?? 0;
                            }
                            else if (sensor.Name.Contains("CPU Core"))
                            {
                                coreLoads[sensor.Name] = sensor.Value ?? 0;
                            }
                        }
                    }

                    // Update on UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CpuUsage = totalCpuLoad;

                        // Initialize cores if needed
                        if (CpuCores.Count == 0)
                        {
                            foreach (var core in coreLoads)
                            {
                                CpuCores.Add(new CpuCoreModel(core.Key));
                            }
                        }

                        // Update core values
                        for (int i = 0; i < CpuCores.Count; i++)
                        {
                            var coreName = CpuCores[i].Name;
                            if (coreLoads.ContainsKey(coreName))
                            {
                                CpuCores[i].AddUsageValue(coreLoads[coreName]);
                            }
                        }
                    });
                }
            }
        }

        private void UpdateRamStats()
        {
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Memory)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "Memory")
                        {
                            RamUsage = sensor.Value ?? 0;
                        }
                        else if (sensor.SensorType == SensorType.Data)
                        {
                            if (sensor.Name == "Memory Used")
                            {
                                UsedRam = sensor.Value ?? 0;
                            }
                            else if (sensor.Name == "Memory Available")
                            {
                                float availableRam = sensor.Value ?? 0;
                                TotalRam = UsedRam + availableRam;
                            }
                        }
                    }
                    break;
                }
            }
        }

        private void UpdateGpuStats()
        {
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    hardware.Update();

                    // Update GPU name if not set
                    if (string.IsNullOrEmpty(GpuName))
                    {
                        GpuName = hardware.Name;
                    }

                    foreach (var sensor in hardware.Sensors)
                    {
                        switch (sensor.SensorType)
                        {
                            case SensorType.Load:
                                if (sensor.Name == "GPU Core")
                                {
                                    GpuUsage = sensor.Value ?? 0;
                                }
                                break;

                            case SensorType.Temperature:
                                if (sensor.Name == "GPU Core")
                                {
                                    GpuTemperature = sensor.Value ?? 0;
                                }
                                break;

                            case SensorType.SmallData:
                                if (sensor.Name == "GPU Memory Used")
                                {
                                    GpuMemoryUsed = sensor.Value ?? 0;
                                }
                                else if (sensor.Name == "GPU Memory Total")
                                {
                                    GpuMemoryTotal = sensor.Value ?? 0;
                                }
                                break;
                        }
                    }
                    break; // Exit after processing the first GPU
                }
            }
        }

        private void UpdateTime(object state)
    {
        try
        {
            CurrentTime = DateTime.Now.ToString("HH:mm");
            CurrentDate = DateTime.Now.ToString("ddd, MM/dd/yyyy");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Time update error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        try
        {
            _timer?.Dispose();
            if (_isHardwareInitialized)
            {
                _computer?.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Dispose error: {ex.Message}");
        }
    }


        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler<bool> AlwaysOnTopChanged;

    }
}
