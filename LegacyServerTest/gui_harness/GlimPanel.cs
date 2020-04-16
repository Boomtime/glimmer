namespace ShadowCreatures.Glimmer {
	using System.Windows.Forms;
    using ShadowCreatures.Glimmer.Utility;

    partial class GlimPanel : UserControl {
		GlimDevice mDevice;
		readonly Histogram mHistogram;

		public GlimPanel() {
			InitializeComponent();
			mHistogram = new Histogram( cName.Size );
		}

		public GlimDevice Device {
			set {
				mDevice = value;
				cName.Text = value.Hostname;
				cName.Image = mHistogram.GenerateBitmap();
			}
		}

		public void UpdateStats() {
			mHistogram.PushSample( mDevice.CPU );
			cName.Image = mHistogram.GenerateBitmap();
		}
	}
}
