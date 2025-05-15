using System;
using System.Drawing;

namespace damebot_engine
{
	using SQUARE_DIFF = (int x, int y);

	public enum MOVE_TYPE
	{
		move, jump, invalid
	}
	public abstract class Piece(SQUARE position, Image image)
	{
		public Image image { get; } = image;
		public SQUARE position { get; private set; } = position;

		public void Move(SQUARE next_position)
		{
			position = next_position;
		}
		public abstract MOVE_TYPE GetMoveType(SQUARE original, SQUARE next);

		protected static SQUARE_DIFF Normalise(SQUARE_DIFF diff)
		{
			return (Math.Sign(diff.x), Math.Sign(diff.y));
		}
	}
	class WhiteMan(SQUARE position): Piece(position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/white_man.png");

		public override MOVE_TYPE GetMoveType(SQUARE original, SQUARE next)
		{
			SQUARE_DIFF difference = next - original;
			if (difference == (1, 1) || difference == (-1, 1))
			{
				return MOVE_TYPE.move;
			}
			else if (difference == (2, 2) || difference == (-2, 2))
			{
				return MOVE_TYPE.jump;
			}
			else
			{
				return MOVE_TYPE.invalid;
			}
		}
	}
	class BlackMan(SQUARE position): Piece(position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/black_man.png");

		public override MOVE_TYPE GetMoveType(SQUARE original, SQUARE next)
		{
			SQUARE_DIFF difference = next - original;
			if (difference == (1, -1) || difference == (-1, -1))
			{
				return MOVE_TYPE.move;
			}
			else if (difference == (2, -2) || difference == (-2, -2))
			{
				return MOVE_TYPE.jump;
			}
			else
			{
				return MOVE_TYPE.invalid;
			}
		}
	}
	class WhiteKing(SQUARE position): Piece(position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/white_king.png");

		public override MOVE_TYPE GetMoveType(SQUARE original, SQUARE next)
		{
			SQUARE_DIFF difference = next - original;
			SQUARE_DIFF direction = Normalise(difference);

			if (difference.x != difference.y && difference.x != -difference.y)
			{
				return MOVE_TYPE.invalid;
			}

#warning Doplnit implementaci.
			return MOVE_TYPE.move;
		}
	}
	class BlackKing(SQUARE position): Piece(position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/black_king.png");

		public override MOVE_TYPE GetMoveType(SQUARE original, SQUARE next)
		{
			SQUARE_DIFF difference = next - original;
			SQUARE_DIFF direction = Normalise(difference);

			if (difference.x != difference.y && difference.x != -difference.y)
			{
				return MOVE_TYPE.invalid;
			}

#warning Doplnit implementaci.
			return MOVE_TYPE.move;
		}
	}
}
