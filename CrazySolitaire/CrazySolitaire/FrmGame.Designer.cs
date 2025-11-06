namespace CrazySolitaire
{
    partial class FrmGame
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
            pbStock = new PictureBox();
            panTalon = new Panel();
            panFoundationStack_Clubs = new Panel();
            panFoundationStack_Hearts = new Panel();
            panFoundationStack_Spades = new Panel();
            panFoundationStack_Diamonds = new Panel();
            panTableauStack_0 = new Panel();
            panTableauStack_1 = new Panel();
            panTableauStack_2 = new Panel();
            panTableauStack_3 = new Panel();
            panTableauStack_4 = new Panel();
            panTableauStack_5 = new Panel();
            panTableauStack_6 = new Panel();
            Timer = new Label();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)pbStock).BeginInit();
            SuspendLayout();
            // 
            // pbStock
            // 
            pbStock.BackgroundImage = Properties.Resources.back_green;
            pbStock.BackgroundImageLayout = ImageLayout.Stretch;
            pbStock.BorderStyle = BorderStyle.Fixed3D;
            pbStock.Location = new Point(40, 27);
            pbStock.Name = "pbStock";
            pbStock.Size = new Size(90, 126);
            pbStock.TabIndex = 0;
            pbStock.TabStop = false;
            pbStock.Click += pbStock_Click;
            // 
            // panTalon
            // 
            panTalon.Location = new Point(160, 27);
            panTalon.Name = "panTalon";
            panTalon.Size = new Size(166, 126);
            panTalon.TabIndex = 1;
            // 
            // panFoundationStack_Clubs
            // 
            panFoundationStack_Clubs.AllowDrop = true;
            panFoundationStack_Clubs.BackgroundImage = Properties.Resources.FoundationStackBg_Clubs;
            panFoundationStack_Clubs.BackgroundImageLayout = ImageLayout.Stretch;
            panFoundationStack_Clubs.BorderStyle = BorderStyle.FixedSingle;
            panFoundationStack_Clubs.Location = new Point(427, 26);
            panFoundationStack_Clubs.Name = "panFoundationStack_Clubs";
            panFoundationStack_Clubs.Size = new Size(100, 126);
            panFoundationStack_Clubs.TabIndex = 2;
            // 
            // panFoundationStack_Hearts
            // 
            panFoundationStack_Hearts.BackgroundImage = Properties.Resources.FoundationStackBg_Hearts;
            panFoundationStack_Hearts.BackgroundImageLayout = ImageLayout.Stretch;
            panFoundationStack_Hearts.BorderStyle = BorderStyle.FixedSingle;
            panFoundationStack_Hearts.Location = new Point(556, 27);
            panFoundationStack_Hearts.Name = "panFoundationStack_Hearts";
            panFoundationStack_Hearts.Size = new Size(100, 126);
            panFoundationStack_Hearts.TabIndex = 3;
            // 
            // panFoundationStack_Spades
            // 
            panFoundationStack_Spades.BackgroundImage = Properties.Resources.FoundationStackBg_Spades;
            panFoundationStack_Spades.BackgroundImageLayout = ImageLayout.Stretch;
            panFoundationStack_Spades.BorderStyle = BorderStyle.FixedSingle;
            panFoundationStack_Spades.Location = new Point(685, 27);
            panFoundationStack_Spades.Name = "panFoundationStack_Spades";
            panFoundationStack_Spades.Size = new Size(100, 126);
            panFoundationStack_Spades.TabIndex = 3;
            // 
            // panFoundationStack_Diamonds
            // 
            panFoundationStack_Diamonds.BackgroundImage = Properties.Resources.FoundationStackBg_Diamonds;
            panFoundationStack_Diamonds.BackgroundImageLayout = ImageLayout.Stretch;
            panFoundationStack_Diamonds.BorderStyle = BorderStyle.FixedSingle;
            panFoundationStack_Diamonds.Location = new Point(814, 27);
            panFoundationStack_Diamonds.Name = "panFoundationStack_Diamonds";
            panFoundationStack_Diamonds.Size = new Size(100, 126);
            panFoundationStack_Diamonds.TabIndex = 3;
            // 
            // panTableauStack_0
            // 
            panTableauStack_0.AllowDrop = true;
            panTableauStack_0.Location = new Point(40, 254);
            panTableauStack_0.Name = "panTableauStack_0";
            panTableauStack_0.Size = new Size(101, 408);
            panTableauStack_0.TabIndex = 4;
            // 
            // panTableauStack_1
            // 
            panTableauStack_1.Location = new Point(169, 254);
            panTableauStack_1.Name = "panTableauStack_1";
            panTableauStack_1.Size = new Size(101, 408);
            panTableauStack_1.TabIndex = 5;
            // 
            // panTableauStack_2
            // 
            panTableauStack_2.Location = new Point(298, 254);
            panTableauStack_2.Name = "panTableauStack_2";
            panTableauStack_2.Size = new Size(101, 408);
            panTableauStack_2.TabIndex = 5;
            // 
            // panTableauStack_3
            // 
            panTableauStack_3.Location = new Point(427, 254);
            panTableauStack_3.Name = "panTableauStack_3";
            panTableauStack_3.Size = new Size(101, 408);
            panTableauStack_3.TabIndex = 5;
            // 
            // panTableauStack_4
            // 
            panTableauStack_4.Location = new Point(556, 254);
            panTableauStack_4.Name = "panTableauStack_4";
            panTableauStack_4.Size = new Size(101, 408);
            panTableauStack_4.TabIndex = 5;
            // 
            // panTableauStack_5
            // 
            panTableauStack_5.Location = new Point(685, 254);
            panTableauStack_5.Name = "panTableauStack_5";
            panTableauStack_5.Size = new Size(101, 408);
            panTableauStack_5.TabIndex = 5;
            // 
            // panTableauStack_6
            // 
            panTableauStack_6.Location = new Point(814, 254);
            panTableauStack_6.Name = "panTableauStack_6";
            panTableauStack_6.Size = new Size(101, 408);
            panTableauStack_6.TabIndex = 5;
            // 
            // Timer
            // 
            Timer.AutoSize = true;
            Timer.BackColor = Color.Transparent;
            Timer.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Timer.Location = new Point(344, 27);
            Timer.Name = "Timer";
            Timer.Size = new Size(63, 32);
            Timer.TabIndex = 6;
            Timer.Text = "0:00";
            // 
            // button1
            // 
            button1.BackgroundImage = Properties.Resources.undo_button;
            button1.BackgroundImageLayout = ImageLayout.Stretch;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.ForeColor = Color.Transparent;
            button1.Location = new Point(344, 59);
            button1.Margin = new Padding(0);
            button1.Name = "button1";
            button1.Size = new Size(38, 35);
            button1.TabIndex = 7;
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // FrmGame
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(0, 64, 0);
            ClientSize = new Size(976, 674);
            Controls.Add(button1);
            Controls.Add(Timer);
            Controls.Add(panTableauStack_6);
            Controls.Add(panTableauStack_5);
            Controls.Add(panTableauStack_4);
            Controls.Add(panTableauStack_3);
            Controls.Add(panTableauStack_2);
            Controls.Add(panTableauStack_1);
            Controls.Add(panTableauStack_0);
            Controls.Add(panFoundationStack_Diamonds);
            Controls.Add(panFoundationStack_Spades);
            Controls.Add(panFoundationStack_Hearts);
            Controls.Add(panFoundationStack_Clubs);
            Controls.Add(panTalon);
            Controls.Add(pbStock);
            Name = "FrmGame";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Crazy Solitaire";
            FormClosing += FrmGame_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pbStock).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pbStock;
        private Panel panTalon;
        private Panel panFoundationStack_Clubs;
        private Panel panFoundationStack_Hearts;
        private Panel panFoundationStack_Spades;
        private Panel panFoundationStack_Diamonds;
        private Panel panTableauStack_0;
        private Panel panTableauStack_1;
        private Panel panTableauStack_2;
        private Panel panTableauStack_3;
        private Panel panTableauStack_4;
        private Panel panTableauStack_5;
        private Panel panTableauStack_6;
        private Label Timer;
        private Button button1;
    }
}
