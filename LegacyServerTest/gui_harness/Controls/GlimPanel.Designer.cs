namespace ShadowCreatures.Glimmer {
	partial class GlimPanel {
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
			this.cWifi = new System.Windows.Forms.PictureBox();
			this.cName = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.cWifi)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cWifi
			// 
			this.cWifi.Location = new System.Drawing.Point(3, 3);
			this.cWifi.Name = "cWifi";
			this.cWifi.Size = new System.Drawing.Size(48, 48);
			this.cWifi.TabIndex = 0;
			this.cWifi.TabStop = false;
			// 
			// cName
			// 
			this.cName.BackColor = System.Drawing.SystemColors.Control;
			this.cName.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cName.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.cName.Location = new System.Drawing.Point(57, 3);
			this.cName.Name = "cName";
			this.cName.Size = new System.Drawing.Size(480, 48);
			this.cName.TabIndex = 2;
			this.cName.Text = "GlimSwarm-101";
			this.cName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.cWifi);
			this.panel1.Controls.Add(this.cName);
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(541, 56);
			this.panel1.TabIndex = 4;
			// 
			// GlimPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panel1);
			this.Name = "GlimPanel";
			this.Size = new System.Drawing.Size(548, 64);
			((System.ComponentModel.ISupportInitialize)(this.cWifi)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox cWifi;
		private System.Windows.Forms.Label cName;
		private System.Windows.Forms.Panel panel1;
	}
}
