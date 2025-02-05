using System.Net.Sockets;
using System.Net;
using System.Text;

namespace TicTacToe
{
    public partial class MainPage : ContentPage
    {
        private readonly IGameService _gameService;
        private TcpListener _server;
        private List<TcpClient> _clients = new();
        private char[] _board = new char[9];
        private bool _isPlayerXTurn = true;
        private bool _isServer = false;

        public Button Btn4 { get; private set; }
        public Button Btn0 { get; private set; }
        public Button Btn1 { get; private set; }
        public Button Btn2 { get; private set; }
        public Button Btn3 { get; private set; }
        public Button Btn5 { get; private set; }
        public Button Btn6 { get; private set; }
        public Button Btn7 { get; private set; }
        public Button Btn8 { get; private set; }
        public Label StatusLabel { get; private set; }
        public Entry PortEntry { get; private set; }
        public Label TurnLabel { get; private set; }

        

        private void UpdateBoard(string[] board)
        {
            var buttons = new[] { Btn0, Btn1, Btn2, Btn3, Btn4, Btn5, Btn6, Btn7, Btn8 };
            for (int i = 0; i < board.Length; i++)
            {
                buttons[i].Text = board[i];
                buttons[i].IsEnabled = string.IsNullOrWhiteSpace(board[i]);
            }
        }
        private void CheckGameResult()
        {
            var winPatterns = new int[][] {
            new int[] {0,1,2}, new int[] {3,4,5}, new int[] {6,7,8},
            new int[] {0,3,6}, new int[] {1,4,7}, new int[] {2,5,8},
            new int[] {0,4,8}, new int[] {2,4,6}
        };

            foreach (var pattern in winPatterns)
            {
                if (_board[pattern[0]] != '\0' &&
                    _board[pattern[0]] == _board[pattern[1]] &&
                    _board[pattern[1]] == _board[pattern[2]])
                {
                    BroadcastGameState($"Wygrywa {_board[pattern[0]]}!");
                    return;
                }
            }

            if (!_board.Contains('\0'))
            {
                BroadcastGameState("Remis!");
            }
        }

    }
}
    