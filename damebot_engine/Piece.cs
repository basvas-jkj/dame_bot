using System;
using System.Drawing;

namespace damebot_engine
{
	using SQUARE_DIFF = (int X, int Y);

	public enum MOVE_TYPE
	{
		move, jump, invalid, incomplete_jump
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
		public abstract MOVE_TYPE GetMoveType(SQUARE original, SQUARE next);
		public abstract bool CanCapture(SQUARE original);
		public bool CanCapture()
		{
			return CanCapture(Position);
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
		public sealed override MOVE_TYPE GetMoveType(SQUARE original, SQUARE next)
		{
			SQUARE_DIFF difference = next - original;
			if ((difference == (1, Forward) || difference == (-1, Forward))
				&& board[next] == null)
			{
				return MOVE_TYPE.move;
			}
			else if ((difference == (2, DoubleForward) || difference == (-2, DoubleForward))
				&& IsJumpPossible(original, next))
			{
				return MOVE_TYPE.jump;
			}
			else
			{
				return MOVE_TYPE.invalid;
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
		public sealed override bool CanCapture(SQUARE original)
		{
			return CanCapture(original, (1, Forward)) || CanCapture(original, (-1, Forward));
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
		private bool CanCapture(SQUARE original, SQUARE_DIFF direction)
		{
			for (SQUARE s = original + direction; s.IsOnBoard(board); s += direction)
			{
				Piece? p = board[s];
				if (HasDifferentColour(p))
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
		public sealed override bool CanCapture(SQUARE original)
		{
			return CanCapture(original, (1, 1)) || CanCapture(original, (-1, -1))
				|| CanCapture(original, (1, -1)) || CanCapture(original, (-1, 1));
		}
		public override MOVE_TYPE GetMoveType(SQUARE original, SQUARE next)
		{
			SQUARE_DIFF difference = next - original;
			if (difference.X != difference.Y && difference.X != -difference.Y)
			{
				return MOVE_TYPE.invalid;
			}
			else if (board[next] != null)
			{
				return MOVE_TYPE.invalid;
			}

			bool met_enemy_piece = false;
			SQUARE_DIFF direction = difference.Normalise();
			for (SQUARE s = original + direction; s != next; s += direction)
			{
				Piece? p = board[s];
				if (p == null)
				{
					continue;
				}
				else if (HasDifferentColour(p))
				{
					if (met_enemy_piece)
					{
						return MOVE_TYPE.invalid;
					}
					else
					{
						met_enemy_piece = true;
					}
				}
				else
				{
					return MOVE_TYPE.invalid;
				}
			}

			return (met_enemy_piece) ? MOVE_TYPE.jump : MOVE_TYPE.move;
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
