using System.Collections.Generic;

namespace damebot_engine
{
	public enum MOVE_VALIDITY
	{
		invalid, valid, incomplete
	}

	public delegate void MoveEvent();
	public interface IEngine
	{
		bool IsOnMovePiece(SQUARE position);
		MOVE_VALIDITY ValidateMove(Piece piece, SQUARE original, SQUARE next);
		void PerformMove(IReadOnlyList<SQUARE> move);

		event MoveEvent? OnMove;
	}
	public class DameEngine(IBoard board, IPlayer white, IPlayer black): IEngine
	{
		public IBoard Board { get; } = board;
		public IPlayer White { get; } = white;
		public IPlayer Black { get; } = black;

		private IPlayer player_on_move = white;

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
		public MOVE_VALIDITY ValidateMove(Piece piece, SQUARE original, SQUARE next)
		{
			MOVE_TYPE type = piece.GetMoveType(original, next);

			if (type == MOVE_TYPE.move)
			{
				return MOVE_VALIDITY.valid;
			}
			return MOVE_VALIDITY.invalid;
		}
		public void PerformMove(IReadOnlyList<SQUARE> move)
		{
			SQUARE original = move[0];
			SQUARE next = move[^1];
			Board[next] = Board[original];
			Board[original] = null;

			Board[next]!.Move(next);

			for (int fa = 0, fb = 1; fb < move.Count; fa += 1, fb += 1)
			{
				//
			}

			OnMove?.Invoke();
		}
	}
}
