using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibreHardwareMonitor.Hardware;

namespace yardstick{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window, INotifyPropertyChanged{
        private string _modelName;

        public string ModelName
        {
            get { return _modelName; }
            set
            {
                _modelName = value;
                // OnPropertyChanged("ModelName");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Computer computer = new Computer
            {
                IsCpuEnabled = true
            };
            computer.Open();
            computer.Accept(new UpdateVisitor());
            ModelName = computer.Hardware[0].Name;
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    foreach(var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Clock)
                        {
                            Console.WriteLine("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                        }
                    }                    
                }
               
            }
            // new BuildBat().Build();
        }
        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;
    }
    
}