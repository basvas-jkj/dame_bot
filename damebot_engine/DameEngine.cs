using System.Diagnostics;
using System.Threading.Tasks;

namespace damebot_engine
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="next_player"></param>
    public delegate void MoveEvent(IPlayer? next_player);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="move"></param>
    public delegate void MarkEvent(MOVE move);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="piece"></param>
    public delegate void GameOverEvent(IPlayer piece);
    /// <summary>
    /// 
    /// </summary>
    public interface IEngine
    {
        bool IsOnMovePiece(SQUARE position);
        MOVE_INFO ValidateMove(Piece piece, MOVE move, SQUARE next);
        void PerformMove(MOVE move);
        void PerformAutomaticMove();

        /// <summary>
        /// Event raised when any piece is moved.
        /// </summary>
        event MoveEvent? OnMove;
        /// <summary>
        /// Event raised when computer marks move which it wants to perform.
        /// </summary>
        event MarkEvent? OnMark;
        /// <summary>
        /// Event raised the player on the move run out of their possibilities ()including the case they have lost all of their pieces).
        /// </summary>
        event GameOverEvent? OnGameOver;
    }

    /// <summary>
    /// An object that carries the resources of the game and controlling the events that take place during the game.
    /// </summary>
    /// <param name="board">The game board on which the game take place.</param>
    /// <param name="white">The player holding white pieces.</param>
    /// <param name="black">The player holding black pieces.</param>
    public class DameEngine(IBoard board, IPlayer white, IPlayer black): IEngine
    {
        /// <summary>
        /// The game board on which the game take place.
        /// </summary>
        IBoard board = board;
        /// <summary>
        /// Represents the player holding white pieces.
        /// </summary>
        IPlayer white = white;
        /// <summary>
        /// Represents the player holding black pieces.
        /// </summary>
        IPlayer black = black;

        private IPlayer player_on_move = white;
        private IPlayer waiting_player
        {
            get => (player_on_move == white) ? black : white;
        }
        private void SwitchPlayers()
        {
            player_on_move = (player_on_move == white) ? black : white;
        }

        /// <summary>
        /// Event raised when any piece is moved.
        /// </summary>
        public event MoveEvent? OnMove;
        /// <summary>
        /// Event raised when computer marks move which it wants to perform.
        /// </summary>
        public event MarkEvent? OnMark;
        /// <summary>
        /// Event raised the player on the move run out of their possibilities ()including the case they have lost all of their pieces).
        /// </summary>
        public event GameOverEvent? OnGameOver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsOnMovePiece(SQUARE position)
        {
            if (player_on_move == white)
            {
                return board[position] is WhiteMan || board[position] is WhiteKing;
            }
            else
            {
                return board[position] is BlackMan || board[position] is BlackKing;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="move"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public MOVE_INFO ValidateMove(Piece piece, MOVE move, SQUARE next)
        {
            MOVE_INFO info = piece.GetMoveInfo(board, move, next);

            if (info.CompleteJump || info.IncompleteJump)
            {
                return info;
            }
            if (info.Move && !player_on_move.CanCapture(board))
            {
                return info;
            }
            else
            {
                return new MOVE_INFO();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="move"></param>
        public void PerformMove(MOVE move)
        {
            Debug.Assert(board[move.Squares[0]] != null);

            Piece moving = board[move.Squares[0]]!;
            Piece moved = board.PerformMove(move);

            foreach (Piece captured in move.CapturedPieces)
            {
                waiting_player.RemovePiece(captured);
            }

            player_on_move.RemovePiece(moving);
            player_on_move.AddPiece(moved);

            SwitchPlayers();

            if (player_on_move.GetPieces().Count == 0)
            {
                OnGameOver?.Invoke(waiting_player);
            }
            else if (player_on_move.IsAutomatic)
            {
                OnMove?.Invoke(null);
                PerformAutomaticMove();
            }
            else
            {
                OnMove?.Invoke(player_on_move);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void PerformAutomaticMove()
        {
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            player_on_move.FindNextMove(board, waiting_player)
                .ContinueWith(PerformAutomaticMove, scheduler);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="generated"></param>
        async void PerformAutomaticMove(Task<MOVE?> generated)
        {
            if (generated.Exception != null)
            {
                Debug.WriteLine(generated.Exception);
            }
            else if (generated.Result != null)
            {
                await Task.Delay(200);
                OnMark?.Invoke(generated.Result.Value);
                await Task.Delay(1000);
                PerformMove(generated.Result.Value);
            }
            else
            {
                OnGameOver?.Invoke(waiting_player);
            }
        }
    }
}
