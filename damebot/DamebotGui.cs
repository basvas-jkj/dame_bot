using damebot_engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace damebot
{
    /// <summary>
    /// Graphical user interface for DamebotEngine.
    /// </summary>
    public partial class DamebotGui: Form
    {
        /// <summary>
        /// Colour of the dark squares.
        /// </summary>
        static readonly Color dark = Color.FromArgb(172, 113, 30);
        /// <summary>
        /// Colour of the light squares.
        /// </summary>
        static readonly Color light = Color.FromArgb(255, 255, 240);
        /// <summary>
        /// Colour of the squares selected for the upcoming move by the player.
        /// </summary>
        static readonly Color selected = Color.FromArgb(255, 0, 0);
        /// <summary>
        /// Colour of the squares selected for the upcoming by the compoter.
        /// </summary>
        static readonly Color marked = Color.FromArgb(0, 159, 255);

        /// <summary>
        /// Chooses colour for the board field.
        /// </summary>
        /// <param name="square">Identifiesn of the game board field.</param>
        /// <returns>Colour which will be used during board drawing.</returns>
        private SolidBrush SelectColour(SQUARE square)
        {
            if (selected_squares?.Contains(square) == true)
            {
                return new SolidBrush(selected);
            }
            else if (marked_squares?.Contains(square) == true)
            {
                return new SolidBrush(marked);
            }
            else if ((square.Column + square.Row) % 2 == 0)
            {
                return new SolidBrush(dark);
            }
            else
            {
                return new SolidBrush(light);
            }
        }
        /// <summary>
        /// Computes boundaries of the <paramref name="square"/> used for game board drawing.
        /// </summary>
        /// <returns>computed boundaries as an instance of <c>Rectangle</c></returns>
        private Rectangle ComputeRectangle(SQUARE square)
        {
            int left = square.Column * board_panel.Size.Width / board.Size;
            int upper = board_panel.Size.Height - (square.Row + 1) * board_panel.Size.Height / board.Size;
            int width = board_panel.Size.Width / board.Size;
            int height = board_panel.Size.Height / board.Size;

            return new Rectangle(left, upper, width, height);
        }
        /// <summary>
        /// Computes a square containing the given <paramref name="point"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private SQUARE LocationToSquare(Point point)
        {
            int column = board.Size * point.X / board_panel.Width;
            int row = board.Size - board.Size * point.Y / board_panel.Height - 1;

            return new SQUARE(column, row);
        }

        /// <summary>
        /// Reference to the game board used by the engine.
        /// </summary>
        IBoard board;
        /// <summary>
        /// Reference to the player holding white pieces.
        /// </summary>
        IPlayer white;
        /// <summary>
        /// Reference to the player holding black pieces.
        /// </summary>
        IPlayer black;
        /// <summary>
        /// Reference to the game engine that controls the game.
        /// </summary>
        IEngine engine;

        /// <summary>
        /// List of squares selected by the player for their next move.
        /// </summary>
        IReadOnlyList<SQUARE>? selected_squares;
        /// <summary>
        /// List of squares selected by the computer for its next move.
        /// </summary>
        IReadOnlyList<SQUARE>? marked_squares;
        /// <summary>
        /// The partial move selected by the player.
        /// </summary>
        MOVE current_move;
        /// <summary>
        /// Is the computer on the turn?
        /// </summary>
        bool wait_for_computer;

        /// <summary>
        /// Initialise player instances for the new game.
        /// </summary>
        /// <param name="white_is_computer">Is the white player represented by a computer?</param>
        /// <param name="black_is_computer">Is the black player represented by a computer?</param>
        private void InitPlayers(bool white_is_computer, bool black_is_computer)
        {
            white = new DamePlayer(white_is_computer, PLAYER_TYPE.max, "Bílý");
            black = new DamePlayer(black_is_computer, PLAYER_TYPE.min, "Černý");
        }
        /// <summary>
        /// Initialises the engine for the new game instance.
        /// </summary>
        private void ResetGame()
        {
            selected_squares = null;
            marked_squares = null;

            InitPlayers(computer_black_radio.Checked, white_computer_radio.Checked);
            board = new DameBoard();
            board.GenerateInitialPieces(white, black);
            engine = new DameEngine(board, white, black);
            engine.OnMove += OnMoveHandler;
            engine.OnMark += OnMarkHandler;
            engine.OnGameOver += OnGameOverHandler;
            wait_for_computer = computer_black_radio.Checked;

            Draw();

            if (wait_for_computer)
            {
                engine.PerformAutomaticMove();
            }
        }
        /// <summary>
        /// Draws the labels marking the rows and columns.
        /// </summary>
        private void GenerateEdgeLabels()
        {
            int lable_width = board_panel.Width / board.Size;
            int lable_height = board_panel.Height / board.Size;
            for (int column = 0; column < board.Size; column += 1)
            {
                string character = ((char)('a' + column)).ToString();
                Label top = new()
                {
                    Width = lable_width,
                    Height = board_panel.Top,
                    Top = 0,
                    Left = board_panel.Left + column * lable_width,
                    Text = character,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Label bottom = new()
                {
                    Width = lable_width,
                    Height = board_panel.Top,
                    Top = board_panel.Bottom,
                    Left = board_panel.Left + column * lable_width,
                    Text = character,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                top.MouseLeave += BackgroundPanelMouseLeaveHandler;
                bottom.MouseLeave += BackgroundPanelMouseLeaveHandler;

                background_panel.Controls.Add(top);
                background_panel.Controls.Add(bottom);
            }
            for (int row = 0; row < board.Size; row += 1)
            {
                string number = (board.Size - row).ToString();
                Label left = new()
                {
                    Width = board_panel.Left,
                    Height = lable_height,
                    Top = board_panel.Top + row * lable_height,
                    Left = 0,
                    Text = number,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Label right = new()
                {
                    Width = board_panel.Left,
                    Height = lable_height,
                    Top = board_panel.Top + row * lable_height,
                    Left = board_panel.Right,
                    Text = number,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                left.MouseLeave += BackgroundPanelMouseLeaveHandler;
                right.MouseLeave += BackgroundPanelMouseLeaveHandler;

                background_panel.Controls.Add(left);
                background_panel.Controls.Add(right);
            }
        }
        /// <summary>
        /// Initialize the GUI and the engine.
        /// </summary>
        public DamebotGui()
        {
            InitializeComponent();
            ResetGame();
            GenerateEdgeLabels();
        }

        #region move selection

        /// <summary>
        /// Creates a partial move starting with the piece laying on the <paramref name="position"/>.
        /// Redraw if the <paramref name="position"/> contains a valid piece.
        /// </summary>
        private void SelectPiece(SQUARE position)
        {
            Debug.Assert(selected_squares == null);

            if (engine.IsOnMovePiece(position))
            {
                current_move = new(position);
                selected_squares = current_move.Squares;
                Draw();
            }
        }
        /// <summary>
        /// Extend a move by the selected <paramref name="position"/>.
        /// Performs the move if it is complete.
        /// Resets the move if it is invalid.
        /// </summary>
        /// <param name="position"></param>
        /// <exception cref="NullReferenceException">Throws if the move doesn't start with a valid piece.</exception>
        private void SelectMove(SQUARE position)
        {
            Debug.Assert(selected_squares != null);

            Piece piece = board[selected_squares[0]] ?? throw new NullReferenceException();

            MOVE_INFO info = engine.ValidateMove(piece, current_move, position);
            if (info.IncompleteJump)
            {
                current_move.AddJump(info.Square, info.CapturedPiece!);
                Draw();
                return;
            }
            else if (info.CompleteJump)
            {
                current_move.AddJump(info.Square, info.CapturedPiece!);
            }
            else if (info.Move)
            {
                current_move.AddMove(info.Square);
            }
            else
            {
                ResetMove();
                return;
            }

            marked_squares = null;
            engine.PerformMove(current_move);
        }
        /// <summary>
        /// Reverts the move selection and redraw board.
        /// </summary>
        void ResetMove()
        {
            selected_squares = null;
            Draw();
        }

        #endregion
        #region draw board and pieces

        /// <summary>
        /// Draws the <paramref name="square"/> on the <paramref name="board_graphics"/>.
        /// </summary>
        private void DrawSquare(Graphics board_graphics, SQUARE square)
        {
            Rectangle board_square = ComputeRectangle(square);
            Brush brush = SelectColour(square);

            board_graphics.FillRectangle(brush, board_square);
        }
        /// <summary>
        /// Draws all squares of the <paramref name="board_graphics"/>.
        /// </summary>
        /// <param name="board_graphics"></param>
        private void DrawBoardPanel(Graphics board_graphics)
        {
            for (int fa = 0; fa < board.Size; fa += 1)
            {
                for (int fb = 0; fb < board.Size; fb += 1)
                {
                    DrawSquare(board_graphics, new SQUARE(fa, fb));
                }
            }
        }
        /// <summary>
        /// Draws all pieces of the board on the <paramref name="board_graphics"/>.
        /// </summary>
        private void DrawPieces(Graphics board_graphics)
        {
            IEnumerable<Piece> all_pieces = Enumerable.Concat(
                white.GetPieces(),
                black.GetPieces()
            );
            foreach (Piece piece in all_pieces)
            {
                Rectangle rectangle = ComputeRectangle(piece.Position);
                board_graphics.DrawImage(piece.Image, rectangle);
            }
        }
        /// <summary>
        /// Draws the board and all pieces on the <paramref name="board_graphics"/>.
        /// </summary>
        private void Draw(Graphics board_graphics)
        {
            DrawBoardPanel(board_graphics);
            DrawPieces(board_graphics);
        }
        /// <summary>
        /// Draws the board and all pieces.
        /// </summary>
        private void Draw()
        {
            Draw(board_panel.CreateGraphics());
        }

        #endregion
        #region event handlers

        /// <summary>
        /// Handles the Move event of the <c>engine</c>.
        /// </summary>
        /// <param name="next_player">
        /// reference to an instance of the player on move
        /// <c>null</c> if the compueter is on the move
        /// </param>
        private void OnMoveHandler(IPlayer? next_player)
        {
            wait_for_computer = (next_player == null);
            ResetMove();
        }
        /// <summary>
        /// Handles the Mark event of the <c>engine</c>.
        /// </summary>
        /// <param name="move">The move marked by computer.</param>
        private void OnMarkHandler(MOVE move)
        {
            marked_squares = move.Squares;
            Draw();
        }
        /// <summary>
        /// Handles the GameOver event of the <c>engine</c>.
        /// </summary>
        /// <param name="player">the winner of the game</param>
        private void OnGameOverHandler(IPlayer player)
        {
            ResetMove();
            string caption = string.Format("{0} zvítězil.", player.Name);
            DialogResult result = MessageBox.Show("Chceš zahájit novou hru?", caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ResetGame();
            }
            else if (result == DialogResult.No)
            {
                Application.Exit();
            }
        }
        /// <summary>
        /// Redraws the board on the Paint event of the <c>board_panel</c>.
        /// </summary>
        private void BoardPaintHandler(object sender, PaintEventArgs e)
        {
            Draw(e.Graphics);
        }
        /// <summary>
        /// Handles the MouseClick event of the <c>board_panel</c>.
        /// </summary>
        private void BoardClickHandler(object sender, MouseEventArgs e)
        {
            if (wait_for_computer)
                return;

            SQUARE square = LocationToSquare(e.Location);
            if (selected_squares == null)
            {
                SelectPiece(square);
            }
            else
            {
                SelectMove(square);
            }
        }
        /// <summary>
        /// Handles the Click event of the <c>new_game_button</c>.
        /// </summary>
        private void NewGameClickHandler(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Chceš zahájit novou hru?", "Nová hra?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                ResetGame();
            }
        }
        /// <summary>
        /// Handles the CheckedChanged event of the <c>radio_button</c>.
        /// </summary>
        private void RadioCheckedChangedHandler(object sender, EventArgs e)
        {
            if (!(sender as RadioButton)!.Checked)
                return;

            DialogResult result = MessageBox.Show("Chceš zahájit hru s novým nastavením?", "Nová hra?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                ResetGame();
            }
        }
        /// <summary>
        /// Handles the MouseLeave event of the <c>background_panel</c>.
        /// </summary>
        private void BackgroundPanelMouseLeaveHandler(object? sender, EventArgs e)
        {
            if (wait_for_computer)
                return;

            ResetMove();
        }

        #endregion
    }
}
