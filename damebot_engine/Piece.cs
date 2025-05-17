using System;
using System.Drawing;
using System.Linq;

namespace damebot_engine
{
	using SQUARE_DIFF = (int X, int Y);
	public struct MOVE_INFO
	{
		public bool IncompleteJump { get; } = false;
		public bool CompleteJump { get; } = false;
		public bool Move { get; } = false;

		public SQUARE Square { get; }
		public Piece? CapturedPiece { get; } = null;

		public MOVE_INFO(SQUARE s)
		{
			Move = true;
			Square = s;
		}
		public MOVE_INFO(SQUARE s, Piece p, bool is_complete)
		{
			if (is_complete)
			{
				CompleteJump = true;
			}
			else
			{
				IncompleteJump = true;
			}

			Square = s;
			CapturedPiece = p;
		}
	}

	public abstract class Piece(IBoard board, SQUARE position, Image image)
	{
		protected IBoard board = board;
		public Image image { get; } = image;
		public SQUARE Position { get; private set; } = position;

		public void Move(SQUARE next_position)
		{
			Position = next_position;
		}
		public abstract MOVE_INFO GetMoveInfo(MOVE m, SQUARE next);
		public abstract bool CanCapture(MOVE m);
		public bool CanCapture()
		{
			return CanCapture(new MOVE(Position));
		}
		public abstract bool CanBePromoted();
		public abstract Piece Promote();

		protected abstract bool HasDifferentColour(Piece? other);
	}

	abstract class ManBase(IBoard board, SQUARE position, Image image): Piece(board, position, image)
	{
		protected abstract int Forward { get; }
		protected abstract int DoubleForward { get; }

		bool IsJumpPossible(SQUARE original, SQUARE next)
		{
			Piece? p = board[original | next];
			return HasDifferentColour(p) && board[next] == null;
		}
		public sealed override MOVE_INFO GetMoveInfo(MOVE m, SQUARE next)
		{
			SQUARE_DIFF difference = next - m.LastSquare;
			if ((difference == (1, Forward) || difference == (-1, Forward))
				&& board[next] == null)
			{
				return new MOVE_INFO(next);
			}
			else if ((difference == (2, DoubleForward) || difference == (-2, DoubleForward))
				&& IsJumpPossible(m.LastSquare, next))
			{
				Piece p = board[m.LastSquare | next]!;
				MOVE simulated = new(m);
				simulated.AddJump(next, p);

				bool can_capture = CanCapture(simulated);
				return new MOVE_INFO(next, p, !can_capture);
			}
			else
			{
				return new MOVE_INFO();
			}
		}
		private bool CanCapture(SQUARE original, SQUARE_DIFF direction)
		{
			SQUARE obstacle = original + direction;
			SQUARE destination = obstacle + direction;

			if (obstacle.IsOnBoard(board) && destination.IsOnBoard(board))
			{
				return board[destination] == null && HasDifferentColour(board[obstacle]);
			}
			else
			{
				return false;
			}
		}
		public sealed override bool CanCapture(MOVE m)
		{
			return CanCapture(m.LastSquare, (1, Forward)) || CanCapture(m.LastSquare, (-1, Forward));
		}
	}
	class WhiteMan(IBoard board, SQUARE position): ManBase(board, position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/white_man.png");

		public override bool CanBePromoted()
		{
			return Position.Y == board.Size - 1;
		}
		public override Piece Promote()
		{
			return new WhiteKing(board, Position);
		}
		protected override int Forward { get => 1; }
		protected override int DoubleForward { get => 2 * Forward; }

		protected override bool HasDifferentColour(Piece? other)
		{
			return other is BlackMan || other is BlackKing;
		}
	}
	class BlackMan(IBoard board, SQUARE position): ManBase(board, position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/black_man.png");

		public override bool CanBePromoted()
		{
			return Position.Y == 0;
		}
		public override Piece Promote()
		{
			return new BlackKing(board, Position);
		}
		protected override int Forward { get => -1; }
		protected override int DoubleForward { get => 2 * Forward; }
		protected override bool HasDifferentColour(Piece? other)
		{
			return other is WhiteMan || other is WhiteKing;
		}
	}


	abstract class KingBase(IBoard board, SQUARE position, Image image): Piece(board, position, image)
	{
		private bool CanCapture(MOVE m, SQUARE_DIFF direction)
		{
			for (SQUARE s = m.LastSquare + direction; s.IsOnBoard(board); s += direction)
			{
				Piece? p = board[s];
				if (HasDifferentColour(p) && !m.CapturedPieces.Contains(p))
				{
					s += direction;
					return s.IsOnBoard(board) && board[s] == null;
				}
				else if (p != null)
				{
					return false;
				}
			}
			return false;
		}
		public sealed override bool CanCapture(MOVE m)
		{
			return CanCapture(m, (1, 1)) || CanCapture(m, (-1, -1))
				|| CanCapture(m, (1, -1)) || CanCapture(m, (-1, 1));
		}
		public override MOVE_INFO GetMoveInfo(MOVE m, SQUARE next)
		{
			SQUARE_DIFF difference = next - m.LastSquare;
			if (difference.X != difference.Y && difference.X != -difference.Y)
			{
				return new MOVE_INFO();
			}
			else if (board[next] != null)
			{
				return new MOVE_INFO();
			}

			Piece? jumped_enemy_piece = null;
			SQUARE_DIFF direction = difference.Normalise();
			for (SQUARE s = m.LastSquare + direction; s != next; s += direction)
			{
				Piece? p = board[s];
				if (p == null)
				{
					continue;
				}
				else if (HasDifferentColour(p))
				{
					if (jumped_enemy_piece != null)
					{
						return new MOVE_INFO();
					}
					else
					{
						jumped_enemy_piece = p;
					}
				}
				else
				{
					return new MOVE_INFO();
				}
			}

			if (jumped_enemy_piece == null)
			{
				return new MOVE_INFO(next);
			}
			else
			{
				MOVE simulated = new(m);
				simulated.AddJump(next, jumped_enemy_piece);

				bool can_capture = CanCapture(simulated);
				return new MOVE_INFO(next, jumped_enemy_piece, !can_capture);
			}
		}
		public override bool CanBePromoted()
		{
			return false;
		}
		public override Piece Promote()
		{
			throw new InvalidOperationException();
		}
	}
	class WhiteKing(IBoard board, SQUARE position): KingBase(board, position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/white_king.png");

		protected override bool HasDifferentColour(Piece? other)
		{
			return other is BlackMan || other is BlackKing;
		}
	}
	class BlackKing(IBoard board, SQUARE position): KingBase(board, position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/black_king.png");

		protected override bool HasDifferentColour(Piece? other)
		{
			return other is WhiteMan || other is WhiteKing;
		}
	}
}
