namespace gui_harness
{
	partial class Main
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.cGlimList = new System.Windows.Forms.ListView();
			this.cHunt = new System.Windows.Forms.Button();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.cStarlightLum = new System.Windows.Forms.TrackBar();
			this.cLuminance = new System.Windows.Forms.TrackBar();
			this.cSaturation = new System.Windows.Forms.TrackBar();
			this.ctl_debug = new System.Windows.Forms.TextBox();
			this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.cColourPick = new System.Windows.Forms.Button();
			this.cColourSelected = new System.Windows.Forms.Label();
			this.cColourSetAll = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cFuncPartyNoGame = new System.Windows.Forms.RadioButton();
			this.cFuncPartyGame = new System.Windows.Forms.RadioButton();
			this.cFuncChannelTest = new System.Windows.Forms.RadioButton();
			this.cFuncStatic = new System.Windows.Forms.RadioButton();
			this.cFuncRainbow = new System.Windows.Forms.RadioButton();
			this.cPartyDebugShot = new System.Windows.Forms.Button();
			this.ctl_status = new System.Windows.Forms.StatusStrip();
			this.cAutoHunt = new System.Windows.Forms.CheckBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cStarlightLum)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cLuminance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cSaturation)).BeginInit();
			this.flowLayoutPanel3.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.16747F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64.83253F));
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.ctl_debug, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 1, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(18, 19);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1254, 730);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.cGlimList);
			this.flowLayoutPanel1.Controls.Add(this.cHunt);
			this.flowLayoutPanel1.Controls.Add(this.cAutoHunt);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(4, 5);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(433, 355);
			this.flowLayoutPanel1.TabIndex = 1;
			// 
			// cGlimList
			// 
			this.cGlimList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.cGlimList.Location = new System.Drawing.Point(4, 5);
			this.cGlimList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cGlimList.Name = "cGlimList";
			this.cGlimList.Size = new System.Drawing.Size(180, 262);
			this.cGlimList.TabIndex = 4;
			this.cGlimList.UseCompatibleStateImageBehavior = false;
			this.cGlimList.View = System.Windows.Forms.View.List;
			// 
			// cHunt
			// 
			this.cHunt.Location = new System.Drawing.Point(192, 5);
			this.cHunt.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cHunt.Name = "cHunt";
			this.cHunt.Size = new System.Drawing.Size(159, 59);
			this.cHunt.TabIndex = 3;
			this.cHunt.Text = "Hunt";
			this.cHunt.UseVisualStyleBackColor = true;
			this.cHunt.Click += new System.EventHandler(this.Hunt_Click);
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.Controls.Add(this.cStarlightLum);
			this.flowLayoutPanel2.Controls.Add(this.cLuminance);
			this.flowLayoutPanel2.Controls.Add(this.cSaturation);
			this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel2.Location = new System.Drawing.Point(4, 370);
			this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(433, 355);
			this.flowLayoutPanel2.TabIndex = 3;
			// 
			// cStarlightLum
			// 
			this.cStarlightLum.Location = new System.Drawing.Point(4, 5);
			this.cStarlightLum.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cStarlightLum.Maximum = 1000;
			this.cStarlightLum.Name = "cStarlightLum";
			this.cStarlightLum.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.cStarlightLum.Size = new System.Drawing.Size(90, 336);
			this.cStarlightLum.TabIndex = 3;
			this.cStarlightLum.TickFrequency = 100;
			this.cStarlightLum.Value = 500;
			// 
			// cLuminance
			// 
			this.cLuminance.Location = new System.Drawing.Point(102, 5);
			this.cLuminance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cLuminance.Maximum = 1000;
			this.cLuminance.Name = "cLuminance";
			this.cLuminance.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.cLuminance.Size = new System.Drawing.Size(90, 336);
			this.cLuminance.TabIndex = 2;
			this.cLuminance.TickFrequency = 100;
			this.cLuminance.Value = 500;
			// 
			// cSaturation
			// 
			this.cSaturation.Location = new System.Drawing.Point(200, 5);
			this.cSaturation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cSaturation.Maximum = 1000;
			this.cSaturation.Name = "cSaturation";
			this.cSaturation.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.cSaturation.Size = new System.Drawing.Size(90, 336);
			this.cSaturation.TabIndex = 4;
			this.cSaturation.TickFrequency = 100;
			this.cSaturation.Value = 1000;
			// 
			// ctl_debug
			// 
			this.ctl_debug.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctl_debug.Location = new System.Drawing.Point(445, 370);
			this.ctl_debug.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.ctl_debug.Multiline = true;
			this.ctl_debug.Name = "ctl_debug";
			this.ctl_debug.ReadOnly = true;
			this.ctl_debug.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ctl_debug.Size = new System.Drawing.Size(805, 355);
			this.ctl_debug.TabIndex = 4;
			// 
			// flowLayoutPanel3
			// 
			this.flowLayoutPanel3.Controls.Add(this.cColourPick);
			this.flowLayoutPanel3.Controls.Add(this.cColourSelected);
			this.flowLayoutPanel3.Controls.Add(this.cColourSetAll);
			this.flowLayoutPanel3.Controls.Add(this.groupBox1);
			this.flowLayoutPanel3.Controls.Add(this.cPartyDebugShot);
			this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel3.Location = new System.Drawing.Point(445, 5);
			this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(805, 355);
			this.flowLayoutPanel3.TabIndex = 5;
			// 
			// cColourPick
			// 
			this.cColourPick.Location = new System.Drawing.Point(4, 5);
			this.cColourPick.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cColourPick.Name = "cColourPick";
			this.cColourPick.Size = new System.Drawing.Size(220, 56);
			this.cColourPick.TabIndex = 0;
			this.cColourPick.Text = "Pick Colour";
			this.cColourPick.UseVisualStyleBackColor = true;
			this.cColourPick.Click += new System.EventHandler(this.ColourPick_Click);
			// 
			// cColourSelected
			// 
			this.cColourSelected.BackColor = System.Drawing.Color.White;
			this.cColourSelected.Location = new System.Drawing.Point(232, 0);
			this.cColourSelected.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.cColourSelected.Name = "cColourSelected";
			this.cColourSelected.Size = new System.Drawing.Size(75, 78);
			this.cColourSelected.TabIndex = 2;
			// 
			// cColourSetAll
			// 
			this.cColourSetAll.Location = new System.Drawing.Point(315, 5);
			this.cColourSetAll.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cColourSetAll.Name = "cColourSetAll";
			this.cColourSetAll.Size = new System.Drawing.Size(111, 56);
			this.cColourSetAll.TabIndex = 1;
			this.cColourSetAll.Text = "Set all";
			this.cColourSetAll.UseVisualStyleBackColor = true;
			this.cColourSetAll.Click += new System.EventHandler(this.ColourResend_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cFuncPartyNoGame);
			this.groupBox1.Controls.Add(this.cFuncPartyGame);
			this.groupBox1.Controls.Add(this.cFuncChannelTest);
			this.groupBox1.Controls.Add(this.cFuncStatic);
			this.groupBox1.Controls.Add(this.cFuncRainbow);
			this.groupBox1.Location = new System.Drawing.Point(434, 5);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox1.Size = new System.Drawing.Size(272, 262);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Function";
			// 
			// cFuncPartyNoGame
			// 
			this.cFuncPartyNoGame.AutoSize = true;
			this.cFuncPartyNoGame.Location = new System.Drawing.Point(9, 195);
			this.cFuncPartyNoGame.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cFuncPartyNoGame.Name = "cFuncPartyNoGame";
			this.cFuncPartyNoGame.Size = new System.Drawing.Size(180, 29);
			this.cFuncPartyNoGame.TabIndex = 9;
			this.cFuncPartyNoGame.TabStop = true;
			this.cFuncPartyNoGame.Text = "party no game";
			this.cFuncPartyNoGame.UseVisualStyleBackColor = true;
			// 
			// cFuncPartyGame
			// 
			this.cFuncPartyGame.AutoSize = true;
			this.cFuncPartyGame.Location = new System.Drawing.Point(9, 156);
			this.cFuncPartyGame.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cFuncPartyGame.Name = "cFuncPartyGame";
			this.cFuncPartyGame.Size = new System.Drawing.Size(150, 29);
			this.cFuncPartyGame.TabIndex = 8;
			this.cFuncPartyGame.TabStop = true;
			this.cFuncPartyGame.Text = "party game";
			this.cFuncPartyGame.UseVisualStyleBackColor = true;
			// 
			// cFuncChannelTest
			// 
			this.cFuncChannelTest.AutoSize = true;
			this.cFuncChannelTest.Location = new System.Drawing.Point(9, 117);
			this.cFuncChannelTest.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cFuncChannelTest.Name = "cFuncChannelTest";
			this.cFuncChannelTest.Size = new System.Drawing.Size(160, 29);
			this.cFuncChannelTest.TabIndex = 7;
			this.cFuncChannelTest.TabStop = true;
			this.cFuncChannelTest.Text = "channel test";
			this.cFuncChannelTest.UseVisualStyleBackColor = true;
			// 
			// cFuncStatic
			// 
			this.cFuncStatic.AutoSize = true;
			this.cFuncStatic.Checked = true;
			this.cFuncStatic.Location = new System.Drawing.Point(9, 33);
			this.cFuncStatic.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cFuncStatic.Name = "cFuncStatic";
			this.cFuncStatic.Size = new System.Drawing.Size(94, 29);
			this.cFuncStatic.TabIndex = 5;
			this.cFuncStatic.TabStop = true;
			this.cFuncStatic.Text = "static";
			this.cFuncStatic.UseVisualStyleBackColor = true;
			// 
			// cFuncRainbow
			// 
			this.cFuncRainbow.AutoSize = true;
			this.cFuncRainbow.Location = new System.Drawing.Point(9, 75);
			this.cFuncRainbow.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cFuncRainbow.Name = "cFuncRainbow";
			this.cFuncRainbow.Size = new System.Drawing.Size(174, 29);
			this.cFuncRainbow.TabIndex = 6;
			this.cFuncRainbow.TabStop = true;
			this.cFuncRainbow.Text = "rainbow cycle";
			this.cFuncRainbow.UseVisualStyleBackColor = true;
			// 
			// cPartyDebugShot
			// 
			this.cPartyDebugShot.Location = new System.Drawing.Point(3, 275);
			this.cPartyDebugShot.Name = "cPartyDebugShot";
			this.cPartyDebugShot.Size = new System.Drawing.Size(151, 58);
			this.cPartyDebugShot.TabIndex = 8;
			this.cPartyDebugShot.Text = "Party Game Debug Shot";
			this.cPartyDebugShot.UseVisualStyleBackColor = true;
			this.cPartyDebugShot.Click += new System.EventHandler(this.cPartyDebugShot_Click);
			// 
			// ctl_status
			// 
			this.ctl_status.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.ctl_status.Location = new System.Drawing.Point(0, 745);
			this.ctl_status.Name = "ctl_status";
			this.ctl_status.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
			this.ctl_status.Size = new System.Drawing.Size(1290, 22);
			this.ctl_status.TabIndex = 1;
			this.ctl_status.Text = "statusStrip1";
			// 
			// cAutoHunt
			// 
			this.cAutoHunt.AutoSize = true;
			this.cAutoHunt.Location = new System.Drawing.Point(3, 275);
			this.cAutoHunt.Name = "cAutoHunt";
			this.cAutoHunt.Size = new System.Drawing.Size(137, 29);
			this.cAutoHunt.TabIndex = 5;
			this.cAutoHunt.Text = "Auto-hunt";
			this.cAutoHunt.UseVisualStyleBackColor = true;
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1290, 767);
			this.Controls.Add(this.ctl_status);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "Main";
			this.Text = "Alpha Crucis";
			this.Load += new System.EventHandler(this.Main_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.cStarlightLum)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cLuminance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cSaturation)).EndInit();
			this.flowLayoutPanel3.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.StatusStrip ctl_status;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button cColourSetAll;
		private System.Windows.Forms.Label cColourSelected;
		private System.Windows.Forms.TrackBar cLuminance;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.TrackBar cStarlightLum;
		private System.Windows.Forms.TrackBar cSaturation;
		private System.Windows.Forms.TextBox ctl_debug;
		private System.Windows.Forms.Button cColourPick;
		private System.Windows.Forms.Button cHunt;
		private System.Windows.Forms.RadioButton cFuncStatic;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
		private System.Windows.Forms.RadioButton cFuncRainbow;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ListView cGlimList;
		private System.Windows.Forms.RadioButton cFuncChannelTest;
		private System.Windows.Forms.RadioButton cFuncPartyGame;
		private System.Windows.Forms.RadioButton cFuncPartyNoGame;
		private System.Windows.Forms.Button cPartyDebugShot;
		private System.Windows.Forms.CheckBox cAutoHunt;
	}
}

