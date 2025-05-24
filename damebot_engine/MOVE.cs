using System;
using System.Collections.Generic;
using System.Text;

namespace damebot_engine
{
    /// <summary>
    /// Represents a move of one piece.
    /// </summary>
    public readonly struct MOVE
    {
        /// <summary>
        /// Stores all squares the moved piece passed through (including the initial and target square). 
        /// </summary>
        readonly List<SQUARE> squares;
        /// <summary>
        /// Stores all pieces captured by this move.
        /// </summary>
        readonly List<Piece> captered_pieces;

        /// <summary>
        /// Returns the target square of this move.
        /// </summary>
        public SQUARE LastSquare
        {
            get => squares[^1];
        }
        /// <summary>
        /// Readonly list of all squares the moved piece passed through (including the initial and target square). 
        /// </summary>
        public IReadOnlyList<SQUARE> Squares
        {
            get => squares;
        }
        /// <summary>
        /// REadonly list of all pieces captured by this move.
        /// </summary>
        public IReadOnlyList<Piece> CapturedPieces
        {
            get => captered_pieces;
        }

        /// <summary>
        /// Adds a target square to this move.
        /// </summary>
        /// <param name="next">Target square of this move.</param>
        /// <exception cref="InvalidOperationException">This move already contains more than one square.</exception>
        public void AddMove(SQUARE next)
        {
            if (squares.Count > 1)
                throw new InvalidOperationException();

            squares.Add(next);
        }
        /// <summary>
        /// Adds a target square of next jump to this move.
        /// </summary>
        /// <param name="next">Target square of the next jump.</param>
        /// <param name="captured">A piece captured in this jump.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddJump(SQUARE next, Piece captured)
        {
            if (squares.Count > 1 && captered_pieces.Count == 0)
                throw new InvalidOperationException();

            squares.Add(next);
            captered_pieces.Add(captured);
        }

        /// <summary>
        /// Initialise a move from its initial square.
        /// </summary>
        /// <param name="original">Intial square of he move.</param>
        public MOVE(SQUARE original)
        {
            squares = new() { original };
            captered_pieces = new();
        }
        /// <summary>
        /// Create an effective deep copy of the move.
        /// </summary>
        /// <param name="other_move">The move which will be copied.</param>
        public MOVE(MOVE other_move)
        {
            squares = new List<SQUARE>(other_move.squares);
            captered_pieces = new List<Piece>(other_move.captered_pieces);
        }

        /// <summary>
        /// Converts this MOVE to its string reprezentation. 
        /// </summary>
        /// <returns>String representation of this MOVE.</returns>
        public override string ToString()
        {
            StringBuilder builder = new(captered_pieces.Count == 0 ? "move:" : "jump:");
            foreach (SQUARE square in squares)
            {
                builder.Append(' ');
                builder.Append(square);
            }
            return builder.ToString();
        }
    }
}
