using System.Collections.Generic;

namespace damebot_engine
{
	public interface IBoard
	{
		Piece? this[SQUARE position] { get; set; }

		int Size { get; }
		IReadOnlyList<Piece> GetPieces();
	}
	public class DefaultBoard: IBoard
	{
		private Piece?[,] board;
		public Piece? this[SQUARE position]
		{
			get => board[position.X, position.Y];
			set => board[position.X, position.Y] = value;
		}

		public int Size { get => 8; }

		IEnumerable<SQUARE> GenerateInitialPositions()
		{
			for (int f = 0; f < Size; f += 1)
			{
				yield return new SQUARE(f, f % 2);
				yield return new SQUARE(f, Size + f % 2 - 2);
			}
		}
		IEnumerable<Piece> GenerateInitialPieces()
		{
			foreach (SQUARE position in GenerateInitialPositions())
			{
				Piece piece = (position.Y < Size / 2)
					? new WhiteMan(position)
					: new BlackMan(position);

				this[position] = piece;
				yield return piece;
			}
		}

		List<Piece> pieces;
		public DefaultBoard()
		{
			board = new Piece[Size, Size];
			pieces = new List<Piece>(GenerateInitialPieces());
		}
		public IReadOnlyList<Piece> GetPieces()
		{
			return pieces;
		}
	}
}
