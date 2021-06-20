using System;
using System.IO;
using LibreHardwareMonitor.Hardware;

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
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor)
        {
        }

        public void VisitParameter(IParameter parameter)
        {
        }
    }
}