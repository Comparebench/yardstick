using System;
using System.ComponentModel;

namespace yardstick
{
    public static class Account
    {
        private static string _displayName;
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static String Name{ get; set; }

        public static String DisplayName {
            get{ return _displayName;}
            set{
                _displayName = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(DisplayName)));
            }
        }

        public static String Picture{ get; set; }
        public static String Email{ get; set; }
        public static String Token{ get; set; }
        public static Boolean IsLoggedIn{ get; set; }
    }
}