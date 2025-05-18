using System.Diagnostics;

namespace damebot_engine
{
	public delegate void MoveEvent(IPlayer? next_player);
	public interface IEngine
	{
		bool IsOnMovePiece(SQUARE position);
		MOVE_INFO ValidateMove(Piece piece, MOVE m, SQUARE next);
		void PerformMove(MOVE m);

		event MoveEvent? OnMove;
	}
	public class DameEngine(IBoard board, IPlayer white, IPlayer black): IEngine
	{
		public IBoard Board { get; } = board;
		public IPlayer White { get; } = white;
		public IPlayer Black { get; } = black;

		private IPlayer player_on_move = white;
		private IPlayer waiting_player
		{
			get => (player_on_move == White) ? Black : White;
		}
		private void SwitchPlayers()
		{
			player_on_move = (player_on_move == White) ? Black : White;
		}

		public event MoveEvent? OnMove;

		public bool IsOnMovePiece(SQUARE position)
		{
			if (player_on_move == White)
			{
				return Board[position] is WhiteMan || Board[position] is WhiteKing;
			}
			else
			{
				return Board[position] is BlackMan || Board[position] is BlackKing;
			}
		}
		public MOVE_INFO ValidateMove(Piece piece, MOVE m, SQUARE next)
		{
			MOVE_INFO info = piece.GetMoveInfo(m, next);

			if (info.CompleteJump || info.IncompleteJump)
			{
				return info;
			}
			if (info.Move && !player_on_move.CanCapture())
			{
				return info;
			}
			else
			{
				return new MOVE_INFO();
			}
		}

		public void PerformMove(MOVE m)
		{
			Debug.Assert(Board[m.Squares[0]] != null);

			Piece moved = Board[m.Squares[0]]!;
			Board.PerformMove(m);

			foreach (Piece captured in m.CapturedPieces)
			{
				waiting_player.RemovePiece(captured);
				Board.RemovePiece(captured);
			}

			Debug.Assert(moved == Board[m.Squares[^1]]!);
			if (moved.CanBePromoted())
			{
				Piece promoted = moved.Promote();

				Board.RemovePiece(moved);
				Board.AddPiece(promoted);

				player_on_move.RemovePiece(moved);
				player_on_move.AddPiece(promoted);
			}

			SwitchPlayers();
			IPlayer next_player = player_on_move;

			if (next_player.Automatic)
			{
				OnMove?.Invoke(null);
				PerformMove(next_player.FindNextMove(Board));
			}
			else
			{
				OnMove?.Invoke(next_player);
			}
		}
	}
}
