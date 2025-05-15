using System.Collections.Generic;

namespace damebot_engine
{
	public interface IBoard
	{
		int Size { get; }
		Piece? this[SQUARE position] { get; set; }

		void GenerateInitialPieces(Player white, Player black);
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

		public DefaultBoard()
		{
			board = new Piece[Size, Size];
		}

		IEnumerable<SQUARE> GenerateInitialPositions()
		{
			for (int f = 0; f < Size; f += 1)
			{
				yield return new SQUARE(f, f % 2);
				yield return new SQUARE(f, Size + f % 2 - 2);
			}
		}
		public void GenerateInitialPieces(Player white, Player black)
		{
			foreach (SQUARE position in GenerateInitialPositions())
			{
				Piece piece;
				if (position.Y < Size / 2)
				{
					piece = new WhiteMan(this, position);
					white.AddPiece(piece);
				}
				else
				{
					piece = new BlackMan(this, position);
					black.AddPiece(piece);
				}

				this[position] = piece;
			}
		}
	}
}
