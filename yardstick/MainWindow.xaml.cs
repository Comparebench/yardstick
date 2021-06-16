using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using DataFormat = System.Windows.DataFormat;
using MessageBox = System.Windows.MessageBox;

namespace yardstick{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window, INotifyPropertyChanged{
        private string _cpuModelName;
        private string _gpuModelName;
        private RestClient restClient;
        private Profile _profile = new Profile();
        private List<Profile> _profiles = new List<Profile>();
        public string CPUModelName
        {
            get { return _cpuModelName; }
            set
            {
                _cpuModelName = value;
                // OnPropertyChanged("ModelName");
            }
        }
        public string GPUModelName
        {
            get { return _gpuModelName; }
            set
            {
                _gpuModelName = value;
                // OnPropertyChanged("ModelName");
            }
        }
        private string _cbScore;
        public string cbScore
        {
            get { return _cbScore; }
            set
            {
                _cbScore = value;
                OnPropertyChanged("cbScore");
            }
        }

        public MainWindow(RestClient _restClient){
            restClient = _restClient;
            // Construct basic hardware info
            InitializeComponent();
            Computer computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true
            };
            computer.Open();
            computer.Accept(new UpdateVisitor());
            CPUModelName = computer.Hardware[0].Name;
            GPUModelName = computer.Hardware[2].Name;
            _profile.CPUModel = CPUModelName;
            _profile.GPUModel = GPUModelName;
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
        }

     
        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        private void testConnection(object sender, RoutedEventArgs e){
            var request = new RestRequest("api/account/profile", Method.POST);
            var response = restClient.Execute(request);
            Trace.WriteLine(response);
        }

        private void SelectCinebenchLocation(object sender, RoutedEventArgs e)
        {
            using(var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
            
                if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Console.WriteLine("Selected Path: " + fbd.SelectedPath);
                    new BuildBat().Build(fbd.SelectedPath, BenchChoice.Cinebench);
                    var cbResult = new CBRunner().Run();
                    cbScore = cbResult.Split("(")[0].Split("CB ")[1];

                    _profile.ListBenchmarks.Add(new BenchmarkResult{
                        BenchmarkType = "Cinebench",
                        score = cbScore
                    });

                    _profiles.Add(_profile);
                    
                    ProfileGrid.DataContext = _profiles;
                    OnPropertyChanged("ProfileGrid");
                }
            }
        }
        private void UploadResult(object sender, RoutedEventArgs e)
        {
            var client = new RestClient("http://localhost:8180");
            
            var request = new RestRequest("api/benchmarks/upload", Method.POST);
            request.AddJsonBody(JsonConvert.SerializeObject(_profile));
            var response = client.Execute(request);
            Trace.WriteLine(response);
            
        }
        
        private void mnuNew_Click(object sender, EventArgs e){
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }
     
        public class CBRunner
        {
            public string Run()
            {
                // Process p = new Process();
                // p.StartInfo.UseShellExecute = false;
                // p.StartInfo.RedirectStandardOutput = true;
                // p.StartInfo.FileName = @"bench.bat";
                // p.Start();
                // p.WaitForExit();

                using (StreamReader rd = new StreamReader("text.txt"))
                {
                    string[] lines = rd.ReadToEnd()
                        .Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                    return lines[lines.Length - 2];
                }
            }
        }
    }
    
}