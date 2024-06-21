using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class CryptoServer
{
    private static readonly int port = 6666;

    public static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine("Attente client");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            // Log the request details
            string clientEndpoint = client.Client.RemoteEndPoint.ToString();
            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} - {clientEndpoint} - {request}");

            string response = HandleRequest(request);
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
            client.Close();
            Console.WriteLine("Attente client");
        }
    }

    private static string HandleRequest(string request)
    {
        string[] parts = request.Split('|');
        if (parts.Length != 2)
        {
            return "Invalid request";
        }

        char method = parts[0][0];
        string text = parts[1].ToUpper().Replace(" ", "");

        switch (method)
        {
            case 'C':
                return CaesarCipher(text, 3);
            case 'P':
                return PlayfairCipher(text);
            case 'S':
                return SubstitutionCipher(text);
            default:
                return "Invalid method";
        }
    }

    private static string CaesarCipher(string input, int shift)
    {
        StringBuilder result = new StringBuilder();
        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                char d = (char)((c + shift - 'A') % 26 + 'A');
                result.Append(d);
            }
        }
        return result.ToString();
    }

    private static string PlayfairCipher(string input)
    {
        // Remplacer W par X
        input = input.Replace('W', 'X');
        // Ajouter un X si la longueur est impaire
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

        StringBuilder result = new StringBuilder();

        for (int i = 0; i < input.Length; i += 2)
        {
            char a = input[i];
            char b = input[i + 1];

            int rowA = -1, colA = -1, rowB = -1, colB = -1;

            // Trouver les positions des caractères dans la keySquare
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
                result.Append(keySquare[rowA, colB]);
                result.Append(keySquare[rowB, colA]);
            }
        }

        return result.ToString();
    }

    private static string SubstitutionCipher(string input)
    {
        string clear = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string cipher = "HIJKLMNVWXYZBCADEFGOPQRSTU";

        StringBuilder result = new StringBuilder();
        foreach (char c in input)
        {
            int index = clear.IndexOf(c);
            if (index >= 0)
            {
                result.Append(cipher[index]);
            }
        }
        return result.ToString();
    }
}
