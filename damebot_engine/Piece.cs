using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace damebot_engine
{
	using SQUARE_DIFF = (int X, int Y);
	public readonly struct MOVE_INFO
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

	public abstract record class Piece(IBoard board, SQUARE Position, Image Image)
	{
		protected readonly IBoard board = board;
		public Image Image { get; } = Image;
		public SQUARE Position { get; private init; } = Position;
		public abstract int Value { get; }

		public Piece Move(SQUARE next_position)
		{
			return this with { Position = next_position };
		}
		public abstract MOVE_INFO GetMoveInfo(MOVE m, SQUARE next);
		public abstract bool CanCapture(MOVE m);
		public bool CanCapture()
		{
			return CanCapture(new MOVE(Position));
		}
		public abstract bool CanBePromoted();
		public abstract Piece Promote();
		public abstract IEnumerable<MOVE> EnumerateMoves(MOVE m);
		public abstract IEnumerable<MOVE> EnumerateJumps(MOVE m);

		protected abstract bool HasDifferentColour(Piece? other);
	}

	abstract record class ManBase(IBoard board, SQUARE position, Image image): Piece(board, position, image)
	{
		protected abstract int Forward { get; }
		protected abstract int DoubleForward { get; }

		bool IsJumpPossible(SQUARE original, SQUARE next)
		{
			if (!next.IsOnBoard(board))
				return false;

			Piece? p = board[original | next];
			return HasDifferentColour(p) && board[next] == null;
		}
		public sealed override MOVE_INFO GetMoveInfo(MOVE m, SQUARE next)
		{
			SQUARE_DIFF difference = next - m.LastSquare;
			if ((difference == (1, Forward) || difference == (-1, Forward))
				&& next.IsOnBoard(board) && board[next] == null)
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
		bool CanCapture(SQUARE original, SQUARE_DIFF direction)
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

		public sealed override IEnumerable<MOVE> EnumerateMoves(MOVE m)
		{
			SQUARE original = m.Squares[^1];
			SQUARE_DIFF[] directions = [(1, Forward), (-1, Forward)];

			foreach (SQUARE_DIFF direction in directions)
			{
				SQUARE next = original + direction;
				MOVE_INFO info = GetMoveInfo(m, next);

				if (info.Move)
				{
					MOVE copy = new(m);
					copy.AddMove(next);
					yield return copy;
				}
			}
		}
		public sealed override IEnumerable<MOVE> EnumerateJumps(MOVE m)
		{
			SQUARE original = m.Squares[^1];
			SQUARE_DIFF[] directions = [(2, DoubleForward), (-2, DoubleForward)];

			foreach (SQUARE_DIFF direction in directions)
			{
				SQUARE next = original + direction;
				MOVE_INFO info = GetMoveInfo(m, next);


				if (info.CompleteJump)
				{
					MOVE copy = new(m);
					copy.AddJump(next, board[original | next]!);
					yield return copy;
				}
				else if (info.IncompleteJump)
				{
					MOVE copy = new(m);
					copy.AddJump(next, board[original | next]!);
					foreach (MOVE complete_move in EnumerateJumps(copy))
					{
						yield return complete_move;
					}
				}
			}
		}
	}
	record class WhiteMan(IBoard board, SQUARE position): ManBase(board, position, loaded_image)
	{
		static readonly Image loaded_image = Image.FromFile("img/white_man.png");
		public override int Value { get => 1; }

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

		public override string ToString()
		{
			return "b";
		}
	}
	record class BlackMan(IBoard board, SQUARE position): ManBase(board, position, loaded_image)
	{
		static readonly Image loaded_image = Image.FromFile("img/black_man.png");
		public override int Value { get => -1; }

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

		public override string ToString()
		{
			return "č";
		}
	}

	abstract record class KingBase(IBoard board, SQUARE position, Image image): Piece(board, position, image)
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

		IEnumerable<MOVE> EnumerateMoves(MOVE m, SQUARE_DIFF direction)
		{
			SQUARE original = m.Squares[^1];
			SQUARE next = original + direction;

			while (next.IsOnBoard(board))
			{
				MOVE_INFO info = GetMoveInfo(m, next);
				if (info.Move)
				{
					MOVE copy = new(m);
					copy.AddMove(next);
					yield return copy;
				}
				else
				{
					yield break;
				}

				next += direction;
			}
		}
		IEnumerable<MOVE> EnumerateJumps(MOVE m, SQUARE_DIFF direction)
		{
			SQUARE original = m.Squares[^1];
			SQUARE next = original + direction;

			while (next.IsOnBoard(board) && board[next] == null)
			{
				next += direction;
			}

			if (!next.IsOnBoard(board) || !HasDifferentColour(board[next]))
			{
				yield break;
			}

			next += direction;
			while (next.IsOnBoard(board) && board[next] == null)
			{
				MOVE_INFO info = GetMoveInfo(m, next);
				MOVE copy = new(m);
				copy.AddJump(next, info.CapturedPiece!);

				if (info.CompleteJump)
				{
					yield return copy;
				}
				else if (info.IncompleteJump)
				{
					foreach (MOVE complete_move in EnumerateJumps(copy))
					{
						yield return complete_move;
					}
				}
				else
				{
					yield break;
				}

				next += direction;
			}
		}
		public sealed override IEnumerable<MOVE> EnumerateMoves(MOVE m)
		{
			IEnumerable<MOVE> posible_moves = [];
			SQUARE_DIFF[] directions = [(1, 1), (1, -1), (-1, 1), (-1, -1)];

			foreach (SQUARE_DIFF direction in directions)
			{
				IEnumerable<MOVE> enumerated_moves = EnumerateMoves(m, direction);
				posible_moves = posible_moves.Concat(enumerated_moves);
			}

			return posible_moves;
		}
		public sealed override IEnumerable<MOVE> EnumerateJumps(MOVE m)
		{
			IEnumerable<MOVE> posible_moves = [];
			SQUARE_DIFF[] directions = [(1, 1), (1, -1), (-1, 1), (-1, -1)];

			foreach (SQUARE_DIFF direction in directions)
			{
				IEnumerable<MOVE> enumerated_moves = EnumerateJumps(m, direction);
				posible_moves = posible_moves.Concat(enumerated_moves);
			}

			return posible_moves;
		}
	}
	record class WhiteKing(IBoard board, SQUARE position): KingBase(board, position, loaded_image)
	{
		static readonly Image loaded_image = Image.FromFile("img/white_king.png");
		public override int Value { get => 4; }

		protected override bool HasDifferentColour(Piece? other)
		{
			return other is BlackMan || other is BlackKing;
		}

		public override string ToString()
		{
			return "B";
		}
	}
	record class BlackKing(IBoard board, SQUARE position): KingBase(board, position, loaded_image)
	{
		static readonly Image loaded_image = Image.FromFile("img/black_king.png");
		public override int Value { get => -4; }

		protected override bool HasDifferentColour(Piece? other)
		{
			return other is WhiteMan || other is WhiteKing;
		}

		public override string ToString()
		{
			return "Č";
		}
	}
}
