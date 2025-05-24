using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace damebot_engine
{
    /// <summary>
    /// Alias for square difference.
    /// </summary>
    using SQUARE_DIFF = (int Column, int Row);
    /// <summary>
    /// Stores information about a move.
    /// </summary>
    public readonly struct MOVE_INFO
    {
        /// <summary>
        /// Is the move jump which can be followed up with another jump?
        /// </summary>
        public bool IncompleteJump { get; } = false;
        /// <summary>
        /// Is the move jump which can't be followed up with another jump?
        /// </summary>
        public bool CompleteJump { get; } = false;
        /// <summary>
        /// Is the move a standard move (i.e. not a jump)?
        /// </summary>
        public bool Move { get; } = false;

        /// <summary>
        /// Target SQUARE of this move.
        /// </summary>
        public SQUARE Square { get; }
        /// <summary>
        /// A piece captured in this jump.
        /// </summary>
        public Piece? CapturedPiece { get; } = null;

        /// <summary>
        /// Creates instance representing a standard move.
        /// </summary>
        /// <param name="square">target square of this move</param>
        public MOVE_INFO(SQUARE square)
        {
            Move = true;
            Square = square;
        }
        /// <summary>
        /// Creates instance representing one jump.
        /// </summary>
        /// <param name="square">target square of this move</param>
        /// <param name="piece">piece captured in this jump</param>
        /// <param name="is_complete">Is the jump non-extensible?</param>
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

    /// <summary>
    /// Base class for game pieces.
    /// </summary>
    /// <param name="Position">Square which the piece lies on.</param>
    /// <param name="Image">Image representing the piece in GUI.</param>
    public abstract record class Piece(SQUARE Position, Image Image)
    {
        /// <summary>
        /// Reference to image representing the peice in GUI.
        /// </summary>
        public Image Image { get; } = Image;
        /// <summary>
        /// Square which the piece lies on.
        /// </summary>
        public SQUARE Position { get; private init; } = Position;
        /// <summary>
        /// Value of the piece used by DameEngine.
        /// </summary>
        public abstract int Value { get; }

        /// <summary>
        /// Changes position of this piece.
        /// </summary>
        /// <param name="next_position">Position which is the piece moves at.</param>
        /// <returns>Copy of this pices moved at the <paramref name="next_position"/></returns>
        public Piece Move(SQUARE next_position)
        {
            return this with { Position = next_position };
        }
        /// <summary>
        /// Checks if the <paramref name="move"/> extended by <paramref name="next"/>
        /// square is valid and finds information about it.
        /// </summary>
        /// <returns>Instance of <c>MOVE_INFO</c>.</returns>
        public abstract MOVE_INFO GetMoveInfo(IBoard board, MOVE move, SQUARE next);

        /// <summary>
        /// Checks if this piece can capture any enemy piece after performing a given move.
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <param name="move">The move performed before the check.</param>
        /// <returns>
        /// <c>true</c> if this piece can capture any enemy piece,
        /// <c>false</c> otherwise
        /// </returns>
        public abstract bool CanCapture(IBoard board, MOVE move);
        /// <summary>
        /// Checks if this piece can capture any enemy piece.
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <returns>
        /// <c>true</c> if this piece can capture any enemy piece,
        /// <c>false</c> otherwise
        /// </returns>
        public bool CanCapture(IBoard board)
        {
            return CanCapture(board, new MOVE(Position));
        }

        /// <summary>
        /// Checks if this piece can be promoted.
        /// </summary>
        /// <param name="board">board on which the check take place</param>
        /// <returns>
        /// <c>true</c> if this piece can be promoted on the given <paramref name="board"/>,
        /// <c>false</c> otherwise
        /// </returns>
        public abstract bool CanBePromoted(IBoard board);
        /// <summary>
        /// Promotes this piece.
        /// </summary>
        /// <returns>
        /// A promoted piece instance lying on the same position as this piece.
        /// </returns>
        public abstract Piece Promote();
        /// <summary>
        /// Enumerates all moves that this piece can perform on the given
        /// <paramref name="board"/> after performing the <paramref name="move"/>.
        /// </summary>
        /// <returns>IEnumerable of all possible moves.</returns>
        public abstract IEnumerable<MOVE> EnumerateMoves(IBoard board, MOVE move);
        /// <summary>
        /// Enumerates all jumps that this piece can perform on the given
        /// <paramref name="board"/> after performing the <paramref name="move"/>.
        /// </summary>
        /// <returns>IEnumerable of all possible jumps.</returns>
        public abstract IEnumerable<MOVE> EnumerateJumps(IBoard board, MOVE move);

        /// <summary>
        /// Checks if <paramref name="other"/> piece has the opposite colour than this piece.
        /// </summary>
        /// <param name="other">Any piece or null.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> has the opposite colour than this piece,
        /// <c>false</c> otherwise (including the case when <paramref name="other"/> is <c>null</c>)
        /// </returns>
        protected abstract bool HasDifferentColour(Piece? other);
    }

    /// <summary>
    /// Base class for man type of pieces.
    /// </summary>
    /// <param name="Position">Square which the piece lies on.</param>
    /// <param name="Image">Image representing the piece in GUI.</param>
    abstract record class ManBase(SQUARE Position, Image image): Piece(Position, image)
    {
        /// <summary>
        /// Direction of this man move.
        /// </summary>
        /// <value>
        /// <c>1</c>, if the man moves forwards,
        /// <c>-1</c>, if the man moves backwards,
        /// </value>
        protected abstract int Forward { get; }
        /// <summary>
        /// Direction of this man move jump.
        /// </summary>
        /// <value>Always doble of <c>Forward.</c></value>
        protected int DoubleForward { get => 2 * Forward; }

        /// <summary>
        /// Checks if the jump from <paramref name="original"/> square to <paramref name="next"/>
        /// square is possible on the given <paramref name="board"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the jump is possible, <c>false otherwise</c>
        /// </returns>
        bool IsJumpPossible(IBoard board, SQUARE original, SQUARE next)
        {
            if (!next.IsOnBoard(board))
                return false;

            Piece? piece = board[original | next];
            return HasDifferentColour(piece) && board[next] == null;
        }
        /// <summary>
        /// Checks if the <paramref name="move"/> extended by <paramref name="next"/>
        /// square is valid and finds information about it.
        /// </summary>
        /// <returns>Instance of <c>MOVE_INFO</c>.</returns>
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
        /// <summary>
        /// Checks if this man can capture any enemy piece in the given 
        /// <paramref name="direction"/> from the <paramref name="original"/> square
        /// with respect to position defined by <paramref name="board"/>. 
        /// </summary>
        /// <returns>
        /// <c>true</c> if this piece can capture any enemy piece,
        /// <c>false</c> otherwise
        /// </returns>
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
        /// <summary>
        /// Checks if this man can capture any enemy piece after performing a given move.
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <param name="move">The move performed before the check.</param>
        /// <returns>
        /// <c>true</c> if this man can capture any enemy piece,
        /// <c>false</c> otherwise
        /// </returns>
        public sealed override bool CanCapture(IBoard board, MOVE move)
        {
            return CanCapture(board, move.LastSquare, (1, Forward)) || CanCapture(board, move.LastSquare, (-1, Forward));
        }

        /// <summary>
        /// Enumerates all moves that this man can perform on the given
        /// <paramref name="board"/> after performing the <paramref name="move"/>.
        /// </summary>
        /// <returns>IEnumerable of all possible moves.</returns>
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
        /// <summary>
        /// Enumerates all jumps that this man can perform on the given
        /// <paramref name="board"/> after performing the <paramref name="move"/>.
        /// </summary>
        /// <returns>IEnumerable of all possible jumps.</returns>
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
    /// <summary>
    /// A piece representing one white man.
    /// </summary>
    /// <param name="Position">Position of this piece.</param>
    record class WhiteMan(SQUARE Position): ManBase(Position, loaded_image)
    {
        /// <summary>
        /// Image representing WhiteMan in GUI.
        /// </summary>
        static readonly Image loaded_image = Image.FromFile("img/white_man.png");
        /// <summary>
        /// Value of the WhiteMan used by DameEngine.
        /// </summary>
        public override int Value { get => 1; }
        /// <summary>
        /// Direction of this man move.
        /// </summary>
        /// <value>Always <c>1</c> as white men moves forwards.</value>
        protected override int Forward { get => 1; }

        /// <summary>
        /// Checks if this WhiteMan can be promoted into WhiteKing.
        /// </summary>
        /// <param name="board">board on which the check take place</param>
        /// <returns>
        /// <c>true</c> if this WhiteMan can be promoted on the given <paramref name="board"/>,
        /// <c>false</c> otherwise
        /// </returns>
        public override bool CanBePromoted(IBoard board)
        {
            return Position.Row == board.Size - 1;
        }
        /// <summary>
        /// Promotes this WhiteMan into WhiteKing.
        /// </summary>
        /// <returns>
        /// An instance of WhiteKing lying on the same position as this WhiteMan.
        /// </returns>
        public override Piece Promote()
        {
            return new WhiteKing(Position);
        }

        /// <summary>
        /// Checks if <paramref name="other"/> piece has the opposite colour than this WhiteMan.
        /// </summary>
        /// <param name="other">Any piece or null.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> is BlackMan or BlackKing,
        /// <c>false</c> otherwise (including the case when <paramref name="other"/> is <c>null</c>)
        /// </returns>
        protected override bool HasDifferentColour(Piece? other)
        {
            return other is BlackMan || other is BlackKing;
        }
        /// <summary>
        /// Converts this WhiteMan to its string reprezentation. 
        /// </summary>
        /// <returns>String representation of this WhiteMan.</returns>
        public override string ToString()
        {
            return "b";
        }
    }
    /// <summary>
    /// A piece representing one black man.
    /// </summary>
    /// <param name="Position">Position of this piece.</param>
    record class BlackMan(SQUARE Position): ManBase(Position, loaded_image)
    {
        /// <summary>
        /// Image representing BlackMan in GUI.
        /// </summary>
        static readonly Image loaded_image = Image.FromFile("img/black_man.png");
        /// <summary>
        /// Value of the BlackMan used by DameEngine.
        /// </summary>
        public override int Value { get => -1; }
        /// <summary>
        /// Direction of this man move.
        /// </summary>
        /// <value>Always <c>-1</c> as black men moves backwards.</value>
        protected override int Forward { get => -1; }

        /// <summary>
        /// Checks if this BlackMan can be promoted into BlackKing.
        /// </summary>
        /// <param name="board">board on which the check take place</param>
        /// <returns>
        /// <c>true</c> if this BlackMan can be promoted on the given <paramref name="board"/>,
        /// <c>false</c> otherwise
        /// </returns>
        public override bool CanBePromoted(IBoard board)
        {
            return Position.Row == 0;
        }
        /// <summary>
        /// Promotes this BlackMan into BlackKing.
        /// </summary>
        /// <returns>
        /// An instance of BlackKing lying on the same position as this BlackMan.
        /// </returns>
        public override Piece Promote()
        {
            return new BlackKing(Position);
        }

        /// <summary>
        /// Checks if <paramref name="other"/> piece has the opposite colour than this BlackMan.
        /// </summary>
        /// <param name="other">Any piece or null.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> is WhiteMan or WhiteKing,
        /// <c>false</c> otherwise (including the case when <paramref name="other"/> is <c>null</c>)
        /// </returns>
        protected override bool HasDifferentColour(Piece? other)
        {
            return other is WhiteMan || other is WhiteKing;
        }
        /// <summary>
        /// Converts this BlackMan to its string reprezentation. 
        /// </summary>
        /// <returns>String representation of this BlackMan.</returns>
        public override string ToString()
        {
            return "č";
        }
    }

    /// <summary>
    /// Base class for king type of pieces.
    /// </summary>
    /// <param name="Position">Square which the piece lies on.</param>
    /// <param name="Image">Image representing the piece in GUI.</param>
    abstract record class KingBase(SQUARE Position, Image image): Piece(Position, image)
    {
        /// <summary>
        /// Checks if this king can capture any enemy piece in
        /// the given <paramref name="direction"/> after performing a given <paramref name="move"/>
        /// with respect to position defined by <paramref name="board"/>. 
        /// </summary>
        /// <returns>
        /// <c>true</c> if this piece can capture any enemy piece,
        /// <c>false</c> otherwise
        /// </returns>
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
        /// <summary>
        /// Checks if this king can capture any enemy piece after performing a given move.
        /// </summary>
        /// <param name="board">The board on which the game takes place.</param>
        /// <param name="move">The move performed before the check.</param>
        /// <returns>
        /// <c>true</c> if this king can capture any enemy piece,
        /// <c>false</c> otherwise
        /// </returns>
        public sealed override bool CanCapture(IBoard board, MOVE move)
        {
            return CanCapture(board, move, (1, 1)) || CanCapture(board, move, (-1, -1))
                || CanCapture(board, move, (1, -1)) || CanCapture(board, move, (-1, 1));
        }
        /// <summary>
        /// Checks if the <paramref name="move"/> extended by <paramref name="next"/>
        /// square is valid and finds information about it.
        /// </summary>
        /// <returns>Instance of <c>MOVE_INFO</c>.</returns>
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
        /// <summary>
        /// Checks if the king can be promoted.
        /// </summary>
        /// <param name="board">board on which the check take place</param>
        /// <returns>Always <c>false</c> as the kinh can't be promoted.</returns>
        public override bool CanBePromoted(IBoard board)
        {
            return false;
        }
        /// <summary>
        /// Throws an exception because king can't be promoted.
        /// </summary>
        /// <returns>Never returns.</returns>
        /// <exception cref="InvalidOperationException">Always throws.</exception>
        public override Piece Promote()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Enumerates all moves that this king can perform on the given
        /// <paramref name="board"/> after performing the <paramref name="move"/>
        /// in the given <paramref name="direction"/>.
        /// </summary>
        /// <returns>IEnumerable of all possible moves.</returns>
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
        /// <summary>
        /// Enumerates all jumps that this king can perform on the given
        /// <paramref name="board"/> after performing the <paramref name="move"/>
        /// in the given <paramref name="direction"/>.
        /// </summary>
        /// <returns>IEnumerable of all possible jumps.</returns>
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

        /// <summary>
        /// Enumerates all moves that this king can perform on the given
        /// <paramref name="board"/> after performing the <paramref name="move"/>.
        /// </summary>
        /// <returns>IEnumerable of all possible moves.</returns>
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
        /// <summary>
        /// Enumerates all jumps that this king can perform on the given
        /// <paramref name="board"/> after performing the <paramref name="move"/>.
        /// </summary>
        /// <returns>IEnumerable of all possible jumps.</returns>
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
    /// <summary>
    /// A piece representing one white king.
    /// </summary>
    /// <param name="Position">Position of this piece.</param>
    record class WhiteKing(SQUARE Position): KingBase(Position, loaded_image)
    {
        /// <summary>
        /// Image representing WhiteKing in GUI.
        /// </summary>
        static readonly Image loaded_image = Image.FromFile("img/white_king.png");
        /// <summary>
        /// Value of the WhiteKing used by DameEngine.
        /// </summary>
        public override int Value { get => 5; }

        /// <summary>
        /// Checks if <paramref name="other"/> piece has the opposite colour than this WhiteKing.
        /// </summary>
        /// <param name="other">Any piece or null.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> is BlackMan or BlackKing,
        /// <c>false</c> otherwise (including the case when <paramref name="other"/> is <c>null</c>)
        /// </returns>
        protected override bool HasDifferentColour(Piece? other)
        {
            return other is BlackMan || other is BlackKing;
        }
        /// <summary>
        /// Converts this WhiteKing to its string reprezentation. 
        /// </summary>
        /// <returns>String representation of this WhiteKing.</returns>
        public override string ToString()
        {
            return "B";
        }
    }
    /// <summary>
    /// A piece representing one black king.
    /// </summary>
    /// <param name="Position">Position of this piece.</param>
    record class BlackKing(SQUARE Position): KingBase(Position, loaded_image)
    {
        /// <summary>
        /// Image representing BlackKing in GUI.
        /// </summary>
        static readonly Image loaded_image = Image.FromFile("img/black_king.png");
        /// <summary>
        /// Value of the BlackKing used by DameEngine.
        /// </summary>
        public override int Value { get => -5; }

        /// <summary>
        /// Checks if <paramref name="other"/> piece has the opposite colour than this BlackKing.
        /// </summary>
        /// <param name="other">Any piece or null.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> is WhiteMan or WhiteKing,
        /// <c>false</c> otherwise (including the case when <paramref name="other"/> is <c>null</c>)
        /// </returns>
        protected override bool HasDifferentColour(Piece? other)
        {
            return other is WhiteMan || other is WhiteKing;
        }
        /// <summary>
        /// Converts this BlackKing to its string reprezentation. 
        /// </summary>
        /// <returns>String representation of this BlackKing.</returns>
        public override string ToString()
        {
            return "Č";
        }
    }
}
