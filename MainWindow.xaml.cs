using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using ResourceMonitor.ViewModels;
using System.Diagnostics;

namespace ResourceMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Debug.WriteLine("MainWindow initialized");

                // Position window in top right corner with a small margin
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                this.Left = screenWidth - this.Width;
                this.Top = 0;

                _viewModel = new MainViewModel();
                // Subscribe to the always-on-top changed event
                _viewModel.AlwaysOnTopChanged += OnAlwaysOnTopChanged;
                DataContext = _viewModel;

                // Initial setup of always-on-top state
                SetAlwaysOnTopState(_viewModel.IsAlwaysOnTop);

                Debug.WriteLine("ViewModel created and assigned to DataContext");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow initialization error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show("Error initializing application. Please ensure you're running as administrator.", "Initialization Error");
            }
        }

        private void OnAlwaysOnTopChanged(object sender, bool isAlwaysOnTop)
        {
            SetAlwaysOnTopState(isAlwaysOnTop);
        }

        private void SetAlwaysOnTopState(bool isAlwaysOnTop)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (isAlwaysOnTop)
            {
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
            }
            else
            {
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
            }
            Topmost = isAlwaysOnTop; // Keep WPF property in sync
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // Handle or ignore the exception that can occur if DragMove is called while already in a drag operation
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}