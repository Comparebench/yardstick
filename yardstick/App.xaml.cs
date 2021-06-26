using System;
using System.IO;
using MaterialDesignThemes.Wpf;

namespace yardstick{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        
    }
    public class CbRunner
    {
        public string Run(){
            // Process p = new Process();
            // p.StartInfo.UseShellExecute = false;
            // p.StartInfo.RedirectStandardOutput = true;
            // p.StartInfo.FileName = @"bench.bat";
            // p.Start();
            // p.WaitForExit();

            using StreamReader rd = new StreamReader("text.txt");
            var lines = rd.ReadToEnd()
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            return lines[^2];
        }
    }
}