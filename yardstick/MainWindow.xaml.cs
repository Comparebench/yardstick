using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
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

        public MainWindow(){
            var paletteHelper = new PaletteHelper();
            //Retrieve the app's existing theme
            ITheme theme = paletteHelper.GetTheme();
            //Change the base theme to Dark
            theme.SetBaseTheme(Theme.Dark);
            //Change the app's current theme
            paletteHelper.SetTheme(theme);
            InitializeComponent();
            api = new Api();
            
            MainViewModel = new MainViewModel();
            MainViewModel.Loading = true;
            
            AttemptLogin();
            if (Account.IsLoggedIn){
                api.getAccountDetails();
                Trace.WriteLine("Got account details");
                Trace.WriteLine(Account.DisplayName);
                
                MainViewModel.Profiles = api.getProfiles();
                
                DataContext = MainViewModel;
                
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


        private void OpenDialog(object sender, RoutedEventArgs e){
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "*.3dmark-result files | *.3dmark-result";
            open.Multiselect = true;
            open.Title = "Open 3DMark Files";
            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                MainViewModel.CurrentProfile.BenchmarkFiles = open.FileNames;
            }
        }

        private void UploadResult(object sender, RoutedEventArgs e){
            api.UploadResult(MainViewModel.CurrentProfile);
        }
        
        private void SelectCinebenchLocation(object sender, RoutedEventArgs e){
            using var fbd = new FolderBrowserDialog();
            fbd.ShowDialog();

            if (string.IsNullOrWhiteSpace(fbd.SelectedPath)) return;
            
            Console.WriteLine("Selected Path: " + fbd.SelectedPath);
            new BuildBat().Build(fbd.SelectedPath, BenchChoice.Cinebench);
            var cbResult = CbRunner.Run();
            MainViewModel.CbScore = cbResult.Split("(")[0].Split("CB ")[1];

            MainViewModel.CurrentProfile.ListBenchmarks.Add(new BenchmarkResult{
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