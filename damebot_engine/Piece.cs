using System.Drawing;

namespace damebot_engine
{
	public abstract class Piece(Square position, Image image)
	{
		public Square position { get; private set; } = position;
		public Image image { get; } = image;
	}
	class WhiteMan(Square position): Piece(position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/white_man.png");
	}
	class BlackMan(Square position): Piece(position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/black_man.png");
	}
	class WhiteKing(Square position): Piece(position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/white_king.png");
	}
	class BlackKing(Square position): Piece(position, loaded_image)
	{
		static Image loaded_image = Image.FromFile("img/black_king.png");
	}
}
