using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace yardstick.ViewModels
{
    public class BuildViewModel : INotifyPropertyChanged
    {
        private String _cbScore;

        public BuildViewModel(Profile profile){
            Profile = profile;
        }

        public BuildViewModel(){ }

        public Profile Profile{ get; }

        public String Name{ get; set; }

        public List<IHardware> CpuModels => Profile.CpuModels;

        public List<IHardware> GpuModels => Profile.GpuModels;

        public IHardware MotherboardModel => Profile.MotherboardModel;

        public IHardware RamModel => Profile.RamModel;

        public String CbScore{
            get => _cbScore;
            set{
                _cbScore = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CbScore"));
            }
        }

        public float? GetRamValue{
            get{
                return Profile.RamModel.Sensors.SingleOrDefault(a => a.Name == "Memory Used")?.Max +
                       Profile.RamModel.Sensors.SingleOrDefault(a => a.Name == "Memory Available")?.Max;
            }
        }

        public List<BenchmarkResult> ListBenchmarks => Profile.ListBenchmarks;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(PropertyChangedEventArgs e){
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}