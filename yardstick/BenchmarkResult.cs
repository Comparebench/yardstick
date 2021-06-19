using System;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;

namespace yardstick
{
    public class Profile
    {
        public String Name{ get; set; }
        
        public Profile(){
            ListBenchmarks = new List<BenchmarkResult>();
        }

        public IHardware CPUModel{ get; set; }

        public List<IHardware> GPUModels{ get; set; }

        public IHardware MotherboardModel{ get; set; }

        public List<BenchmarkResult> ListBenchmarks{ get; set; }
    }

    public class BenchmarkResult
    {
        public string BenchmarkType{ get; set; }
        public string score{ get; set; }
    }
}