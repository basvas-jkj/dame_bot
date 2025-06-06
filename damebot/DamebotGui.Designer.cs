﻿using System.Drawing;
using System.Windows.Forms;

namespace damebot
{
    partial class DamebotGui
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            board_panel = new Panel();
            white_black_radio = new RadioButton();
            white_computer_radio = new RadioButton();
            computer_black_radio = new RadioButton();
            new_game_button = new Button();
            background_panel = new Panel();
            background_panel.SuspendLayout();
            SuspendLayout();
            // 
            // board_panel
            // 
            board_panel.Location = new Point(20, 20);
            board_panel.Margin = new Padding(0);
            board_panel.Name = "board_panel";
            board_panel.Size = new Size(386, 386);
            board_panel.TabIndex = 0;
            board_panel.Paint += BoardPaintHandler;
            board_panel.MouseClick += BoardClickHandler;
            // 
            // white_black_radio
            // 
            white_black_radio.AutoSize = true;
            white_black_radio.Checked = true;
            white_black_radio.Location = new Point(481, 33);
            white_black_radio.Name = "white_black_radio";
            white_black_radio.Size = new Size(99, 23);
            white_black_radio.TabIndex = 1;
            white_black_radio.TabStop = true;
            white_black_radio.Text = "bílý × černý";
            white_black_radio.UseVisualStyleBackColor = true;
            white_black_radio.CheckedChanged += RadioCheckedChangedHandler;
            // 
            // white_computer_radio
            // 
            white_computer_radio.AutoSize = true;
            white_computer_radio.Location = new Point(481, 62);
            white_computer_radio.Name = "white_computer_radio";
            white_computer_radio.Size = new Size(109, 23);
            white_computer_radio.TabIndex = 2;
            white_computer_radio.Text = "bílý × počítač";
            white_computer_radio.UseVisualStyleBackColor = true;
            white_computer_radio.CheckedChanged += RadioCheckedChangedHandler;
            // 
            // computer_black_radio
            // 
            computer_black_radio.AutoSize = true;
            computer_black_radio.Location = new Point(481, 91);
            computer_black_radio.Name = "computer_black_radio";
            computer_black_radio.Size = new Size(121, 23);
            computer_black_radio.TabIndex = 3;
            computer_black_radio.Text = "počítač × černý";
            computer_black_radio.UseVisualStyleBackColor = true;
            computer_black_radio.CheckedChanged += RadioCheckedChangedHandler;
            // 
            // new_game_button
            // 
            new_game_button.BackColor = Color.Silver;
            new_game_button.FlatAppearance.BorderColor = Color.Black;
            new_game_button.FlatStyle = FlatStyle.Popup;
            new_game_button.ForeColor = SystemColors.ControlText;
            new_game_button.Location = new Point(481, 142);
            new_game_button.Name = "new_game_button";
            new_game_button.Padding = new Padding(5);
            new_game_button.Size = new Size(121, 45);
            new_game_button.TabIndex = 4;
            new_game_button.Text = "Nová hra";
            new_game_button.UseVisualStyleBackColor = false;
            new_game_button.Click += NewGameClickHandler;
            // 
            // background_panel
            // 
            background_panel.BackColor = SystemColors.ControlDarkDark;
            background_panel.Controls.Add(board_panel);
            background_panel.Location = new Point(12, 12);
            background_panel.Margin = new Padding(0);
            background_panel.Name = "background_panel";
            background_panel.Size = new Size(426, 426);
            background_panel.TabIndex = 5;
            background_panel.MouseLeave += BackgroundPanelMouseLeaveHandler;
            // 
            // DamebotGui
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(639, 450);
            Controls.Add(background_panel);
            Controls.Add(new_game_button);
            Controls.Add(computer_black_radio);
            Controls.Add(white_computer_radio);
            Controls.Add(white_black_radio);
            Name = "DamebotGui";
            Text = "Damebot - program na hraní dámy";
            background_panel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel board_panel;
        private RadioButton white_black_radio;
        private RadioButton white_computer_radio;
        private RadioButton computer_black_radio;
        private Button new_game_button;
        private Panel background_panel;
    }
}
