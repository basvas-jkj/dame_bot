using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace damebot_engine
{
    /// <summary>
    /// Alias for a tuple of a game board simulated position and a piece moved in the simulated move.
    /// </summary>
    using SIMULATED_MOVE = (IBoard board, Piece moved);
    /// <summary>
    /// Common interface for a game board.
    /// </summary>
    public interface IBoard
    {
        /// <summary>
        /// Count of fields on the edge of the board. 
        /// </summary>
        int Size { get; }
        /// <summary>
        /// Returns piece which occupies the given field.
        /// </summary>
        /// <param name="position">Identifies the requested field.</param>
        /// <returns>
        /// <c>Piece</c> which  the given field,
        /// <c>null</c> if the given field is empty.
        /// </returns>
        Piece? this[SQUARE position] { get; }

        /// <summary>
        /// Creates all initial pieces, places them on the board and assigns them to corresponding players.
        /// </summary>
        /// <param name="white">player who gets white pieces</param>
        /// <param name="black">player who gets balck pieces</param>
        void GenerateInitialPieces(IPlayer white, IPlayer black);
        /// <summary>
        /// Moves affected piece and removes captured pieces from this bouard.
        /// </summary>
        /// <param name="move">Move which will be performed.</param>
        /// <returns>Moved and potentially promoted version of piece.</returns>
        Piece PerformMove(MOVE move);
        /// <summary>
        /// Creates a copy of this board and performs the given move on it.
        /// </summary>
        /// <param name="move">simulated move</param>
        /// <returns>A tuple of board copy which the move was performed on and moved version of the affected piece.</returns>
        SIMULATED_MOVE SimulateMove(MOVE move);
        /// <summary>
        /// Computes a sum of values of all pieces placed on this board.
        /// </summary>
        /// <returns>
        /// sum of values of all pieces
        /// </returns>
        int EvaluatePosition();
    }
    /// <summary>
    /// Represents a dame game board.
    /// </summary>
    public class DameBoard: IBoard
    {
        /// <summary>
        /// Stores the full board situation.
        /// </summary>
        private Piece?[,] board;
        /// <summary>
        /// Returns piece which occupies the given field.
        /// </summary>
        /// <param name="position">Identifies the requested field.</param>
        /// <returns>
        /// <c>Piece</c> which  the given field,
        /// <c>null</c> if the given field is empty.
        /// </returns>
        public Piece? this[SQUARE position]
        {
            get => board[position.Column, position.Row];
            private set => board[position.Column, position.Row] = value;
        }

        /// <summary>
        /// Count of fields on the edge of the board. 
        /// </summary>
        public int Size { get => 8; }

        /// <summary>
        /// 
        /// </summary>
        public DameBoard()
        {
            board = new Piece[Size, Size];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        public DameBoard(DameBoard board)
        {
            this.board = (Piece?[,])board.board.Clone();
        }

        /// <summary>
        /// Enumerate all squares occupied by pieces on the initial position.
        /// </summary>
        /// <returns>Enumeration of all squares initially occupied by pieces.</returns>
        IEnumerable<SQUARE> GenerateInitialPositions()
        {
            for (int column = 0; column < Size; column += 2)
            {
                yield return new SQUARE(column, 0);
                yield return new SQUARE(column, 2);
                yield return new SQUARE(column, Size - 2);
            }
            for (int column = 1; column < Size; column += 2)
            {
                yield return new SQUARE(column, 1);
                yield return new SQUARE(column, Size - 1);
                yield return new SQUARE(column, Size - 3);
            }
        }
        /// <summary>
        /// Creates all initial pieces, places them on the board and assigns them to corresponding players.
        /// </summary>
        /// <param name="white">player who gets white pieces</param>
        /// <param name="black">player who gets balck pieces</param>
        public void GenerateInitialPieces(IPlayer white, IPlayer black)
        {
            foreach (SQUARE position in GenerateInitialPositions())
            {
                Piece piece;
                if (position.Row < Size / 2)
                {
                    piece = new WhiteMan(position);
                    white.AddPiece(piece);
                }
                else
                {
                    piece = new BlackMan(position);
                    black.AddPiece(piece);
                }

                this[position] = piece;
            }
        }
        /// <summary>
        /// Computes a sum of values of all pieces placed on this board.
        /// </summary>
        /// <returns>
        /// sum of values of all pieces
        /// </returns>
        public int EvaluatePosition()
        {
            int value = 0;
            foreach (Piece? piece in board)
            {
                value += piece?.Value ?? 0;
            }
            return value;
        }
        /// <summary>
        /// Moves affected piece and removes captured pieces from this bouard.
        /// </summary>
        /// <param name="move">Move which will be performed.</param>
        /// <returns>Moved and potentially promoted version of piece.</returns>
        public Piece PerformMove(MOVE move)
        {
            IReadOnlyList<SQUARE> squares = move.Squares;
            SQUARE original = squares[0];
            SQUARE next = squares[^1];

            Piece moving = this[original]!;
            Piece moved = moving.Move(next);

            this[original] = null;
            this[next] = moved;

            foreach (Piece captured in move.CapturedPieces)
            {
                RemovePiece(captured);
            }

            if (moved.CanBePromoted(this))
            {
                RemovePiece(moved);
                moved = moved.Promote();
                AddPiece(moved);
            }

            return moved;
        }
        /// <summary>
        /// Creates a copy of this board and performs the given move on it.
        /// </summary>
        /// <param name="move">simulated move</param>
        /// <returns>A tuple of board copy which the move was performed on and moved version of the affected piece.</returns>
        public SIMULATED_MOVE SimulateMove(MOVE move)
        {
            DameBoard simulated = new(this);
            Piece moved = simulated.PerformMove(move);
            return (simulated, moved);
        }

        /// <summary>
        /// Place the given <paramref name="added_piece"/> on this board.
        /// </summary>
        /// <param name="added_piece">A piece added on this board.</param>
        public void AddPiece(Piece added_piece)
        {
            Debug.Assert(this[added_piece.Position] == null);
            this[added_piece.Position] = added_piece;
        }
        /// <summary>
        /// Reomves the given <paramref name="removed_piece"/>> from this board.
        /// </summary>
        /// <param name="removed_piece">A piece removed from this board.</param>
        public void RemovePiece(Piece removed_piece)
        {
            this[removed_piece.Position] = null;
        }

        /// <summary>
        /// Converts this <c>DefaultBoard</c> to its string representation.
        /// </summary>
        /// <returns>String representation of this <c>DefaultBoard</c>.</returns>
        public override string ToString()
        {
            StringBuilder builder = new("----------\n");

            for (int row = Size - 1; row >= 0; row -= 1)
            {
                builder.Append("|");
                for (int column = 0; column < Size; column += 1)
                {
                    builder.Append(board[column, row]?.ToString() ?? " ");
                }
                builder.AppendLine("|");
            }

            builder.AppendLine("----------");
            return builder.ToString();
        }
    }
}
