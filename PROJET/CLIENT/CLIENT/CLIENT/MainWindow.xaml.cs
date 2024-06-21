using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PromeoCrypto
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EncryptText();
            }
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            EncryptText();
        }

        private void EncryptText()
        {
            string text = InputTextBox.Text.ToUpper().Replace(" ", "");
            if (!System.Text.RegularExpressions.Regex.IsMatch(text, "^[A-Z]+$"))
            {
                MessageBox.Show("Le texte doit contenir uniquement des lettres.");
                return;
            }

            char method = 'C';
            if (PlayfairRadioButton.IsChecked == true)
                method = 'P';
            else if (SubstitutionRadioButton.IsChecked == true)
                method = 'S';

            string request = $"{method}|{text}";

            string response = SendRequest(request);
            OutputLabel.Content = response;
        }

        private string SendRequest(string request)
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 6666);
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.ASCII.GetBytes(request);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);

                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                client.Close();

                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur de connexion au serveur : " + ex.Message);
                return string.Empty;
            }
        }

        private void InputTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}