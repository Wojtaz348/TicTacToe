namespace TicTacToe
{
    public interface IGameService
    {
        Action<object, GameUpdateEventArgs> GameUpdated { get; set; }
        Action<object, string> ErrorOccurred { get; set; }
    }
}