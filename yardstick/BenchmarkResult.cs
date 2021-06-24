using System;
using System.Collections.Generic;
using HardwareInformation;
using HardwareInformation.Information;

namespace yardstick
{
    public class Profile
    {
        private MachineInformation _machineInformation;
        public String Name{ get; set; }
        
        public Profile(){
            _machineInformation = MachineInformationGatherer.GatherInformation();
            ListBenchmarks = new List<BenchmarkResult>();
            Cpu = _machineInformation.Cpu;
            Gpus = _machineInformation.Gpus;
            Motherboard = _machineInformation.SmBios;
            RAMSticks = _machineInformation.RAMSticks;
        }

        public CPU Cpu { get; set; }

        public IReadOnlyList<GPU> Gpus{ get; set; }

        public SMBios Motherboard { get; set; }
        
        public IReadOnlyList<RAM> RAMSticks { get; set; }
      
        public List<BenchmarkResult> ListBenchmarks{ get; set; }
    }

    public class BenchmarkResult
    {
        public string BenchmarkType{ get; set; }
        public string Score{ get; set; }
    }
}