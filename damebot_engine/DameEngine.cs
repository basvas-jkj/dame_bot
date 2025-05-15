using System.Collections.Generic;
using System.Diagnostics;

namespace damebot_engine
{
	public delegate void MoveEvent();
	public interface IEngine
	{
		bool IsOnMovePiece(SQUARE position);
		MOVE_TYPE ValidateMove(Piece piece, SQUARE original, SQUARE next);
		void PerformMove(IReadOnlyList<SQUARE> move, bool jump);

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
		public MOVE_TYPE ValidateMove(Piece piece, SQUARE original, SQUARE next)
		{
			MOVE_TYPE type = piece.GetMoveType(original, next);

			if (type == MOVE_TYPE.jump)
			{
				return (piece.CanCapture(next)) ? MOVE_TYPE.incomplete_jump : MOVE_TYPE.jump;
			}
			if (type == MOVE_TYPE.move && !player_on_move.CanCapture())
			{
				return MOVE_TYPE.move;
			}
			else
			{
				return MOVE_TYPE.invalid;
			}
		}

		private void RemovePieces(IReadOnlyList<SQUARE> move)
		{
			for (int fa = 0, fb = 1; fb < move.Count; fa += 1, fb += 1)
			{
				SQUARE s = move[fa] | move[fb];
				waiting_player.RemovePiece(Board[s]!);
				Board[s] = null;
			}
		}
		public void PerformMove(IReadOnlyList<SQUARE> move, bool jump)
		{
			Debug.Assert(Board[move[0]] != null);
			Debug.Assert(move.Count >= 2);

			SQUARE original = move[0];
			SQUARE next = move[^1];

			Piece moved = Board[original]!;
			Board[original] = null;
			Board[next] = moved;

			moved.Move(next);

			if (jump)
			{
				RemovePieces(move);
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
