using System;
using System.Collections.Generic;
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
    public partial class MainWindow
    {
        private readonly RestClient _restClient;
        private Account _account;

        private BuildViewModel BuildViewModel{ get; set; }
        public List<Profile> Profiles{ get; set; } = new List<Profile>();

        public MainWindow(RestClient restClient, Account account){
            _account = account;
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

            var profile = new Profile{
                CpuModels = computer.Hardware.Where(a => a.HardwareType == HardwareType.Cpu).ToList(),
                GpuModels = computer.Hardware
                    .Where(a => a.HardwareType == HardwareType.GpuAmd || a.HardwareType == HardwareType.GpuNvidia)
                    .ToList(),
                MotherboardModel = computer.Hardware.First(a => a.HardwareType == HardwareType.Motherboard),
                RamModel = computer.Hardware.First(a => a.HardwareType == HardwareType.Memory)
            };

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

        // private void TestConnection(object sender, RoutedEventArgs e){
        //     var request = new RestRequest("api/account/profile", Method.POST);
        //     var response = _restClient.Execute(request);
        //     Trace.WriteLine(response);
        // }

        private void SelectCinebenchLocation(object sender, RoutedEventArgs e){
            using var fbd = new FolderBrowserDialog();
            fbd.ShowDialog();

            if (string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;
            
            Console.WriteLine("Selected Path: " + fbd.SelectedPath);
            new BuildBat().Build(fbd.SelectedPath, BenchChoice.Cinebench);
            var cbResult = CbRunner.Run();
            BuildViewModel.CbScore = cbResult.Split("(")[0].Split("CB ")[1];

            BuildViewModel.Profile.ListBenchmarks.Add(new BenchmarkResult{
                BenchmarkType = "Cinebench",
                Score = BuildViewModel.CbScore
            });
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

            String irr = JsonConvert.SerializeObject(BuildViewModel.Profile, sets);

            if(!File.Exists(BuildViewModel.Name + ".json"))
                File.Create(BuildViewModel.Name + ".json").Close();
            File.WriteAllText(BuildViewModel.Name + ".json", irr);

            request.AddJsonBody(irr);
            var response = _restClient.Execute(request);
            Trace.WriteLine(response);
        }

        private void mnuNew_Click(object sender, EventArgs e){
            var aboutWindow = new About();
            aboutWindow.ShowDialog();
        }

        public static class CbRunner
        {
            public static string Run(){
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = @"bench.bat";
                p.Start();
                p.WaitForExit();

                using var rd = new StreamReader("text.txt");
                var lines = rd.ReadToEnd()
                    .Split(new[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                return lines[^2];
            }
        }
    }
}