using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace damebot_engine
{
    /// <summary>
    /// Alias for a tuple of move and its evaluation.
    /// </summary>
    using EVALUATED_MOVE = (MOVE? move, int evaluation);
    /// <summary>
    /// Alias for a tuple of move and its future evaluation.
    /// </summary>
    using MOVE_TASK = (MOVE move, Task<int> evaluation_task);
    /// <summary>
    /// Represents whether player chooses the move with the best or worst evaluation
    /// </summary>
    /// <value>
    /// <c>min</c> for player choosing worst evaluation,
    /// <c>max</c> for player choosing best evaluation.
    /// </value>
    public enum PLAYER_TYPE { min, max }

    /// <summary>
    /// Common interface for a player.
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// Is the player represented by a computer?
        /// </summary>
        bool IsAutomatic { get; }
        /// <summary>
        /// Name of the player (such as „bílý“, „black“, „rot“ or „French“ - depending on the game).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Adds a piece to the player's inventory.
        /// </summary>
        /// <param name="piece">added piece</param>
        void AddPiece(Piece piece);
        /// <summary>
        /// Removes a piece of the player's inventory.
        /// </summary>
        /// <param name="piece">removed piece</param>
        void RemovePiece(Piece piece);
        /// <summary>
        /// Returns a list of all this player's pieces.
        /// </summary>
        /// <returns>readonly list of all this player's pieces</returns>
        IReadOnlyList<Piece> GetPieces();
        /// <summary>
        /// Creates an effective deep copy of this player.
        /// </summary>
        /// <returns>effective deep copy of this player</returns>
        IPlayer Copy();

        /// <summary>
        /// Checks if this player can capture any enemy piece.
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <returns>
        /// <c>true</c> if this player can capture any enemy piece,
        /// <c>false</c> otherwise
        /// </returns>
        bool CanCapture(IBoard board);
        /// <summary>
        /// Finds the most suitable move for this player on the given <paramref name="board"/>. 
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <param name="other">The enemy player.</param>
        /// <returns>future of the most suitable move</returns>
        Task<MOVE?> FindNextMove(IBoard board, IPlayer other);
    }
    /// <summary>
    /// Represents a dame player.
    /// </summary>
    /// <param name="automatic">Is the player represented by a computer?</param>
    /// <param name="type">type of the player (<c>min</c> or <c>max</c>)</param>
    /// <param name="name">name of the player</param>
    public class DamePlayer(bool automatic, PLAYER_TYPE type, string name): IPlayer
    {
        /// <summary>
        /// Is the player represented by a computer?
        /// </summary>
        public bool IsAutomatic { get; } = automatic;
        /// <summary>
        /// Name of the player.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Type of the player (<c>min</c> or <c>max</c>).
        /// </summary>
        readonly PLAYER_TYPE type = type;
        /// <summary>
        /// Inventory of player's pieces.
        /// </summary>
        List<Piece> pieces = new();

        /// <summary>
        /// Adds a piece to the player's inventory.
        /// </summary>
        /// <param name="piece">added piece</param>
        public void AddPiece(Piece piece)
        {
            pieces.Add(piece);
        }
        /// <summary>
        /// Removes a piece of the player's inventory.
        /// </summary>
        /// <param name="piece">removed piece</param>
        public void RemovePiece(Piece piece)
        {
            pieces.Remove(piece);
        }
        /// <summary>
        /// Returns a list of all this player's pieces.
        /// </summary>
        /// <returns>readonly list of all this player's pieces</returns>
        public IReadOnlyList<Piece> GetPieces()
        {
            return pieces;
        }

        /// <summary>
        /// Creates an effective deep copy of this player.
        /// </summary>
        /// <returns>effective deep copy of this player</returns>
        public IPlayer Copy()
        {
            return new DamePlayer(IsAutomatic, type, Name)
            {
                pieces = new List<Piece>(pieces)
            };
        }

        /// <summary>
        /// Maximum supported evaluation - represents evaluation of invalid move of <c>min</c> player.
        /// </summary>
        const int MinInitialEvaluation = 999_999;
        /// <summary>
        /// Minimum supported evaluation - represents evaluation of invalid move of <c>max</c> player.
        /// </summary>
        const int MaxInitialEvaluation = -999_999;
        /// <summary>
        /// Selects <c>MinInitialEvaluation</c> or <c>MaxInitialEvaluation</c> depending on the player's type.
        /// </summary>
        int InitialEvaluation
        {
            get => (type == PLAYER_TYPE.min) ? MinInitialEvaluation : MaxInitialEvaluation;
        }

        /// <summary>
        /// Selects the better evaluatio from this player's perspective.
        /// /// </summary>
        /// <param name="original_evaluation">original best evaluation</param>
        /// <param name="new_evaluation">newly computed evaluation</param>
        /// <returns>
        /// <paramref name="original_evaluation"/> if it's same or better than <paramref name="new_evaluation"/>,
        /// <paramref name="new_evaluation"/> otherwise.
        /// </returns>
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
        /// <summary>
        /// Enumerates all moves that this player can perform on the given <paramref name="board"/>.
        /// </summary>
        /// <param name="board">The game board on which the moves are enumerated.</param>
        /// <returns>IEnumerable of all possible moves.</returns>
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
        /// <summary>
        /// Enumerates all jumps that this player can perform on the given <paramref name="board"/>.
        /// </summary>
        /// <param name="board">The game board on which the jumps are enumerated.</param>
        /// <returns>IEnumerable of all possible jumps.</returns>
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

        /// <summary>
        /// Checks if this player can capture any enemy piece.
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <returns>
        /// <c>true</c> if this player can capture any enemy piece,
        /// <c>false</c> otherwise
        /// </returns>
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
        /// <summary>
        /// Finds the most suitable move for this player on the given <paramref name="board"/>. 
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <param name="other">The enemy player.</param>
        /// <returns>future of the most suitable move</returns>
        public async Task<MOVE?> FindNextMove(IBoard board, IPlayer other)
        {
            EVALUATED_MOVE evaluation = await FindNextMove(board, other, 1);
            Debug.WriteLine("({0}) {1}", evaluation.evaluation, evaluation.move);

            return evaluation.move;
        }

        /// <summary>
        /// Maximum depth used to in minimax algorithm.
        /// </summary>
        const int max_depth = 6;
        /// <summary>
        /// Finds the most suitable move for this player on the given <paramref name="board"/> with respect to the current <paramref name="depth"/>. 
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <param name="other">The enemy player.</param>
        /// <param name="depth">Current depth of the minimax algorithm.</param>
        /// <returns>future of the most suitable move</returns>
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
            foreach ((MOVE move, Task<int> future_evaluation) in tasks)
            {
                int new_evaluation = await future_evaluation;
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
        /// <summary>
        /// Computes the evaluation on the given move <paramref name="move"/> with respect to the <paramref name="board"/> situation and current <paramref name="depth"/>.
        /// </summary>
        /// <param name="board">The board on which the evaluation take place.</param>
        /// <param name="other">The enemy player.</param>
        /// <param name="depth">Current depth of the minimax algorithm.</param>
        /// <param name="move">The move wich will be evaluated.</param>
        /// <returns>Evaluation of the move <paramref name="move"/>.</returns>
        async Task<int> EvaluateMove(IBoard board, IPlayer other, int depth, MOVE move)
        {
            Piece moving = board[move.Squares[0]]!;
            (IBoard simulated, Piece moved) = board.SimulateMove(move);

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
