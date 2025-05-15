using System.Collections.Generic;

namespace damebot_engine
{
	public abstract class Player
	{
		protected List<Piece> pieces = new();

		public IReadOnlyList<Piece> GetPieces()
		{
			return pieces;
		}
		public void AddPiece(Piece p)
		{
			pieces.Add(p);
		}
		public void RemovePiece(Piece p)
		{
			pieces.Remove(p);
		}

		public bool CanCapture()
		{
			foreach (Piece p in pieces)
			{
				if (p.CanCapture())
				{
					return true;
				}
			}
			return false;
		}
	}
	public class RealPlayer: Player
	{ }
	public class AutomaticPlayer: Player
	{ }
	public class DamebotDefaultPlayer: AutomaticPlayer
	{ }
}
