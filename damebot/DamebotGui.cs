using damebot_engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace damebot
{
    public partial class DamebotGui: Form
    {
        static readonly Color dark = Color.FromArgb(172, 113, 30);
        static readonly Color light = Color.FromArgb(255, 255, 240);
        static readonly Color selected = Color.FromArgb(255, 0, 0);
        static readonly Color marked = Color.FromArgb(0, 159, 255);

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
            else if ((square.X + square.Y) % 2 == 0)
            {
                return new SolidBrush(dark);
            }
            else
            {
                return new SolidBrush(light);
            }
        }
        private Rectangle ComputeRectangle(SQUARE square)
        {
            int left = square.X * board_panel.Size.Width / board.Size;
            int upper = board_panel.Size.Height - (square.Y + 1) * board_panel.Size.Height / board.Size;
            int width = board_panel.Size.Width / board.Size;
            int height = board_panel.Size.Height / board.Size;

            return new Rectangle(left, upper, width, height);
        }
        private SQUARE LocationToSquare(Point point)
        {
            int x = board.Size * point.X / board_panel.Width;
            int y = board.Size - board.Size * point.Y / board_panel.Height - 1;

            return new SQUARE(x, y);
        }

        IBoard board;
        IPlayer white;
        IPlayer black;
        IEngine engine;
        IReadOnlyList<SQUARE>? selected_squares;
        IReadOnlyList<SQUARE>? marked_squares;
        MOVE current_move;
        bool wait_for_computer;

        private void InitPlayers(bool white_is_computer, bool black_is_computer)
        {
            white = new DamePlayer(white_is_computer, PLAYER_TYPE.max, "Bílý");
            black = new DamePlayer(black_is_computer, PLAYER_TYPE.min, "Černý");
        }
        private void ResetGame()
        {
            selected_squares = null;
            marked_squares = null;

            InitPlayers(computer_black_radio.Checked, white_computer_radio.Checked);
            board = new DefaultBoard();
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
        private void GenerateEdgeLabels()
        {
            int lable_width = board_panel.Width / board.Size;
            int lable_height = board_panel.Height / board.Size;
            for (int f = 0; f < board.Size; f += 1)
            {
                string character = ((char)('a' + f)).ToString();
                Label top = new()
                {
                    Width = lable_width,
                    Height = board_panel.Top,
                    Top = 0,
                    Left = board_panel.Left + f * lable_width,
                    Text = character,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Label bottom = new()
                {
                    Width = lable_width,
                    Height = board_panel.Top,
                    Top = board_panel.Bottom,
                    Left = board_panel.Left + f * lable_width,
                    Text = character,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                top.MouseLeave += background_panel_MouseLeave;
                bottom.MouseLeave += background_panel_MouseLeave;

                background_panel.Controls.Add(top);
                background_panel.Controls.Add(bottom);
            }
            for (int f = 0; f < board.Size; f += 1)
            {
                string number = (board.Size - f).ToString();
                Label left = new()
                {
                    Width = board_panel.Left,
                    Height = lable_height,
                    Top = board_panel.Top + f * lable_height,
                    Left = 0,
                    Text = number,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Label right = new()
                {
                    Width = board_panel.Left,
                    Height = lable_height,
                    Top = board_panel.Top + f * lable_height,
                    Left = board_panel.Right,
                    Text = number,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                left.MouseLeave += background_panel_MouseLeave;
                right.MouseLeave += background_panel_MouseLeave;

                background_panel.Controls.Add(left);
                background_panel.Controls.Add(right);
            }
        }
        public DamebotGui()
        {
            InitializeComponent();
            ResetGame();
            GenerateEdgeLabels();
        }

        #region move selection

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
        void ResetMove()
        {
            selected_squares = null;
            Draw();
        }

        #endregion
        #region draw board and pieces

        private void DrawSquare(Graphics board_graphics, SQUARE square)
        {
            Rectangle board_square = ComputeRectangle(square);
            Brush brush = SelectColour(square);

            board_graphics.FillRectangle(brush, board_square);
        }
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
        private void Draw(Graphics board_graphics)
        {
            DrawBoardPanel(board_graphics);
            DrawPieces(board_graphics);
        }
        private void Draw()
        {
            Draw(board_panel.CreateGraphics());
        }

        #endregion
        #region event handlers

        private void OnMoveHandler(IPlayer? next_player)
        {
            wait_for_computer = (next_player == null);
            Draw();
        }
        private void OnMarkHandler(MOVE m)
        {
            marked_squares = m.Squares;
            ResetMove();
        }
        private void OnGameOverHandler(IPlayer p)
        {
            ResetMove();
            string caption = string.Format("{0} zvítězil.", p.Name);
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
        private void board_panel_Paint(object sender, PaintEventArgs e)
        {
            Draw(e.Graphics);
        }
        private void board_panel_MouseClick(object sender, MouseEventArgs e)
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
        private void new_game_button_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Chceš zahájit novou hru?", "Nová hra?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                ResetGame();
            }
        }
        private void radio_button_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender as RadioButton)!.Checked)
                return;

            DialogResult result = MessageBox.Show("Chceš zahájit hru s novým nastavením?", "Nová hra?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                ResetGame();
            }
        }
        private void background_panel_MouseLeave(object? sender, EventArgs e)
        {
            if (wait_for_computer)
                return;

            ResetMove();
        }

        #endregion
    }
}
