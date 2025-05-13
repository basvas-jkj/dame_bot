using System.Collections.Generic;

namespace damebot_engine
{
	public record class Square(int X, int Y);
	public delegate void MoveEvent();

	public interface IBoard
	{
		public int Size { get; }
		public IReadOnlyList<Piece> GetPieces();
	}
	public class DefaultBoard: IBoard
	{
		public int Size => 8;

		IEnumerable<Square> GenerateInitialPositions()
		{
			for (int f = 0; f < Size; f += 1)
			{
				yield return new Square(f, f % 2);
				yield return new Square(f, Size + f % 2 - 2);
			}
		}
		IEnumerable<Piece> GenerateInitialPieces()
		{
			foreach (Square position in GenerateInitialPositions())
			{
				if (position.Y < Size / 2)
				{
					yield return new WhiteMan(position);
				}
				else
				{
					yield return new BlackMan(position);
				}
			}
		}

		List<Piece> pieces;
		public DefaultBoard()
		{
			pieces = new List<Piece>(GenerateInitialPieces());
		}
		public IReadOnlyList<Piece> GetPieces()
		{
			return pieces;
		}
	}
}
