using System;
using System.Collections.Generic;

namespace damebot_engine
{
    public readonly struct MOVE
    {
        readonly List<SQUARE> squares;
        readonly List<Piece> captered_pieces;

        public SQUARE LastSquare
        {
            get => squares[^1];
        }
        public IReadOnlyList<SQUARE> Squares
        {
            get => squares;
        }
        public IReadOnlyList<Piece> CapturedPieces
        {
            get => captered_pieces;
        }

        public void AddMove(SQUARE next)
        {
            if (squares.Count > 1)
                throw new InvalidOperationException();

            squares.Add(next);
        }
        public void AddJump(SQUARE next, Piece captured)
        {
            if (squares.Count > 1 && captered_pieces.Count == 0)
                throw new InvalidOperationException();

            squares.Add(next);
            captered_pieces.Add(captured);
        }

        public MOVE(SQUARE original)
        {
            squares = new() { original };
            captered_pieces = new();
        }
        public MOVE(MOVE m)
        {
            squares = new List<SQUARE>(m.squares);
            captered_pieces = new List<Piece>(m.captered_pieces);
        }
    }
}
