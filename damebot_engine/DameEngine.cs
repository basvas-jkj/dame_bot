using System.Diagnostics;
using System.Threading.Tasks;

namespace damebot_engine
{
    /// <summary>
    /// Handler of the game engine Move event.
    /// </summary>
    /// <param name="next_player">
    /// reference to an instance of the player on move
    /// <c>null</c> if the compueter is on the move
    /// </param>
    public delegate void MoveEventHandler(IPlayer? next_player);
    /// <summary>
    /// Handler of the game engine Mark event.
    /// </summary>
    /// <param name="move">The move marked by computer.</param>
    public delegate void MarkEventHandler(MOVE move);
    /// <summary>
    /// Handler of the game engine GameOver event.
    /// </summary>
    /// <param name="piece">the winner of the game</param>
    public delegate void GameOverEventHandler(IPlayer piece);
    /// <summary>
    /// Common interface for a game engine.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Checks if the field identified by <paramref name="position"/>
        /// contains a piece which belongs to the player on the move.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the field a piece which belongs to the player on the move,
        /// <c>false</c> otherwise contains a piece
        /// </returns>
        bool IsOnMovePiece(SQUARE position);
        /// <summary>
        /// Checks if the <paramref name="move"/> extended by the <paramref name="next"/> of the <paramref name="piece"/> is valid.
        /// </summary>
        /// <returns>
        /// A <c>MOVE_INFO</c> instance of a valid move, if the move is valid,
        /// otherwise an empty instance of MOVE_INFO.
        /// </returns>
        MOVE_INFO ValidateMove(Piece piece, MOVE move, SQUARE next);
        /// <summary>
        /// Performs the <paramref name="move"/>, update players' pieces,
        /// switch players, invokes neccessery events and if the next player
        /// automatic, invokes his move generation.
        /// </summary>
        /// <param name="move">The move which will be performed.</param>
        void PerformMove(MOVE move);
        /// <summary>
        /// Generates next move for computer player and perfoms it.
        /// </summary>
        void PerformAutomaticMove();

        /// <summary>
        /// Event raised when any piece is moved.
        /// </summary>
        event MoveEventHandler? OnMove;
        /// <summary>
        /// Event raised when computer marks move which it wants to perform.
        /// </summary>
        event MarkEventHandler? OnMark;
        /// <summary>
        /// Event raised the player on the move run out of their possibilities ()including the case they have lost all of their pieces).
        /// </summary>
        event GameOverEventHandler? OnGameOver;
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

        /// <summary>
        /// Reference to the player who will perform their next move.
        /// </summary>
        private IPlayer player_on_move = white;
        /// <summary>
        /// Reference to the player who waits for their turn.
        /// </summary>
        private IPlayer waiting_player
        {
            get => (player_on_move == white) ? black : white;
        }
        /// <summary>
        /// Changes the player who is on their turn to the previously waiting player.
        /// </summary>
        private void SwitchPlayers()
        {
            player_on_move = (player_on_move == white) ? black : white;
        }

        /// <summary>
        /// Event raised when any piece is moved.
        /// </summary>
        public event MoveEventHandler? OnMove;
        /// <summary>
        /// Event raised when computer marks move which it wants to perform.
        /// </summary>
        public event MarkEventHandler? OnMark;
        /// <summary>
        /// Event raised the player on the move run out of their possibilities ()including the case they have lost all of their pieces).
        /// </summary>
        public event GameOverEventHandler? OnGameOver;

        /// <summary>
        /// Checks if the field identified by <paramref name="position"/>
        /// contains a piece which belongs to the player on the move.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the field a piece which belongs to the player on the move,
        /// <c>false</c> otherwise contains a piece
        /// </returns>
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
        /// Checks if the <paramref name="move"/> extended by the <paramref name="next"/> of the <paramref name="piece"/> is valid.
        /// </summary>
        /// <returns>
        /// A <c>MOVE_INFO</c> instance of a valid move, if the move is valid,
        /// otherwise an empty instance of MOVE_INFO.
        /// </returns>
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
        /// Performs the <paramref name="move"/>, update players' pieces,
        /// switch players, invokes neccessery events and if the next player
        /// automatic, invokes his move generation.
        /// </summary>
        /// <param name="move">The move which will be performed.</param>
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
        /// Generates next move for computer player and perfoms it.
        /// </summary>
        public void PerformAutomaticMove()
        {
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            player_on_move.FindNextMove(board, waiting_player)
                .ContinueWith(PerformAutomaticMove, scheduler);
        }
        /// <summary>
        /// Performs a move of computer player.
        /// </summary>
        /// <param name="generated">Asynchronously generated move.</param>
        async void PerformAutomaticMove(Task<MOVE?> generated)
        {
            await generated;
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
