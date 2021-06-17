using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;
namespace yardstick{
    public partial class About : Window{
        public About(){
            InitializeComponent();
            Owner = App.Current.MainWindow;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }         
        private void btnClose(object sender, RoutedEventArgs e){
            Close();
        }
    }
}