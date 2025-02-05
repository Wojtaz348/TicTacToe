namespace TicTacToe
{
    internal class GameUpdateEventArgs
    {
        public object CurrentTurn { get; internal set; }
        public string? GameResult { get; internal set; }
        public object Board { get; internal set; }
    }
}