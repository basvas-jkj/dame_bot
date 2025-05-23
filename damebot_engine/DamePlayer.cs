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
        bool Automatic { get; }
        string Name { get; }

        void AddPiece(Piece p);
        void RemovePiece(Piece p);
        IPlayer Copy();

        bool CanCapture(IBoard board);
        Task<MOVE?> FindNextMove(IBoard board, IPlayer other);
        IReadOnlyList<Piece> GetPieces();
    }
    {
        public bool Automatic { get; } = automatic;
    public class DamePlayer(bool automatic, PLAYER_TYPE type, string name): IPlayer
        public string Name { get; } = name;

        readonly PLAYER_TYPE type = type;
        List<Piece> pieces = new();

        public IReadOnlyList<Piece> GetPieces()
        {
            return pieces;
        }
        public void AddPiece(Piece p)
        {
            pieces.Add(p);
        }
        public void RemovePiece(Piece p)
        {
            pieces.Remove(p);
        }

        public IPlayer Copy()
        {
            return new Player(Automatic, type, Name)
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
            foreach (Piece p in pieces)
            {
                foreach (MOVE m in p.EnumerateMoves(board, new MOVE(p.Position)))
                {
                    yield return m;
                }
            }
        }
        IEnumerable<MOVE> EnumerateAllJumps(IBoard board)
        {
            foreach (Piece p in pieces)
            {
                foreach (MOVE m in p.EnumerateJumps(board, new MOVE(p.Position)))
                {
                    yield return m;
                }
            }
        }
        public bool CanCapture(IBoard board)
        {
            foreach (Piece p in pieces)
            {
                if (p.CanCapture(board))
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
        async public Task<EVALUATED_MOVE> FindNextMove(IBoard board, IPlayer other, int depth)
        {
            List<MOVE_TASK> tasks = new();
            IEnumerable<MOVE> moves = CanCapture(board) ? EnumerateAllJumps(board) : EnumerateAllMoves(board);

            foreach (MOVE m in moves)
            {
                Task<int> t = EvaluateMove(board, other, depth, m);
                tasks.Add((m, t));
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
        async Task<int> EvaluateMove(IBoard board, IPlayer other, int depth, MOVE m)
        {
            Piece moving = board[m.Squares[0]]!;
            (IBoard simulated, Piece moved) = board.SimulateMove(m);
            //Debug.WriteLine(depth);
            //Debug.WriteLine(simulated);

            if (depth == max_depth)
            {
                return simulated.EvaluatePosition();
            }
            else
            {
                Player me = (Player)Copy();
                me.RemovePiece(moving);
                me.AddPiece(moved);

                Player you = (Player)other.Copy();
                foreach (Piece captured in m.CapturedPieces)
                {
                    you.RemovePiece(captured);
                }

                EVALUATED_MOVE move = await you.FindNextMove(simulated, me, depth + 1);
                return move.evaluation;
            }
        }
    }
}
