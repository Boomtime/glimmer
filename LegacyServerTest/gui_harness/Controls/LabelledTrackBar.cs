using System;
using System.Windows.Forms;

namespace ShadowCreatures.Glimmer.Controls {
	public partial class LabelledTrackBar : UserControl {
		public LabelledTrackBar() {
			InitializeComponent();
		}

		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
				ctlName.Text = value;
			}
		}
	}
}
