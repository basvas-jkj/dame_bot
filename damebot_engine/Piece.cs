using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace damebot_engine
{
    using SQUARE_DIFF = (int Column, int Row);
    public readonly struct MOVE_INFO
    {
        public bool IncompleteJump { get; } = false;
        public bool CompleteJump { get; } = false;
        public bool Move { get; } = false;

        public SQUARE Square { get; }
        public Piece? CapturedPiece { get; } = null;

        public MOVE_INFO(SQUARE square)
        {
            Move = true;
            Square = square;
        }
        public MOVE_INFO(SQUARE square, Piece piece, bool is_complete)
        {
            if (is_complete)
            {
                CompleteJump = true;
            }
            else
            {
                IncompleteJump = true;
            }

            Square = square;
            CapturedPiece = piece;
        }
    }

    public abstract record class Piece(SQUARE Position, Image Image)
    {
        public Image Image { get; } = Image;
        public SQUARE Position { get; private init; } = Position;
        public abstract int Value { get; }

        public Piece Move(SQUARE next_position)
        {
            return this with { Position = next_position };
        }
        public abstract MOVE_INFO GetMoveInfo(IBoard board, MOVE move, SQUARE next);

        public abstract bool CanCapture(IBoard board, MOVE move);
        public bool CanCapture(IBoard board)
        {
            return CanCapture(board, new MOVE(Position));
        }

        public abstract bool CanBePromoted(IBoard board);
        public abstract Piece Promote();
        public abstract IEnumerable<MOVE> EnumerateMoves(IBoard board, MOVE move);
        public abstract IEnumerable<MOVE> EnumerateJumps(IBoard board, MOVE move);

        protected abstract bool HasDifferentColour(Piece? other);
    }

    abstract record class ManBase(SQUARE position, Image image): Piece(position, image)
    {
        protected abstract int Forward { get; }
        protected int DoubleForward { get => 2 * Forward; }

        bool IsJumpPossible(IBoard board, SQUARE original, SQUARE next)
        {
            if (!next.IsOnBoard(board))
                return false;

            Piece? piece = board[original | next];
            return HasDifferentColour(piece) && board[next] == null;
        }
        public sealed override MOVE_INFO GetMoveInfo(IBoard board, MOVE move, SQUARE next)
        {
            SQUARE_DIFF difference = next - move.LastSquare;
            if ((difference == (1, Forward) || difference == (-1, Forward))
                && next.IsOnBoard(board) && board[next] == null)
            {
                return new MOVE_INFO(next);
            }
            else if ((difference == (2, DoubleForward) || difference == (-2, DoubleForward))
                && IsJumpPossible(board, move.LastSquare, next))
            {
                Piece piece = board[move.LastSquare | next]!;
                MOVE simulated = new(move);
                simulated.AddJump(next, piece);

                bool can_capture = CanCapture(board, simulated);
                return new MOVE_INFO(next, piece, !can_capture);
            }
            else
            {
                return new MOVE_INFO();
            }
        }
        bool CanCapture(IBoard board, SQUARE original, SQUARE_DIFF direction)
        {
            SQUARE obstacle = original + direction;
            SQUARE destination = obstacle + direction;

            if (obstacle.IsOnBoard(board) && destination.IsOnBoard(board))
            {
                return board[destination] == null && HasDifferentColour(board[obstacle]);
            }
            else
            {
                return false;
            }
        }
        public sealed override bool CanCapture(IBoard board, MOVE move)
        {
            return CanCapture(board, move.LastSquare, (1, Forward)) || CanCapture(board, move.LastSquare, (-1, Forward));
        }

        public sealed override IEnumerable<MOVE> EnumerateMoves(IBoard board, MOVE move)
        {
            SQUARE original = move.Squares[^1];
            SQUARE_DIFF[] directions = [(1, Forward), (-1, Forward)];

            foreach (SQUARE_DIFF direction in directions)
            {
                SQUARE next = original + direction;
                MOVE_INFO info = GetMoveInfo(board, move, next);

                if (info.Move)
                {
                    MOVE copy = new(move);
                    copy.AddMove(next);
                    yield return copy;
                }
            }
        }
        public sealed override IEnumerable<MOVE> EnumerateJumps(IBoard board, MOVE move)
        {
            SQUARE original = move.Squares[^1];
            SQUARE_DIFF[] directions = [(2, DoubleForward), (-2, DoubleForward)];

            foreach (SQUARE_DIFF direction in directions)
            {
                SQUARE next = original + direction;
                MOVE_INFO info = GetMoveInfo(board, move, next);


                if (info.CompleteJump)
                {
                    MOVE copy = new(move);
                    copy.AddJump(next, board[original | next]!);
                    yield return copy;
                }
                else if (info.IncompleteJump)
                {
                    MOVE copy = new(move);
                    copy.AddJump(next, board[original | next]!);
                    foreach (MOVE complete_move in EnumerateJumps(board, copy))
                    {
                        yield return complete_move;
                    }
                }
            }
        }
    }
    record class WhiteMan(SQUARE position): ManBase(position, loaded_image)
    {
        static readonly Image loaded_image = Image.FromFile("img/white_man.png");
        public override int Value { get => 1; }
        protected override int Forward { get => 1; }

        public override bool CanBePromoted(IBoard board)
        {
            return Position.Row == board.Size - 1;
        }
        public override Piece Promote()
        {
            return new WhiteKing(Position);
        }

        protected override bool HasDifferentColour(Piece? other)
        {
            return other is BlackMan || other is BlackKing;
        }
        public override string ToString()
        {
            return "b";
        }
    }
    record class BlackMan(SQUARE position): ManBase(position, loaded_image)
    {
        static readonly Image loaded_image = Image.FromFile("img/black_man.png");
        public override int Value { get => -1; }

        protected override int Forward { get => -1; }

        public override bool CanBePromoted(IBoard board)
        {
            return Position.Row == 0;
        }
        public override Piece Promote()
        {
            return new BlackKing(Position);
        }

        protected override bool HasDifferentColour(Piece? other)
        {
            return other is WhiteMan || other is WhiteKing;
        }

        public override string ToString()
        {
            return "č";
        }
    }

    abstract record class KingBase(SQUARE position, Image image): Piece(position, image)
    {
        private bool CanCapture(IBoard board, MOVE move, SQUARE_DIFF direction)
        {
            for (SQUARE square = move.LastSquare + direction; square.IsOnBoard(board); square += direction)
            {
                Piece? piece = board[square];
                if (HasDifferentColour(piece) && !move.CapturedPieces.Contains(piece))
                {
                    square += direction;
                    return square.IsOnBoard(board) && board[square] == null;
                }
                else if (piece != null)
                {
                    return false;
                }
            }
            return false;
        }
        public sealed override bool CanCapture(IBoard board, MOVE move)
        {
            return CanCapture(board, move, (1, 1)) || CanCapture(board, move, (-1, -1))
                || CanCapture(board, move, (1, -1)) || CanCapture(board, move, (-1, 1));
        }
        public override MOVE_INFO GetMoveInfo(IBoard board, MOVE move, SQUARE next)
        {
            SQUARE_DIFF difference = next - move.LastSquare;
            if (difference.Column != difference.Row && difference.Column != -difference.Row)
            {
                return new MOVE_INFO();
            }
            else if (board[next] != null)
            {
                return new MOVE_INFO();
            }

            Piece? jumped_enemy_piece = null;
            SQUARE_DIFF direction = difference.Normalise();
            for (SQUARE square = move.LastSquare + direction; square != next; square += direction)
            {
                Piece? piece = board[square];
                if (piece == null)
                {
                    continue;
                }
                else if (HasDifferentColour(piece))
                {
                    if (jumped_enemy_piece != null)
                    {
                        return new MOVE_INFO();
                    }
                    else
                    {
                        jumped_enemy_piece = piece;
                    }
                }
                else
                {
                    return new MOVE_INFO();
                }
            }

            if (jumped_enemy_piece == null)
            {
                return new MOVE_INFO(next);
            }
            else
            {
                MOVE simulated = new(move);
                simulated.AddJump(next, jumped_enemy_piece);

                bool can_capture = CanCapture(board, simulated);
                return new MOVE_INFO(next, jumped_enemy_piece, !can_capture);
            }
        }
        public override bool CanBePromoted(IBoard board)
        {
            return false;
        }
        public override Piece Promote()
        {
            throw new InvalidOperationException();
        }

        IEnumerable<MOVE> EnumerateMoves(IBoard board, MOVE move, SQUARE_DIFF direction)
        {
            SQUARE original = move.Squares[^1];
            SQUARE next = original + direction;

            while (next.IsOnBoard(board))
            {
                MOVE_INFO info = GetMoveInfo(board, move, next);
                if (info.Move)
                {
                    MOVE copy = new(move);
                    copy.AddMove(next);
                    yield return copy;
                }
                else
                {
                    yield break;
                }

                next += direction;
            }
        }
        IEnumerable<MOVE> EnumerateJumps(IBoard board, MOVE move, SQUARE_DIFF direction)
        {
            SQUARE original = move.Squares[^1];
            SQUARE next = original + direction;

            while (next.IsOnBoard(board) && board[next] == null)
            {
                next += direction;
            }

            if (!next.IsOnBoard(board) || !HasDifferentColour(board[next]))
            {
                yield break;
            }

            next += direction;
            while (next.IsOnBoard(board) && board[next] == null)
            {
                MOVE_INFO info = GetMoveInfo(board, move, next);
                MOVE copy = new(move);
                copy.AddJump(next, info.CapturedPiece!);

                if (info.CompleteJump)
                {
                    yield return copy;
                }
                else if (info.IncompleteJump)
                {
                    foreach (MOVE complete_move in EnumerateJumps(board, copy))
                    {
                        yield return complete_move;
                    }
                }
                else
                {
                    yield break;
                }

                next += direction;
            }
        }
        public sealed override IEnumerable<MOVE> EnumerateMoves(IBoard board, MOVE move)
        {
            IEnumerable<MOVE> posible_moves = [];
            SQUARE_DIFF[] directions = [(1, 1), (1, -1), (-1, 1), (-1, -1)];

            foreach (SQUARE_DIFF direction in directions)
            {
                IEnumerable<MOVE> enumerated_moves = EnumerateMoves(board, move, direction);
                posible_moves = posible_moves.Concat(enumerated_moves);
            }

            return posible_moves;
        }
        public sealed override IEnumerable<MOVE> EnumerateJumps(IBoard board, MOVE move)
        {
            IEnumerable<MOVE> posible_moves = [];
            SQUARE_DIFF[] directions = [(1, 1), (1, -1), (-1, 1), (-1, -1)];

            foreach (SQUARE_DIFF direction in directions)
            {
                IEnumerable<MOVE> enumerated_moves = EnumerateJumps(board, move, direction);
                posible_moves = posible_moves.Concat(enumerated_moves);
            }

            return posible_moves;
        }
    }
    record class WhiteKing(SQUARE position): KingBase(position, loaded_image)
    {
        static readonly Image loaded_image = Image.FromFile("img/white_king.png");
        public override int Value { get => 5; }

        protected override bool HasDifferentColour(Piece? other)
        {
            return other is BlackMan || other is BlackKing;
        }

        public override string ToString()
        {
            return "B";
        }
    }
    record class BlackKing(SQUARE position): KingBase(position, loaded_image)
    {
        static readonly Image loaded_image = Image.FromFile("img/black_king.png");
        public override int Value { get => -5; }

        protected override bool HasDifferentColour(Piece? other)
        {
            return other is WhiteMan || other is WhiteKing;
        }

        public override string ToString()
        {
            return "Č";
        }
    }
}
