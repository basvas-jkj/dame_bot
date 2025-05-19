using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace damebot_engine
{
	using SIMULATED_MOVE = (IBoard board, Piece moved);

	public interface IBoard
	{
		int Size { get; }
		Piece? this[SQUARE position] { get; }

		void GenerateInitialPieces(IPlayer white, IPlayer black);
		Piece PerformMove(MOVE m);
		SIMULATED_MOVE SimulateMove(MOVE m);
		int EvaluatePosition();

		void AddPiece(Piece added_piece);
		void RemovePiece(Piece removed_piece);
	}
	public class DefaultBoard: IBoard
	{
		private Piece?[,] board;
		public Piece? this[SQUARE position]
		{
			get => board[position.X, position.Y];
			private set => board[position.X, position.Y] = value;
		}

		public int Size { get => 8; }

		public DefaultBoard()
		{
			board = new Piece[Size, Size];
		}
		public DefaultBoard(DefaultBoard @default)
		{
			board = (Piece?[,])@default.board.Clone();
		}

		IEnumerable<SQUARE> GenerateInitialPositions()
		{
			for (int f = 0; f < Size; f += 2)
			{
				yield return new SQUARE(f, 0);
				yield return new SQUARE(f, 2);
				yield return new SQUARE(f, Size - 2);
			}
			for (int f = 1; f < Size; f += 2)
			{
				yield return new SQUARE(f, 1);
				yield return new SQUARE(f, Size - 1);
				yield return new SQUARE(f, Size - 3);
			}
		}
		public void GenerateInitialPieces(IPlayer white, IPlayer black)
		{
			foreach (SQUARE position in GenerateInitialPositions())
			{
				Piece piece;
				if (position.Y < Size / 2)
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
		public int EvaluatePosition()
		{
			int value = 0;
			foreach (Piece? p in board)
			{
				value += p?.Value ?? 0;
			}
			return value;
		}
		public Piece PerformMove(MOVE m)
		{
			IReadOnlyList<SQUARE> squares = m.Squares;
			SQUARE original = squares[0];
			SQUARE next = squares[^1];

			Piece moving = this[original]!;
			Piece moved = moving.Move(next);

			this[original] = null;
			this[next] = moved;

			return moved;
		}
		public SIMULATED_MOVE SimulateMove(MOVE m)
		{
			DefaultBoard simulated = new(this);
			Piece moved = simulated.PerformMove(m);
			return (simulated, moved);
		}

		public void AddPiece(Piece added_piece)
		{
			Debug.Assert(this[added_piece.Position] == null);
			this[added_piece.Position] = added_piece;
		}
		public void RemovePiece(Piece removed_piece)
		{
			this[removed_piece.Position] = null;
		}

		public override string ToString()
		{
			StringBuilder sb = new("----------\n");

			for (int fa = Size - 1; fa >= 0; fa -= 1)
			{
				sb.Append("|");
				for (int fb = 0; fb < Size; fb += 1)
				{
					sb.Append(board[fb, fa]?.ToString() ?? " ");
				}
				sb.AppendLine("|");
			}

			sb.AppendLine("----------");
			return sb.ToString();
		}
	}
}
