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
		static readonly Color marked = Color.FromArgb(255, 0, 0);

		private SolidBrush SelectColour(SQUARE square)
		{
			if (selected_squares.Contains(square))
			{
				return new SolidBrush(selected);
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
		Player white;
		Player black;
		IEngine engine;
		List<SQUARE> selected_squares = new();

		private void InitPlayers(bool white_computer, bool black_computer)
		{
			white = (white_computer) ? new DamebotDefaultPlayer() : new RealPlayer();
			black = (black_computer) ? new DamebotDefaultPlayer() : new RealPlayer();
		}
		private void ResetGame()
		{
			InitPlayers(computer_black_radio.Checked, white_computer_radio.Checked);
			board = new DefaultBoard();
			board.GenerateInitialPieces(white, black);
			engine = new DameEngine(board, white, black);
			engine.OnMove += OnMoveHandler;

			Draw();
		}
		public DamebotGui()
		{
			InitializeComponent();
			ResetGame();
		}

		#region move selection
		private void SelectPiece(SQUARE position)
		{
			Debug.Assert(selected_squares.Count == 0);
			if (engine.IsOnMovePiece(position))
			{
				selected_squares.Add(position);
				Draw();
			}
		}
		private void SelectMove(SQUARE position)
		{
			Piece piece = board[selected_squares[0]] ?? throw new NullReferenceException();
			SQUARE original = selected_squares[^1];

			MOVE_TYPE type = engine.ValidateMove(piece, original, position);
			if (type == MOVE_TYPE.incomplete_jump)
			{
				selected_squares.Add(position);
			}
			else if (type == MOVE_TYPE.invalid)
			{
				selected_squares.Clear();
			}
			else
			{
				List<SQUARE> move = selected_squares;
				selected_squares = new List<SQUARE>();

				move.Add(position);
				engine.PerformMove(move, type == MOVE_TYPE.jump);
			}
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
				board_graphics.DrawImage(piece.image, rectangle);
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
		private void OnMoveHandler()
		{
			Draw();
		}
		private void board_panel_Paint(object sender, PaintEventArgs e)
		{
			Draw(e.Graphics);
		}
		private void board_panel_MouseClick(object sender, MouseEventArgs e)
		{
			SQUARE square = LocationToSquare(e.Location);
			if (selected_squares.Count == 0)
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
		private void board_panel_MouseLeave(object sender, EventArgs e)
		{
			selected_squares.Clear();
			Draw();
		}
		#endregion
	}
}
