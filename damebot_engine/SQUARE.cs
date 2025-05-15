using System;

namespace damebot_engine
{
	using SQUARE_DIFF = (int X, int Y);

	public static class SquareDiffExtension
	{
		public static SQUARE_DIFF Normalise(this SQUARE_DIFF diff)
		{
			return (Math.Sign(diff.X), Math.Sign(diff.Y));
		}
	}
	public readonly struct SQUARE(int x, int y)
	{
		public int X { get; } = x;
		public int Y { get; } = y;
		public bool IsOnBoard(IBoard board)
		{
			return 0 <= X && X < board.Size && 0 <= Y && Y < board.Size;
		}

		public static bool operator ==(SQUARE a, SQUARE b)
		{
			return a.X == b.X && a.Y == b.Y;
		}
		public static bool operator !=(SQUARE a, SQUARE b)
		{
			return !(a == b);
		}
		public static SQUARE_DIFF operator -(SQUARE a, SQUARE b)
		{
			return new SQUARE_DIFF(a.X - b.X, a.Y - b.Y);
		}
		public static SQUARE operator +(SQUARE a, SQUARE_DIFF b)
		{
			return new SQUARE(a.X + b.X, a.Y + b.Y);
		}
		public static SQUARE operator |(SQUARE a, SQUARE b)
		{
			int new_x = (a.X + b.X) / 2;
			int new_y = (a.Y + b.Y) / 2;
			return new SQUARE(new_x, new_y);
		}

		public override bool Equals(object? o)
		{
			return o != null && o.GetType() == GetType() && this == (SQUARE)o;
		}
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}
		public override string ToString()
		{
			return string.Format("[{0}, {1}]", X + 1, Y + 1);
		}
	}
}
