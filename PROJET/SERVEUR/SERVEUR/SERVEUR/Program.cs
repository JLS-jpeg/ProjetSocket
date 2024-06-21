using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class CryptoServer
{
    private static readonly int port = 6666; // Déclaration du port sur lequel le serveur écoutera les connexions

    public static void Main()
    {
        // Création d'un TcpListener pour écouter les connexions entrantes sur toutes les adresses IP disponibles sur le port spécifié
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start(); // Démarre le TcpListener pour commencer à écouter les connexions
        Console.WriteLine("Attente client");

        // Boucle infinie pour accepter et traiter les connexions clients
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient(); // Accepte une connexion client entrante
            NetworkStream stream = client.GetStream(); // Obtient le flux réseau pour envoyer et recevoir des données
            byte[] buffer = new byte[1024]; // Crée un buffer pour stocker les données reçues
            int bytesRead = stream.Read(buffer, 0, buffer.Length); // Lit les données envoyées par le client
            string request = Encoding.ASCII.GetString(buffer, 0, bytesRead); // Convertit les données reçues en chaîne de caractères

            // Journalise les détails de la requête
            string clientEndpoint = client.Client.RemoteEndPoint.ToString();
            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {clientEndpoint} - {request}");

            // Traite la requête et génère une réponse
            string response = HandleRequest(request);
            byte[] responseBytes = Encoding.ASCII.GetBytes(response); // Convertit la réponse en tableau d'octets
            stream.Write(responseBytes, 0, responseBytes.Length); // Envoie la réponse au client
            client.Close(); // Ferme la connexion client
            Console.WriteLine("Attente client");
        }
    }

    // Méthode pour traiter la requête reçue du client
    private static string HandleRequest(string request)
    {
        // Sépare la requête en utilisant le caractère '|' comme délimiteur
        string[] parts = request.Split('|');
        if (parts.Length != 2)
        {
            return "Invalid request"; // Si la requête n'a pas le bon format, retourne un message d'erreur
        }

        char method = parts[0][0]; // Le premier caractère de la requête indique la méthode de chiffrement à utiliser
        string text = parts[1].ToUpper().Replace(" ", ""); // Récupère le texte à chiffrer, le convertit en majuscules et enlève les espaces

        // Sélectionne la méthode de chiffrement en fonction du caractère spécifié
        switch (method)
        {
            case 'C':
                return CaesarCipher(text, 3); // Méthode de chiffrement de César
            case 'P':
                return PlayfairCipher(text); // Méthode de chiffrement de Playfair
            case 'S':
                return SubstitutionCipher(text); // Méthode de chiffrement par substitution
            default:
                return "Invalid method"; // Retourne un message d'erreur si la méthode est invalide
        }
    }

    // Méthode pour chiffrer un texte en utilisant le chiffrement de César
    private static string CaesarCipher(string input, int shift)
    {
        StringBuilder result = new StringBuilder(); // Utilise StringBuilder pour construire la chaîne chiffrée
        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                char d = (char)((c + shift - 'A') % 26 + 'A'); // Effectue le décalage de César
                result.Append(d); // Ajoute le caractère chiffré au résultat
            }
        }
        return result.ToString(); // Retourne le texte chiffré
    }

    // Méthode pour chiffrer un texte en utilisant le chiffrement de Playfair
    private static string PlayfairCipher(string input)
    {
        // Remplace W par X
        input = input.Replace('W', 'X');
        // Ajoute un X si la longueur est impaire
        if (input.Length % 2 != 0)
        {
            input += "X";
        }

        // Matrice Playfair 5x5 (sans W)
        char[,] keySquare = {
            {'B', 'Y', 'D', 'G', 'Z'},
            {'J', 'S', 'F', 'U', 'P'},
            {'L', 'A', 'R', 'K', 'X'},
            {'C', 'O', 'I', 'V', 'E'},
            {'Q', 'N', 'M', 'H', 'T'}
        };

        StringBuilder result = new StringBuilder(); // Utilise StringBuilder pour construire la chaîne chiffrée

        // Parcourt le texte par paires de caractères
        for (int i = 0; i < input.Length; i += 2)
        {
            char a = input[i];
            char b = input[i + 1];

            int rowA = -1, colA = -1, rowB = -1, colB = -1;

            // Trouve les positions des caractères dans la keySquare
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (keySquare[row, col] == a)
                    {
                        rowA = row;
                        colA = col;
                    }
                    if (keySquare[row, col] == b)
                    {
                        rowB = row;
                        colB = col;
                    }
                }
            }

            // Applique les règles de chiffrement de Playfair
            if (rowA == rowB)
            {
                // Même ligne, déplacer à droite
                result.Append(keySquare[rowA, (colA + 1) % 5]);
                result.Append(keySquare[rowB, (colB + 1) % 5]);
            }
            else if (colA == colB)
            {
                // Même colonne, déplacer vers le bas
                result.Append(keySquare[(rowA + 1) % 5, colA]);
                result.Append(keySquare[(rowB + 1) % 5, colB]);
            }
            else
            {
                // Cas du rectangle, garder l'ordre initial des caractères
                result.Append(keySquare[rowB, colA]);
                result.Append(keySquare[rowA, colB]);

            }
        }

        return result.ToString(); // Retourne le texte chiffré
    }

    // Méthode pour chiffrer un texte en utilisant le chiffrement par substitution
    private static string SubstitutionCipher(string input)
    {
        string clear = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Texte clair
        string cipher = "HIJKLMNVWXYZBCADEFGOPQRSTU"; // Texte chiffré correspondant

        StringBuilder result = new StringBuilder(); // Utilise StringBuilder pour construire la chaîne chiffrée
        foreach (char c in input)
        {
            int index = clear.IndexOf(c); // Trouve l'index du caractère dans le texte clair
            if (index >= 0)
            {
                result.Append(cipher[index]); // Ajoute le caractère chiffré correspondant au résultat
            }
        }
        return result.ToString(); // Retourne le texte chiffré
    }
}
