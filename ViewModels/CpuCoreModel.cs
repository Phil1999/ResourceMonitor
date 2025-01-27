using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ResourceMonitor.Models
{
    public class CpuCoreModel : INotifyPropertyChanged
    {
        private Queue<float> _usageHistory;
        private float _currentUsage;
        private string _name;

        public CpuCoreModel(string name)
        {
            _name = name;
            _usageHistory = new Queue<float>(60); // 1 minute of history
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public float CurrentUsage
        {
            get => _currentUsage;
            set
            {
                _currentUsage = value;
                OnPropertyChanged();
            }
        }

        public Queue<float> UsageHistory
        {
            get => _usageHistory;
            set
            {
                _usageHistory = value;
                OnPropertyChanged();
            }
        }

        public void AddUsageValue(float value)
        {
            CurrentUsage = value;

            if (UsageHistory.Count >= 60)
                UsageHistory.Dequeue();

            UsageHistory.Enqueue(value);
            OnPropertyChanged(nameof(UsageHistory));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}