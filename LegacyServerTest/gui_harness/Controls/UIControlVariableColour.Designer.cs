namespace ShadowCreatures.Glimmer.Controls {
	partial class UIControlVariableColour {
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
			this.ctlName = new System.Windows.Forms.Label();
			this.ctlEdit = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.ctlValue = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ctlName
			// 
			this.ctlName.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.ctlName, 3);
			this.ctlName.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ctlName.Location = new System.Drawing.Point(3, 0);
			this.ctlName.Name = "ctlName";
			this.ctlName.Size = new System.Drawing.Size(187, 40);
			this.ctlName.TabIndex = 0;
			this.ctlName.Text = "%name%";
			this.ctlName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ctlEdit
			// 
			this.ctlEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ctlEdit.Location = new System.Drawing.Point(42, 146);
			this.ctlEdit.Name = "ctlEdit";
			this.ctlEdit.ReadOnly = true;
			this.ctlEdit.Size = new System.Drawing.Size(108, 29);
			this.ctlEdit.TabIndex = 2;
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
			this.tableLayoutPanel1.Controls.Add(this.ctlValue, 1, 1);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(193, 183);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// ctlValue
			// 
			this.ctlValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ctlValue.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ctlValue.Location = new System.Drawing.Point(42, 43);
			this.ctlValue.Name = "ctlValue";
			this.ctlValue.Size = new System.Drawing.Size(108, 97);
			this.ctlValue.TabIndex = 3;
			// 
			// ColourVariablePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "ColourVariablePanel";
			this.Size = new System.Drawing.Size(199, 189);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label ctlName;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TextBox ctlEdit;
		private System.Windows.Forms.Panel ctlValue;
	}
}
