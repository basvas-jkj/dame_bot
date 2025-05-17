using System.Collections.Generic;
using System.Diagnostics;

namespace damebot_engine
{
	public delegate void MoveEvent();
	public interface IEngine
	{
		bool IsOnMovePiece(SQUARE position);
		MOVE_INFO ValidateMove(Piece piece, MOVE m, SQUARE next);
		void PerformMove(MOVE m);

		event MoveEvent? OnMove;
	}
	public class DameEngine(IBoard board, Player white, Player black): IEngine
	{
		public IBoard Board { get; } = board;
		public Player White { get; } = white;
		public Player Black { get; } = black;

		private Player player_on_move = white;
		private Player waiting_player
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

			IReadOnlyList<SQUARE> squares = m.Squares;
			SQUARE original = squares[0];
			SQUARE next = squares[^1];

			Piece moved = Board[original]!;
			Board[original] = null;
			Board[next] = moved;

			moved.Move(next);

			foreach (Piece captured in m.CapturedPieces)
			{
				waiting_player.RemovePiece(captured);
				Board[captured.Position] = null;
			}
			if (moved.CanBePromoted())
			{
				Piece promoted = moved.Promote();
				Board[next] = promoted;
				player_on_move.RemovePiece(moved);
				player_on_move.AddPiece(promoted);
			}

			SwitchPlayers();
			OnMove?.Invoke();
		}
	}
}
