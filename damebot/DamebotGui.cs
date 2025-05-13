using damebot_engine;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace damebot
{
	public partial class DamebotGui: Form
	{
		static readonly Color dark = Color.FromArgb(172, 113, 30);
		static readonly Color light = Color.FromArgb(255, 255, 240);
		static readonly Color selected = Color.FromArgb(255, 0, 0);
		static readonly Color marked = Color.FromArgb(255, 0, 0);

		private SolidBrush SelectColour(Square square)
		{
			if (square == selected_square)
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
		private Rectangle ComputeRectangle(Square square)
		{
			int left = square.X * board_panel.Size.Width / board.Size;
			int upper = board_panel.Size.Height - (square.Y + 1) * board_panel.Size.Height / board.Size;
			int width = board_panel.Size.Width / board.Size;
			int height = board_panel.Size.Height / board.Size;

			return new Rectangle(left, upper, width, height);
		}
		private Square LocationToSquare(Point point)
		{
			int x = board.Size * point.X / board_panel.Width;
			int y = board.Size - board.Size * point.Y / board_panel.Height - 1;

			return new Square(x, y);
		}

		IBoard board;
		Square? selected_square = null;

		private IBoard ResetGame()
		{
			board = new DefaultBoard();
			Debug.WriteLine("reset game");
			Draw();
			return board;
		}
		public DamebotGui()
		{
			InitializeComponent();
			board = ResetGame();
		}

		#region draw board and pieces
		private void DrawSquare(Graphics board_graphics, Square square)
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
					DrawSquare(board_graphics, new Square(fa, fb));
				}
			}
		}
		private void DrawPieces(Graphics board_graphics)
		{
			foreach (Piece piece in board.GetPieces())
			{
				Rectangle rectangle = ComputeRectangle(piece.position);
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
		private void board_panel_Paint(object sender, PaintEventArgs e)
		{
			Debug.WriteLine("paint event");
			Draw(e.Graphics);
		}
		private void board_panel_MouseClick(object sender, MouseEventArgs e)
		{
			Square square = LocationToSquare(e.Location);

			if (selected_square == null)
			{
				selected_square = square;
				Debug.WriteLine("klinutí myši");
				Draw();
			}
			else
			{
				selected_square = null;
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
			selected_square = null;
			Debug.WriteLine("myš odešla");
			Draw();
		}
		#endregion


	}
}
