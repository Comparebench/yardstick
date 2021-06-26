using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Auth0.OidcClient;
using HardwareInformation;
using Newtonsoft.Json;
using RestSharp;
using yardstick.ViewModels;
using Application = System.Windows.Application;

namespace yardstick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Api api;

        private MainViewModel MainViewModel{ get; set; }
        public List<Profile> Profiles{ get; set; } = new List<Profile>();

        public MainWindow(){
            api = new Api();
            InitializeComponent();
            MainViewModel = new MainViewModel();
            MainViewModel.Loading = true;
            DataContext = MainViewModel;
            AttemptLogin();
            if (Account.IsLoggedIn){
                api.getAccountDetails();
            }
        }

        private void AttemptLogin(){
            if (File.Exists("auth.json")){
                using (StreamReader file = File.OpenText("auth.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Account.Token = (string)serializer.Deserialize(file, typeof(string));
                    file.Close();
                }
                api.Authenticate();
            }
            else{
                var loginWindow = new Login();
                loginWindow.ShowDialog();
                api.Authenticate();
            }
        }

        private void SelectCinebenchLocation(object sender, RoutedEventArgs e){
            using var fbd = new FolderBrowserDialog();
            fbd.ShowDialog();

            if (string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;
            
            Console.WriteLine("Selected Path: " + fbd.SelectedPath);
            new BuildBat().Build(fbd.SelectedPath, BenchChoice.Cinebench);
            var cbResult = CbRunner.Run();
            MainViewModel.CbScore = cbResult.Split("(")[0].Split("CB ")[1];

            MainViewModel.Profile.ListBenchmarks.Add(new BenchmarkResult{
                BenchmarkType = "Cinebench",
                Score = MainViewModel.CbScore
            });
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
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }
}