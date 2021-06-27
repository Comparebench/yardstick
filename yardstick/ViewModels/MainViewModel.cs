using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace yardstick.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Profile> _profiles;

        public ObservableCollection<Profile> Profiles{
            get => _profiles;
            set{
                _profiles = value;
                OnPropertyChanged("Profiles");
            }
        }
        private String _cbScore;
        public Boolean Loading{
            get;
            set;
        }

        public MainViewModel(){
            CurrentProfile = new CurrentProfile();
            
        }
        
        public CurrentProfile CurrentProfile{ get; set; }

        public String Name{ get; set; }

        /*public IHardware RamModel => Profile.M;*/

        public String CbScore{
            get => _cbScore;
            set{
                _cbScore = value;
                OnPropertyChanged("CbScore");
            }
        }

        public String Gpu{
            get{
                string gpuName = CurrentProfile.Gpus[0].Name;
                if (CurrentProfile.Gpus[0].Vendor == "Nvidia Corporation"){
                    gpuName += " " + "Founders Edition";
                }

                return gpuName;
            }
        }

        public double GetRamValue{
            get{
                double capacity = 0.0;
                for (int i = 0; i < CurrentProfile.RAMSticks.Count; i++){
                    capacity += (CurrentProfile.RAMSticks[i].Capacity / Math.Pow(1024, 3));
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

        public List<BenchmarkResult> ListBenchmarks => CurrentProfile.ListBenchmarks;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}