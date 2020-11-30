namespace ShadowCreatures.Glimmer.Controls {
    using System;
    using System.Drawing;
    using System.Windows.Forms;

	public partial class RichText : UserControl {
		public RichText() {
			InitializeComponent();
		}

		public void Clear() {
			Content.Clear();
		}

		void LimitLineCount( int maxLineCount ) {
			int remove = Content.Lines.Length - maxLineCount;
			if( remove > 0 ) {
				Content.Text.Remove( 0, Content.GetFirstCharIndexFromLine( remove + 1 ) );
			}
		}

		public int MaxLines { get; set; } = 5;

		public Font BaseFont {
			get => Content.Font;
			set => Content.Font = value;
		}

		public void AppendLine( params object[] sequence ) {
			bool prevWasColour = false;
			LimitLineCount( MaxLines );
			Content.SelectionStart = Content.Text.Length;
			void ResetFormat() {
				Content.SelectionFont = new Font( Content.SelectionFont, FontStyle.Regular );
				Content.SelectionColor = SystemColors.ControlText;
				Content.SelectionBackColor = SystemColors.Control;
			}
			ResetFormat();
			foreach( var c in sequence ) {
				switch( c ) {
					case string str:
						prevWasColour = false;
						Content.AppendText( str );
						break;
					case FontStyle style:
						prevWasColour = false;
						Content.SelectionFont = new Font( Content.SelectionFont, style );
						break;
					case Color clr:
						if( prevWasColour ) {
							Content.SelectionBackColor = clr;
						}
						else {
							prevWasColour = true;
							Content.SelectionColor = clr;
						}
						break;
					case null:
						ResetFormat();
						break;
				}
			}
			Content.AppendText( Environment.NewLine );
		}
	}
}
