using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace damebot_engine
{
	using EVALUATED_MOVE = (MOVE move, int evaluation);

	public enum PLAYER_TYPE { min, max }
	public interface IPlayer
	{
		bool Automatic { get; }

		void AddPiece(Piece p);
		void RemovePiece(Piece p);
		IPlayer Copy();

		bool CanCapture();
		MOVE FindNextMove(IBoard board, IPlayer other);
		EVALUATED_MOVE FindNextMove(IBoard board, IPlayer other, int depth);
		IReadOnlyList<Piece> GetPieces();
	}
	public class Player(bool automatic, PLAYER_TYPE type): IPlayer
	{
		public bool Automatic { get; } = automatic;
		readonly PLAYER_TYPE type = type;
		List<Piece> pieces = new();

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

		public IPlayer Copy()
		{
			return new Player(Automatic, type)
			{
				pieces = new List<Piece>(pieces)
			};
		}

		int InitialEvaluation()
		{
			return (type == PLAYER_TYPE.min) ? 1000 : -1000;
		}
		int CompareEvaluations(int original_evaluation, int new_evaluation)
		{
			if (type == PLAYER_TYPE.min)
			{
				return original_evaluation - new_evaluation;
			}
			else
			{
				return new_evaluation - original_evaluation;
			}
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
		public MOVE FindNextMove(IBoard board, IPlayer other)
		{
			Debug.WriteLine("------------------------------------------");
			return FindNextMove(board, other, 1).move;
		}

		const int max_depth = 4;
		public EVALUATED_MOVE FindNextMove(IBoard board, IPlayer other, int depth)
		{
			IEnumerable<MOVE> moves = CanCapture() ? EnumerateAllJumps() : EnumerateAllMoves();

			int evaluation = InitialEvaluation();
			List<MOVE> best_moves = new();
			foreach (MOVE m in moves)
			{
				int new_evaluation = EvaluateMove(board, other, depth, m);
				int result = CompareEvaluations(evaluation, new_evaluation);

				if (result < 0)
				{
					continue;
				}
				else if (result > 0)
				{
					evaluation = new_evaluation;
					best_moves = [m];
				}
				else
				{
					best_moves.Add(m);
				}
			}

			Random generator = new();
			int random = generator.Next(best_moves.Count);
			return (best_moves[random], evaluation);
		}
		int EvaluateMove(IBoard board, IPlayer other, int depth, MOVE m)
		{
			Piece moving = board[m.Squares[0]]!;
			(IBoard simulated, Piece moved) = board.SimulateMove(m);
			Debug.WriteLine(depth);
			Debug.WriteLine(simulated);

			if (depth == max_depth)
			{
				return simulated.EvaluatePosition();
			}
			else
			{
				Player me = (Player)this.Copy();
				me.RemovePiece(moving);
				me.AddPiece(moved);

				Player you = (Player)other.Copy();
				foreach (Piece captured in m.CapturedPieces)
				{
					you.RemovePiece(captured);
				}

				return you.FindNextMove(simulated, me, depth + 1).evaluation;
			}


		}
	}
}
