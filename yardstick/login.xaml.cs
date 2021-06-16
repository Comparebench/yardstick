using System.Windows;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace yardstick
{
    public partial class login : Window
    {
        private Auth0Client client;
        private readonly string[] _connectionNames = new string[]{
            "google-oauth2",
            "discord",
        };
        
        public login(){
            
            InitializeComponent();
        }
        
        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            string domain = ConfigurationManager.AppSettings["Auth0:Domain"];
            string clientId = ConfigurationManager.AppSettings["Auth0:ClientId"];

            client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId
            });

            var extraParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(connectionNameComboBox.Text))
                extraParameters.Add("connection", connectionNameComboBox.Text);

            if (!string.IsNullOrEmpty(audienceTextBox.Text))
                extraParameters.Add("audience", audienceTextBox.Text);

            DisplayResult(await client.LoginAsync(extraParameters: extraParameters));
        }

        private void DisplayResult(LoginResult loginResult)
        {
            // Display error
            if (loginResult.IsError)
            {
                resultTextBox.Text = loginResult.Error;
                return;
            }
            
            
            /*logoutButton.Visibility = Visibility.Visible;
            loginButton.Visibility = Visibility.Collapsed;

            // Display result
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Tokens");
            sb.AppendLine("------");
            sb.AppendLine($"id_token: {loginResult.IdentityToken}");
            sb.AppendLine($"access_token: {loginResult.AccessToken}");
            sb.AppendLine($"refresh_token: {loginResult.RefreshToken}");
            sb.AppendLine();

            sb.AppendLine("Claims");
            sb.AppendLine("------");*/
            /*_account.Name = loginResult.User.Claims*/
            foreach (var claim in loginResult.User.Claims)
            {
                if (claim.Type == "name"){
                    Account.Name = claim.Value;
                }
                else if (claim.Type == "email"){
                    Account.Email = claim.Value;
                }
                else if (claim.Type == "picture"){
                    Account.Picture = claim.Value;
                }
                /*sb.AppendLine($"{claim.Type}: {claim.Value}");*/
            }
            Close();
            MainWindow mainWindow = new MainWindow();
            mainWindow.ShowDialog();

            /*resultTextBox.Text = sb.ToString();*/
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectionNameComboBox.ItemsSource = _connectionNames;
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            BrowserResultType browserResult = await client.LogoutAsync();

            if (browserResult != BrowserResultType.Success)
            {
                resultTextBox.Text = browserResult.ToString();
                return;
            }

            logoutButton.Visibility = Visibility.Collapsed;
            loginButton.Visibility = Visibility.Visible;

            audienceTextBox.Text = "";
            resultTextBox.Text = "";
            connectionNameComboBox.ItemsSource = _connectionNames;
        }
        
        
    }
}