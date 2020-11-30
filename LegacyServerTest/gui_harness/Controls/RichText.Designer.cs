namespace ShadowCreatures.Glimmer.Controls {
	partial class RichText {
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
			this.Content = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// Content
			// 
			this.Content.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Content.Location = new System.Drawing.Point(0, 0);
			this.Content.Name = "Content";
			this.Content.ReadOnly = true;
			this.Content.Size = new System.Drawing.Size(424, 626);
			this.Content.TabIndex = 0;
			this.Content.Text = "";
			// 
			// RichText
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.Content);
			this.Name = "RichText";
			this.Size = new System.Drawing.Size(424, 626);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox Content;
	}
}
