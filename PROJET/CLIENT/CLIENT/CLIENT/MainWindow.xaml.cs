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

        // Méthode appelée lorsqu'une touche est enfoncée dans la zone de texte
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // Vérifie si la touche enfoncée est la touche Enter
            {
                EncryptText(); // Appelle la méthode EncryptText pour chiffrer le texte
            }
        }

        // Méthode appelée lorsqu'on clique sur le bouton
        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            EncryptText(); // Appelle la méthode EncryptText pour chiffrer le texte
        }

        // Méthode pour chiffrer le texte saisi par l'utilisateur
        private void EncryptText()
        {
            // Récupère le texte saisi, le convertit en majuscules et enlève les espaces
            string text = InputTextBox.Text.ToUpper().Replace(" ", "");

            // Vérifie que le texte ne contient que des lettres majuscules
            if (!System.Text.RegularExpressions.Regex.IsMatch(text, "^[A-Z]+$"))
            {
                MessageBox.Show("Le texte doit contenir uniquement des lettres."); // Affiche un message d'erreur si le texte contient des caractères non autorisés
                return; // Sort de la méthode si le texte est invalide
            }

            // Détermine la méthode de chiffrement sélectionnée
            char method = 'C'; // Méthode par défaut : César
            if (PlayfairRadioButton.IsChecked == true)
                method = 'P'; // Méthode Playfair
            else if (SubstitutionRadioButton.IsChecked == true)
                method = 'S'; // Méthode Substitution

            // Formate la requête en une chaîne contenant la méthode et le texte à chiffrer
            string request = $"{method}|{text}";

            // Envoie la requête au serveur et récupère la réponse
            string response = SendRequest(request);

            // Affiche la réponse dans l'étiquette OutputLabel
            OutputLabel.Content = response;
        }

        // Méthode pour envoyer la requête au serveur et récupérer la réponse
        private string SendRequest(string request)
        {
            try
            {
                // Création d'un client TCP et connexion au serveur sur le port 6666
                TcpClient client = new TcpClient("127.0.0.1", 6666);
                NetworkStream stream = client.GetStream();

                // Convertit la requête en tableau d'octets et l'envoie au serveur
                byte[] data = Encoding.ASCII.GetBytes(request);
                stream.Write(data, 0, data.Length);

                // Prépare un buffer pour recevoir la réponse du serveur
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);

                // Convertit la réponse en chaîne de caractères
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                client.Close(); // Ferme la connexion

                return response; // Retourne la réponse
            }
            catch (Exception ex)
            {
                // Affiche un message d'erreur si la connexion au serveur échoue
                MessageBox.Show("Erreur de connexion au serveur : " + ex.Message);
                return string.Empty; // Retourne une chaîne vide en cas d'erreur
            }
        }

    }
}