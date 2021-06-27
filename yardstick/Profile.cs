using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace yardstick
{
    public class Profile
    {
        private string _uid;
        private string _Name;
        private Int16 _ID;
        public ObservableCollection<IGPU> _Gpus;
        
        public String uid{
            get => _uid;
            set{ _uid = value; }
        }
        public Int16 ID{
            get => _ID;
            set{ _ID = value; }
        }
        public String Name{ get; set; }
        
        public ObservableCollection<IGPU> Gpus{
            get => _Gpus;
            set{ _Gpus = value; }
        }
        public ICPU Cpu{ get;set; }
        
        public Profile(dynamic profileData){
            uid = profileData.uid;
            ID = profileData.id;
            Name = profileData.title;
            // Add GPUs
            ObservableCollection<IGPU> _Gpus = new ObservableCollection<IGPU>();
            for (var i = 0; i < profileData.gpu.Count; i++){
                _Gpus.Add(new IGPU(profileData.gpu[i]));
            }
            Gpus = _Gpus;
            // Add CPU
            Cpu = new ICPU();
            Cpu.CPUId = profileData.cpu_id;
            Cpu.Model = profileData.model;

        }


        
        
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        
    }
    public class IGPU
    {
        public IGPU(dynamic gpuData){
            make = gpuData.make;
            model = gpuData.model;
        }
        public String make{ get; set; }
        public String model{ get; set; }
    }
    public class ICPU
    {
        public ICPU(){
            /*CPUId = CpuData.cpu_id;
            Make = CpuData.make;
            Model = CpuData.model;*/
        }
        public Int16 CPUId{ get; set; }
        public String Make{ get; set; }
        public String Model{ get; set; }
    }
}