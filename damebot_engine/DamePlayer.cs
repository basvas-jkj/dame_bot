using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace damebot_engine
{
    using EVALUATED_MOVE = (MOVE? move, int evaluation);
    using MOVE_TASK = (MOVE move, Task<int> evaluation_task);
    public enum PLAYER_TYPE { min, max }

    public interface IPlayer
    {
        bool IsAutomatic { get; }
        string Name { get; }

        void AddPiece(Piece piece);
        void RemovePiece(Piece piece);
        IReadOnlyList<Piece> GetPieces();
        IPlayer Copy();

        bool CanCapture(IBoard board);
        Task<MOVE?> FindNextMove(IBoard board, IPlayer other);
    }
    public class DamePlayer(bool automatic, PLAYER_TYPE type, string name): IPlayer
        public bool IsAutomatic { get; } = automatic;
        public string Name { get; } = name;

        readonly PLAYER_TYPE type = type;
        List<Piece> pieces = new();

        public void AddPiece(Piece piece)
        {
            pieces.Add(piece);
        }
        public void RemovePiece(Piece piece)
        {
            pieces.Remove(piece);
        }
        public IReadOnlyList<Piece> GetPieces()
        {
            return pieces;
        }

        public IPlayer Copy()
        {
            return new DamePlayer(IsAutomatic, type, Name)
            {
                pieces = new List<Piece>(pieces)
            };
        }

        const int MinInitialEvaluation = 999_999;
        const int MaxInitialEvaluation = -999_999;
        int InitialEvaluation
        {
            get => (type == PLAYER_TYPE.min) ? MinInitialEvaluation : MaxInitialEvaluation;
        }
        int SelectBetterEvaluation(int original_evaluation, int new_evaluation)
        {
            if (type == PLAYER_TYPE.min)
            {
                return Math.Min(original_evaluation, new_evaluation);
            }
            else
            {
                return Math.Max(original_evaluation, new_evaluation);
            }
        }
        IEnumerable<MOVE> EnumerateAllMoves(IBoard board)
        {
            foreach (Piece piece in pieces)
            {
                foreach (MOVE move in piece.EnumerateMoves(board, new MOVE(piece.Position)))
                {
                    yield return move;
                }
            }
        }
        IEnumerable<MOVE> EnumerateAllJumps(IBoard board)
        {
            foreach (Piece piece in pieces)
            {
                foreach (MOVE move in piece.EnumerateJumps(board, new MOVE(piece.Position)))
                {
                    yield return move;
                }
            }
        }
        public bool CanCapture(IBoard board)
        {
            foreach (Piece piece in pieces)
            {
                if (piece.CanCapture(board))
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<MOVE?> FindNextMove(IBoard board, IPlayer other)
        {
            //Debug.WriteLine("======================================");
            EVALUATED_MOVE evaluation = await FindNextMove(board, other, 1);
            return evaluation.move;
        }

        const int max_depth = 2;
        async Task<EVALUATED_MOVE> FindNextMove(IBoard board, IPlayer other, int depth)
        {
            List<MOVE_TASK> tasks = new();
            IEnumerable<MOVE> moves = CanCapture(board) ? EnumerateAllJumps(board) : EnumerateAllMoves(board);

            foreach (MOVE move in moves)
            {
                Task<int> future_evaluation = EvaluateMove(board, other, depth, move);
                tasks.Add((move, future_evaluation));
            }

            int best_evaluation = InitialEvaluation;
            List<MOVE> best_moves = new();
            foreach ((MOVE move, Task<int> evaluation_task) in tasks)
            {
                int new_evaluation = await evaluation_task;
                int better = SelectBetterEvaluation(best_evaluation, new_evaluation);

                if (better == best_evaluation && better == new_evaluation)
                {
                    // new evaluation is same as the original best evaluation
                    best_moves.Add(move);
                }
                else if (better == new_evaluation)
                {
                    // new evaluation is better then the original best evaluation
                    best_evaluation = new_evaluation;
                    best_moves = [move];
                    continue;
                }
                else
                {
                    // the original evaluation is still the best
                    continue;
                }
            }

            Random generator = new();
            int random = generator.Next(best_moves.Count);

            if (random < 0 || random >= best_moves.Count)
            {
                return (null, InitialEvaluation);
            }
            else
            {
                return (best_moves[random], best_evaluation);
            }
        }
        async Task<int> EvaluateMove(IBoard board, IPlayer other, int depth, MOVE move)
        {
            Piece moving = board[move.Squares[0]]!;
            (IBoard simulated, Piece moved) = board.SimulateMove(move);
            //Debug.WriteLine(depth);
            //Debug.WriteLine(simulated);

            if (depth == max_depth)
            {
                return simulated.EvaluatePosition();
            }
            else
            {
                DamePlayer me = (DamePlayer)Copy();
                me.RemovePiece(moving);
                me.AddPiece(moved);

                DamePlayer you = (DamePlayer)other.Copy();
                foreach (Piece captured in move.CapturedPieces)
                {
                    you.RemovePiece(captured);
                }

                EVALUATED_MOVE evaluated_move = await you.FindNextMove(simulated, me, depth + 1);
                return evaluated_move.evaluation;
            }
        }
    }
}
