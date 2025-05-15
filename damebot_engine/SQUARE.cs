namespace damebot_engine
{
	public readonly struct SQUARE(int x, int y)
	{
		public int X { get; } = x;
		public int Y { get; } = y;
		public bool IsOnBoard(IBoard board)
		{
			return 0 <= X && X < board.Size && 0 <= Y && Y < board.Size;
		}

		public static (int x, int y) operator -(SQUARE a, SQUARE b)
		{
			return (a.X - b.X, a.Y - b.Y);
		}
		public static SQUARE operator +(SQUARE a, (int x, int y) b)
		{
			return new SQUARE(a.X + b.x, a.Y + b.y);
		}
	}
}
