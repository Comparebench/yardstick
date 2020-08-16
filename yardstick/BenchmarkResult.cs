using System;
using System.Collections.Generic;

namespace yardstick
{
    public class Profile
    {
        public Profile(){
            ListBenchmarks = new List<BenchmarkResult>();
        }

        public String CPUModel{ get; set; }

        public List<BenchmarkResult> ListBenchmarks{ get; set; }
    }

    public class BenchmarkResult
    {
        public string BenchmarkType{ get; set; }
        public string score{ get; set; }
    }
}