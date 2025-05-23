using System;

namespace damebot_engine
{
    using SQUARE_DIFF = (int Column, int Row);
    public static class SquareDiffExtension
    {
        public static SQUARE_DIFF Normalise(this SQUARE_DIFF diff)
        {
            return (Math.Sign(diff.Column), Math.Sign(diff.Row));
        }
    }
    public readonly struct SQUARE(int column, int row)
    {
        public int Column { get; } = column;
        public int Row { get; } = row;
        public bool IsOnBoard(IBoard board)
        {
            return 0 <= Column && Column < board.Size && 0 <= Row && Row < board.Size;
        }

        public static bool operator ==(SQUARE a, SQUARE b)
        {
            return a.Column == b.Column && a.Row == b.Row;
        }
        public static bool operator !=(SQUARE a, SQUARE b)
        {
            return !(a == b);
        }
        public static SQUARE_DIFF operator -(SQUARE a, SQUARE b)
        {
            return new SQUARE_DIFF(a.Column - b.Column, a.Row - b.Row);
        }
        public static SQUARE operator +(SQUARE a, SQUARE_DIFF b)
        {
            return new SQUARE(a.Column + b.Column, a.Row + b.Row);
        }
        public static SQUARE operator |(SQUARE a, SQUARE b)
        {
            int new_x = (a.Column + b.Column) / 2;
            int new_y = (a.Row + b.Row) / 2;
            return new SQUARE(new_x, new_y);
        }

        public override bool Equals(object? o)
        {
            return o != null && o.GetType() == GetType() && this == (SQUARE)o;
        }
        public override int GetHashCode()
        {
            return Column.GetHashCode() ^ Row.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("[{0}, {1}]", Column + 1, Row + 1);
        }
    }
}
