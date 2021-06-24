using System;
using System.Collections.Generic;
using System.ComponentModel;

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

        /*public IHardware RamModel => Profile.M;*/

        public String CbScore{
            get => _cbScore;
            set{
                _cbScore = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CbScore"));
            }
        }

        public String Gpu{
            get{
                string gpuName = Profile.Gpus[0].Name;
                if (Profile.Gpus[0].Vendor == "Nvidia Corporation"){
                    gpuName += " " + "Founders Edition";
                }

                return gpuName;
            }
        }

        public double GetRamValue{
            get{
                double capacity = 0.0;
                for (int i = 0; i < Profile.RAMSticks.Count; i++){
                    capacity += (Profile.RAMSticks[i].Capacity / Math.Pow(1024, 3));
                }

                return capacity;
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