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
using yardstick.ViewModels;

namespace yardstick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly RestClient _restClient;

        public BuildViewModel BuildViewModel{ get; set; }
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
        }

        private Profile BuildProfile(){
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
            
            Profile profile = new Profile();

            profile.CpuModels = computer.Hardware.Where(a => a.HardwareType == HardwareType.Cpu).ToList();
            profile.GpuModels = computer.Hardware
                .Where(a => a.HardwareType == HardwareType.GpuAmd || a.HardwareType == HardwareType.GpuNvidia).ToList();
            profile.MotherboardModel = computer.Hardware.First(a => a.HardwareType == HardwareType.Motherboard);
            profile.RamModel = computer.Hardware.First(a => a.HardwareType == HardwareType.Memory);
            
            foreach (var hardware in computer.Hardware){
                if (hardware.HardwareType != HardwareType.Cpu) continue;
                foreach (var sensor in hardware.Sensors){
                    if (sensor.SensorType == SensorType.Clock){
                        Console.WriteLine("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                    }
                }
            }

            return profile;
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

                    BuildViewModel.ListBenchmarks.Add(new BenchmarkResult{
                        BenchmarkType = "Cinebench",
                        Score = CbScore
                    });
                    ;

                    // Profiles.Add(BuildViewModel);

                    // ProfileGrid.DataContext = Profiles;
                    OnPropertyChanged("ProfileGrid");
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e){
            BuildViewModel = new BuildViewModel(BuildProfile());

            DataContext = BuildViewModel;
        }

        private void UploadResult(object sender, RoutedEventArgs e){
            var request = new RestRequest("api/benchmarks/upload", Method.POST);

            BuildViewModel.Name = BuildName.Text;

            JsonSerializerSettings sets = new JsonSerializerSettings(){
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            request.AddJsonBody(JsonConvert.SerializeObject(BuildViewModel.Profile, sets));
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
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = @"bench.bat";
                p.Start();
                p.WaitForExit();

                using (StreamReader rd = new StreamReader("text.txt")){
                    string[] lines = rd.ReadToEnd()
                        .Split(new string[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                    return lines[^2];
                }
            }
        }
    }
}