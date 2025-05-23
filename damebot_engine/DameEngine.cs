using System.Diagnostics;
using System.Threading.Tasks;

namespace damebot_engine
{
    public delegate void MoveEvent(IPlayer? next_player);
    public delegate void MarkEvent(MOVE move);
    public delegate void GameOverEvent(IPlayer piece);
    public interface IEngine
    {
        bool IsOnMovePiece(SQUARE position);
        MOVE_INFO ValidateMove(Piece piece, MOVE move, SQUARE next);
        void PerformMove(MOVE move);
        void PerformAutomaticMove();

        event MoveEvent? OnMove;
        event MarkEvent? OnMark;
        event GameOverEvent? OnGameOver;
    }

    public class DameEngine(IBoard board, IPlayer white, IPlayer black): IEngine
    {
        IBoard board = board;
        IPlayer white = white;
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

        public event MoveEvent? OnMove;
        public event MarkEvent? OnMark;
        public event GameOverEvent? OnGameOver;

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
                OnMark?.Invoke(new MOVE());
            }
        }

        public void PerformAutomaticMove()
        {
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            player_on_move.FindNextMove(board, waiting_player)
                .ContinueWith(PerformAutomaticMove, scheduler);
        }
        void PerformAutomaticMove(Task<MOVE?> generated)
        {
            if (generated.Exception != null)
            {
                Debug.WriteLine(generated.Exception);
            }
            else if (generated.Result != null)
            {
                PerformMove(generated.Result.Value);
                OnMark?.Invoke(generated.Result.Value);
            }
            else
            {
                OnGameOver?.Invoke(waiting_player);
            }
        }
    }
}
