namespace ShadowCreatures.Glimmer.Controls {
	partial class LabelledTrackBar {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if( disposing && ( components != null ) ) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.ctlName = new System.Windows.Forms.Label();
			this.ctlEdit = new System.Windows.Forms.TextBox();
			this.ctlTrackBar = new System.Windows.Forms.TrackBar();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ctlTrackBar)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.ctlName, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.ctlEdit, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.ctlTrackBar, 1, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(187, 452);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// ctlName
			// 
			this.ctlName.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.ctlName, 3);
			this.ctlName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctlName.Location = new System.Drawing.Point(3, 0);
			this.ctlName.Name = "ctlName";
			this.ctlName.Size = new System.Drawing.Size(181, 45);
			this.ctlName.TabIndex = 0;
			this.ctlName.Text = "%name%";
			this.ctlName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ctlEdit
			// 
			this.ctlEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ctlEdit.Location = new System.Drawing.Point(43, 409);
			this.ctlEdit.Name = "ctlEdit";
			this.ctlEdit.ReadOnly = true;
			this.ctlEdit.Size = new System.Drawing.Size(100, 29);
			this.ctlEdit.TabIndex = 2;
			// 
			// ctlTrackBar
			// 
			this.ctlTrackBar.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ctlTrackBar.Location = new System.Drawing.Point(43, 48);
			this.ctlTrackBar.Maximum = 100;
			this.ctlTrackBar.Name = "ctlTrackBar";
			this.ctlTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.ctlTrackBar.Size = new System.Drawing.Size(80, 355);
			this.ctlTrackBar.TabIndex = 1;
			this.ctlTrackBar.TickFrequency = 5;
			this.ctlTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
			// 
			// LabelledTrackBar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "LabelledTrackBar";
			this.Size = new System.Drawing.Size(193, 458);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ctlTrackBar)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label ctlName;
		private System.Windows.Forms.TrackBar ctlTrackBar;
		private System.Windows.Forms.TextBox ctlEdit;
	}
}
