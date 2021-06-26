using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;

namespace yardstick.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private String _cbScore;
        public Boolean Loading{
            get;
            set;
        }

        public MainViewModel(){
            Profile = new Profile();
        }


        public Profile Profile{ get; set; }
        

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
        
        private async void GetProfileAsync(){
            Loading = true;
            //Invoked on the UI thread
            //Run RetrieveDataAsync on a background thread
            await Task.Run(() => {
                
            });
            //Also invoked on the UI thread
            Loading = false;
        }

        public List<BenchmarkResult> ListBenchmarks => Profile.ListBenchmarks;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(PropertyChangedEventArgs e){
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}