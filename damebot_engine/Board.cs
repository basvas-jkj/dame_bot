using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace damebot_engine
{
    using SIMULATED_MOVE = (IBoard board, Piece moved);
    public interface IBoard
    {
        int Size { get; }
        Piece? this[SQUARE position] { get; }

        void GenerateInitialPieces(IPlayer white, IPlayer black);
        Piece PerformMove(MOVE move);
        SIMULATED_MOVE SimulateMove(MOVE move);
        int EvaluatePosition();
    }
    public class DameBoard: IBoard
    {
        private Piece?[,] board;
        public Piece? this[SQUARE position]
        {
            get => board[position.Column, position.Row];
            private set => board[position.Column, position.Row] = value;
        }

        public int Size { get => 8; }

        public DameBoard()
        {
            board = new Piece[Size, Size];
        }
        public DameBoard(DameBoard board)
        {
            this.board = (Piece?[,])board.board.Clone();
        }

        IEnumerable<SQUARE> GenerateInitialPositions()
        {
            for (int column = 0; column < Size; column += 2)
            {
                yield return new SQUARE(column, 0);
                yield return new SQUARE(column, 2);
                yield return new SQUARE(column, Size - 2);
            }
            for (int column = 1; column < Size; column += 2)
            {
                yield return new SQUARE(column, 1);
                yield return new SQUARE(column, Size - 1);
                yield return new SQUARE(column, Size - 3);
            }
        }
        public void GenerateInitialPieces(IPlayer white, IPlayer black)
        {
            foreach (SQUARE position in GenerateInitialPositions())
            {
                Piece piece;
                if (position.Row < Size / 2)
                {
                    piece = new WhiteMan(position);
                    white.AddPiece(piece);
                }
                else
                {
                    piece = new BlackMan(position);
                    black.AddPiece(piece);
                }

                this[position] = piece;
            }
        }
        public int EvaluatePosition()
        {
            int value = 0;
            foreach (Piece? piece in board)
            {
                value += piece?.Value ?? 0;
            }
            return value;
        }
        public Piece PerformMove(MOVE move)
        {
            IReadOnlyList<SQUARE> squares = move.Squares;
            SQUARE original = squares[0];
            SQUARE next = squares[^1];

            Piece moving = this[original]!;
            Piece moved = moving.Move(next);

            this[original] = null;
            this[next] = moved;

            foreach (Piece captured in move.CapturedPieces)
            {
                RemovePiece(captured);
            }

            if (moved.CanBePromoted(this))
            {
                RemovePiece(moved);
                moved = moved.Promote();
                AddPiece(moved);
            }

            return moved;
        }
        public SIMULATED_MOVE SimulateMove(MOVE move)
        {
            DameBoard simulated = new(this);
            Piece moved = simulated.PerformMove(move);
            return (simulated, moved);
        }

        public void AddPiece(Piece added_piece)
        {
            Debug.Assert(this[added_piece.Position] == null);
            this[added_piece.Position] = added_piece;
        }
        public void RemovePiece(Piece removed_piece)
        {
            this[removed_piece.Position] = null;
        }

        public override string ToString()
        {
            StringBuilder builder = new("----------\n");

            for (int row = Size - 1; row >= 0; row -= 1)
            {
                builder.Append("|");
                for (int column = 0; column < Size; column += 1)
                {
                    builder.Append(board[column, row]?.ToString() ?? " ");
                }
                builder.AppendLine("|");
            }

            builder.AppendLine("----------");
            return builder.ToString();
        }
    }
}
