using System;

namespace damebot_engine
{
    /// <summary>
    /// Alias for square difference.
    /// </summary>
    using SQUARE_DIFF = (int Column, int Row);
    /// <summary>
    /// Extension methods for SQUARE_DIFF tuples.
    /// </summary>
    public static class SquareDiffExtension
    {
        /// <summary>
        /// Truncates both coordinates of the <paramref name="diff"/> to the range [-1, 1].
        /// </summary>
        /// <param name="diff">The SQUARE_DIFF tuple to be truncated.</param>
        /// <returns>Truncated version of <paramref name="diff"/></returns>
        public static SQUARE_DIFF Normalise(this SQUARE_DIFF diff)
        {
            return (Math.Sign(diff.Column), Math.Sign(diff.Row));
        }
        /// <summary>
        /// Switches the sign of both coordinates f the <paramref name="diff"/>.
        /// </summary>
        /// <param name="diff">The SQUARE_DIFF tuple to be inversed.</param>
        /// <returns>Inversed version of <paramref name="diff"/></returns>
        public static SQUARE_DIFF Inverse(this SQUARE_DIFF diff)
        {
            return (-diff.Column, -diff.Row);
        }
    }

    /// <summary>
    /// Represents one field of a game board.
    /// </summary>
    /// <param name="column">column number (zero-based)</param>
    /// <param name="row">row number (zero-based)</param>
    public readonly struct SQUARE(int column, int row)
    {
        /// <summary>
        /// Column number of this square.
        /// </summary>
        public int Column { get; } = column;
        /// <summary>
        /// Row number of this square.
        /// </summary>
        public int Row { get; } = row;

        /// <summary>
        /// Checks if this SQUARE is a valid field on the <paramref name="board"/>.
        /// </summary>
        /// <param name="board">Any IBoard instance.</param>
        /// <returns>
        /// <c>true</c> if both coordinates are in range supported by the <paramref name="board"/>
        /// <c>false</c> otherwise
        /// </returns>
        public bool IsOnBoard(IBoard board)
        {
            return 0 <= Column && Column < board.Size && 0 <= Row && Row < board.Size;
        }

        /// <summary>
        /// Compares two instances of SQUARE.
        /// </summary>
        /// <param name="a">any instance of SQUARE</param>
        /// <param name="b">any instance of SQUARE</param>
        /// <returns>
        /// <c>true</c> if both <paramref name="a"/> and <paramref name="b"/> represents
        /// same field of the game board.
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(SQUARE a, SQUARE b)
        {
            return a.Column == b.Column && a.Row == b.Row;
        }
        /// <summary>
        /// Compares two instances of SQUARE.
        /// </summary>
        /// <param name="a">any instance of SQUARE</param>
        /// <param name="b">any instance of SQUARE</param>
        /// <returns>
        /// <c>true</c> if both <paramref name="a"/> and <paramref name="b"/> doesn't represent
        /// same field of the game board.
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(SQUARE a, SQUARE b)
        {
            return !(a == b);
        }
        /// <summary>
        /// Computes the difference of two SQUARE instances.
        /// </summary>
        /// <param name="a">Any instance of SQUARE.</param>
        /// <param name="b">Any instance of SQUARE.</param>
        /// <returns>
        /// The difference of both cooardinates of <paramref name="a"/> and <paramref name="b"/>
        /// as a SQUARE_DIFF tuple.
        /// </returns>
        public static SQUARE_DIFF operator -(SQUARE a, SQUARE b)
        {
            return new SQUARE_DIFF(a.Column - b.Column, a.Row - b.Row);
        }
        /// <summary>
        /// Computes the sum of an SQUARE instance and SQUARE_DIFF tuple.
        /// </summary>
        /// <param name="a">Any instance of SQUARE.</param>
        /// <param name="b">Any SQUARE_DIFF tuple.</param>
        /// <returns>
        /// The sum of both cooardinates of <paramref name="a"/> and <paramref name="b"/>
        /// as a SQUARE instance.
        /// </returns>
        public static SQUARE operator +(SQUARE a, SQUARE_DIFF b)
        {
            return new SQUARE(a.Column + b.Column, a.Row + b.Row);
        }
        /// <summary>
        /// Computes the arithmetic average of two SQUARE instances.
        /// </summary>
        /// <param name="a">Any instance of SQUARE.</param>
        /// <param name="b">Any instance of SQUARE.</param>
        /// <returns>
        /// The arithmetic average of both coordinates of <paramref name="a"/> and <paramref name="b"/>
        /// as a SQUARE instance using integer arithmetics.
        /// </returns>
        public static SQUARE operator |(SQUARE a, SQUARE b)
        {
            int new_x = (a.Column + b.Column) / 2;
            int new_y = (a.Row + b.Row) / 2;
            return new SQUARE(new_x, new_y);
        }

        /// <summary>
        /// Checks if the <paramref name="o"/> represents the same field as this SQUARE.
        /// </summary>
        /// <param name="o">Any object to be compared with this SQUARE.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="o"/> represents the same field as this SQUARE.
        /// <c>false</c> otherwise (including the case that <paramref name="o"/> is not the instance of SQUARE).
        /// </returns>
        public override bool Equals(object? o)
        {
            return o != null && o.GetType() == GetType() && this == (SQUARE)o;
        }
        /// <summary>
        /// Computes hash code of this SQUARE.
        /// </summary>
        /// <returns>Hash code of this SQUARE.</returns>
        public override int GetHashCode()
        {
            return Column.GetHashCode() ^ Row.GetHashCode();
        }
        /// <summary>
        /// Converts this SQUARE to its string representation.
        /// </summary>
        /// <returns>String representation of this SQUARE.</returns>
        public override string ToString()
        {
            return string.Format("[{0}, {1}]", Column + 1, Row + 1);
        }
    }
}
