using System.Diagnostics;
using System.Threading.Tasks;

namespace damebot_engine
{
    public delegate void MoveEvent(IPlayer? next_player);
    public delegate void MarkEvent(MOVE m);
    public delegate void GameOverEvent(IPlayer p);
    public interface IEngine
    {
        bool IsOnMovePiece(SQUARE position);
        MOVE_INFO ValidateMove(Piece piece, MOVE m, SQUARE next);
        void PerformMove(MOVE m);
        void PerformAutomaticMove();

        event MoveEvent? OnMove;
        event MarkEvent? OnMark;
        event GameOverEvent? OnGameOver;
    }
    public class DameEngine(IBoard board, IPlayer white, IPlayer black): IEngine
    {
        public IBoard Board { get; } = board;
        public IPlayer White { get; } = white;
        public IPlayer Black { get; } = black;

        private IPlayer player_on_move = white;
        private IPlayer waiting_player
        {
            get => (player_on_move == White) ? Black : White;
        }
        private void SwitchPlayers()
        {
            player_on_move = (player_on_move == White) ? Black : White;
        }

        public event MoveEvent? OnMove;
        public event MarkEvent? OnMark;
        public event GameOverEvent? OnGameOver;

        public bool IsOnMovePiece(SQUARE position)
        {
            if (player_on_move == White)
            {
                return Board[position] is WhiteMan || Board[position] is WhiteKing;
            }
            else
            {
                return Board[position] is BlackMan || Board[position] is BlackKing;
            }
        }
        public MOVE_INFO ValidateMove(Piece piece, MOVE m, SQUARE next)
        {
            MOVE_INFO info = piece.GetMoveInfo(Board, m, next);

            if (info.CompleteJump || info.IncompleteJump)
            {
                return info;
            }
            if (info.Move && !player_on_move.CanCapture(Board))
            {
                return info;
            }
            else
            {
                return new MOVE_INFO();
            }
        }

        public void PerformMove(MOVE m)
        {
            Debug.Assert(Board[m.Squares[0]] != null);

            Piece moving = Board[m.Squares[0]]!;
            Piece moved = Board.PerformMove(m);

            foreach (Piece captured in m.CapturedPieces)
            {
                waiting_player.RemovePiece(captured);
                Board.RemovePiece(captured);
            }

            if (moved.CanBePromoted(Board))
            {
                Board.RemovePiece(moved);
                moved = moved.Promote();
                Board.AddPiece(moved);
            }

            player_on_move.RemovePiece(moving);
            player_on_move.AddPiece(moved);

            SwitchPlayers();

            if (player_on_move.GetPieces().Count == 0)
            {
                OnGameOver?.Invoke(waiting_player);
            }
            else if (player_on_move.Automatic)
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
            var v = TaskScheduler.FromCurrentSynchronizationContext();
            player_on_move.FindNextMove(Board, waiting_player)
                .ContinueWith(PerformAutomaticMove, v);
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
