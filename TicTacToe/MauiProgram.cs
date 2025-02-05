using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace TicTacToeServer
{
    public class TicTacToeService
    {
        private static char[] board = { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
        private static int currentPlayer = 1;
        private TcpClient player1, player2;
        private NetworkStream stream1, stream2;
        private readonly object lockObject = new();
        private TcpListener server;

        public async Task StartServer()
        {
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Serwer uruchomiony. Oczekiwanie na graczy...");

            player1 = await server.AcceptTcpClientAsync();
            Console.WriteLine("Gracz 1 połączony.");
            stream1 = player1.GetStream();
            SendMessage(stream1, "Jesteś Graczem 1 (X). Czekaj na drugiego gracza...");

            player2 = await server.AcceptTcpClientAsync();
            Console.WriteLine("Gracz 2 połączony.");
            stream2 = player2.GetStream();
            SendMessage(stream2, "Jesteś Graczem 2 (O). Gra rozpoczęta!");

            SendMessage(stream1, "Gra rozpoczęta!");

            _ = Task.Run(() => HandleClient(player1, 1, stream1, stream2));
            _ = Task.Run(() => HandleClient(player2, 2, stream2, stream1));
        }

        private async Task HandleClient(TcpClient client, int player, NetworkStream myStream, NetworkStream opponentStream)
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int bytesRead = await myStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (int.TryParse(message, out int move) && move >= 0 && move <= 8)
                    {
                        lock (lockObject)
                        {
                            if (board[move] == ' ' && currentPlayer == player)
                            {
                                board[move] = (player == 1) ? 'X' : 'O';
                                currentPlayer = (currentPlayer == 1) ? 2 : 1;

                                string boardState = new string(board);
                                SendMessage(myStream, boardState);
                                SendMessage(opponentStream, boardState);

                                if (CheckWin())
                                {
                                    SendMessage(myStream, "Wygrałeś!");
                                    SendMessage(opponentStream, "Przegrałeś!");
                                    ResetGame();
                                }
                                else if (CheckDraw())
                                {
                                    SendMessage(myStream, "Remis!");
                                    SendMessage(opponentStream, "Remis!");
                                    ResetGame();
                                }
                            }
                            else
                            {
                                SendMessage(myStream, "Nieprawidłowy ruch. Spróbuj ponownie.");
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine($"Gracz {player} rozłączony.");
                    break;
                }
            }
        }

        private void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }


        private bool CheckWin()
        {
            int[][] winPositions = new int[][]
            {
        new int[] {0, 1, 2},
        new int[] {3, 4, 5},
        new int[] {6, 7, 8},
        new int[] {0, 3, 6},
        new int[] {1, 4, 7},
        new int[] {2, 5, 8},
        new int[] {0, 4, 8},
        new int[] {2, 4, 6}
            };

            foreach (var position in winPositions)
            {
                if (board[position[0]] != ' ' &&
                    board[position[0]] == board[position[1]] &&
                    board[position[1]] == board[position[2]])
                    return true;
            }
            return false;
        }

        private bool CheckDraw()
        {
            foreach (char c in board)
                if (c == ' ') return false;
            return true;
        }

        private void ResetGame()
        {
            board = new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
            currentPlayer = 1;
        }
    }

    public partial class MainPage : ContentPage
    {
        private TcpClient client;
        private NetworkStream stream;
        private Button[] buttons;
        private Label statusLabel;
        private Button btn0;
        private Button btn1;
        private Button btn2;
        private Button btn3;
        private Button btn4;
        private Button btn5;
        private Button btn6;
        private Button btn7;
        private Button btn8;

        public MainPage()
        {
            InitializeComponent();
            buttons = new Button[9] { btn0, btn1, btn2, btn3, btn4, btn5, btn6, btn7, btn8 };
            ConnectToServer();
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }

        private async void ConnectToServer()
        {
            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);
            stream = client.GetStream();
            ListenForMessages();
        }

        private async void OnButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Text == "")
            {
                int index = Array.IndexOf(buttons, btn);
                byte[] data = Encoding.UTF8.GetBytes(index.ToString());
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        private async void ListenForMessages()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (message.Length == 9)
                {
                    UpdateBoard(message);
                }
                else
                {
                    statusLabel.Text = message;
                }
            }
        }

        private void UpdateBoard(string boardState)
        {
            for (int i = 0; i < 9; i++)
            {
                buttons[i].Text = boardState[i].ToString();
            }
        }
    }
}