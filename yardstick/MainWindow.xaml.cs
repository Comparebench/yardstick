using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;
using Newtonsoft.Json;
using RestSharp;

namespace yardstick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly RestClient _restClient;

        public Profile Profile{ get; set; } = new Profile();
        public List<Profile> Profiles{ get; set; } = new List<Profile>();

        private string _cbScore;

        public string CbScore{
            get => _cbScore;
            set{
                _cbScore = value;
                OnPropertyChanged("cbScore");
            }
        }

        public MainWindow(RestClient restClient){
            _restClient = restClient;
            // Construct basic hardware info
            InitializeComponent();
            Computer computer = new Computer{
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
            
            Profile.CPUModel = computer.Hardware.First(a => a.HardwareType == HardwareType.Cpu);
            //MoboModelName = computer.Hardware.First(a => a.HardwareType == HardwareType.Motherboard).Name;
            Profile.GPUModels = computer.Hardware
                .Where(a => a.HardwareType == HardwareType.GpuAmd || a.HardwareType == HardwareType.GpuNvidia).ToList();
            Profile.MotherboardModel = computer.Hardware.First(a => a.HardwareType == HardwareType.Motherboard);

            foreach (var hardware in computer.Hardware){
                if (hardware.HardwareType == HardwareType.Cpu){
                    foreach (var sensor in hardware.Sensors){
                        if (sensor.SensorType == SensorType.Clock){
                            Console.WriteLine("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                        }
                    }
                }
            }
        }


        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName){
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TestConnection(object sender, RoutedEventArgs e){
            var request = new RestRequest("api/account/profile", Method.POST);
            var response = _restClient.Execute(request);
            Trace.WriteLine(response);
        }

        private void SelectCinebenchLocation(object sender, RoutedEventArgs e){
            using (var fbd = new FolderBrowserDialog()){
                DialogResult result = fbd.ShowDialog();

                if (!string.IsNullOrWhiteSpace(fbd.SelectedPath)){
                    Console.WriteLine("Selected Path: " + fbd.SelectedPath);
                    new BuildBat().Build(fbd.SelectedPath, BenchChoice.Cinebench);
                    var cbResult = new CBRunner().Run();
                    CbScore = cbResult.Split("(")[0].Split("CB ")[1];

                    Profile.ListBenchmarks.Add(new BenchmarkResult{
                        BenchmarkType = "Cinebench",
                        score = CbScore
                    });

                    Profiles.Add(Profile);

                    // ProfileGrid.DataContext = Profiles;
                    OnPropertyChanged("ProfileGrid");
                }
            }
        }

        private void UploadResult(object sender, RoutedEventArgs e){
            var request = new RestRequest("api/benchmarks/upload", Method.POST);

            Profile.Name = BuildName.Text;
            
            JsonSerializerSettings sets = new JsonSerializerSettings(){
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            
            request.AddJsonBody(JsonConvert.SerializeObject(Profile, sets));
            var response = _restClient.Execute(request);
            Trace.WriteLine(response);
        }

        private void mnuNew_Click(object sender, EventArgs e){
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }

        public class CBRunner
        {
            public string Run(){
                // Process p = new Process();
                // p.StartInfo.UseShellExecute = false;
                // p.StartInfo.RedirectStandardOutput = true;
                // p.StartInfo.FileName = @"bench.bat";
                // p.Start();
                // p.WaitForExit();

                using (StreamReader rd = new StreamReader("text.txt")){
                    string[] lines = rd.ReadToEnd()
                        .Split(new string[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                    return lines[^2];
                }
            }
        }
    }
}