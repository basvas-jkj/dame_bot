using System.Collections.Generic;

namespace damebot_engine
{
	public enum PLAYER_TYPE { min, max }
	public interface IPlayer
	{
		bool Automatic { get; }

		void AddPiece(Piece p);
		void RemovePiece(Piece p);
		bool CanCapture();
		MOVE FindNextMove(IBoard board);
		IReadOnlyList<Piece> GetPieces();
	}
	public class Player: IPlayer
	{
		public bool Automatic { get; }
		protected List<Piece> pieces = new();

		public Player(bool automatic, PLAYER_TYPE type)
		{
			Automatic = automatic;
		}

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
		IEnumerable<MOVE> EnumerateAllMoves()
		{
			foreach (Piece p in pieces)
			{
				foreach (MOVE m in p.EnumerateMoves(new MOVE(p.Position)))
				{
					yield return m;
				}
			}
		}
		IEnumerable<MOVE> EnumerateAllJumps()
		{
			foreach (Piece p in pieces)
			{
				foreach (MOVE m in p.EnumerateJumps(new MOVE(p.Position)))
				{
					yield return m;
				}
			}
		}
		public MOVE FindNextMove(IBoard board)
		{
			IEnumerable<MOVE> moves = CanCapture() ? EnumerateAllJumps() : EnumerateAllMoves();

			MOVE mm = new();
			foreach (MOVE m in moves)
			{
				IBoard simulated = board.SimulateMove(m);
				mm = m;
				System.Diagnostics.Debug.WriteLine(simulated.SimulateMove(m).EvaluatePosition());
			}
			return mm;
		}
	}
}
