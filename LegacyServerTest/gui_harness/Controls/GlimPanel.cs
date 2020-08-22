namespace ShadowCreatures.Glimmer {
    using System.Drawing;
    using System.Windows.Forms;
    using ShadowCreatures.Glimmer.Utility;
	using LineCap = System.Drawing.Drawing2D.LineCap;

	partial class GlimPanel : UserControl {
		GlimDevice mDevice;
		readonly Histogram mHistogram;
		int lastBootCount = 1; // init to 1 will prevent the leading red spike

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
			if( 0 < mDevice.BootCount ) {
				if( lastBootCount == mDevice.BootCount ) {
					mHistogram.PushSample( mDevice.CPU );
				}
				else {
					mHistogram.PushSample( 0 - mDevice.CPU );
					lastBootCount = mDevice.BootCount;
				}
				cName.Image = mHistogram.GenerateBitmap();
				cWifi.Image = new WifiBitmap { PixelSize = cWifi.Size.Width, Signal = mDevice.RSSI }.GenerateBitmap();
			}
		}
	}

	class WifiBitmap {

		readonly Color colourBackground = Color.White;
		readonly Color colourFeint = Color.LightGray;
		readonly Color colourExcellent = Color.DarkGreen;
		readonly Color colourGood = Color.DarkGreen;
		readonly Color colourWeak = Color.DarkOrange;
		readonly Color colourTerrible = Color.DarkRed;
		readonly Color colourCross = Color.Red;

		public int PixelSize { get; set; } = 32;

		public WifiRSSI Signal { get; set; } = WifiRSSI.None;

		public Bitmap GenerateBitmap() {
			var bmp = new Bitmap( PixelSize, PixelSize );
			Point origin = new Point( PixelSize / 2, PixelSize / 8 * 7 );
			Size step = new Size( PixelSize / 5, PixelSize / 5 );
			const float startAngle = 230;
			const float sweepAngle = 80;
			Rectangle makeRect( float scale ) {
				return new Rectangle(
					origin.X - (int)( scale * step.Width ), origin.Y - (int)( scale * step.Height ),
						(int)( scale * 2 * step.Width ), (int)( scale * 2 * step.Height ) );
			}
			using( var gfx = Graphics.FromImage( bmp ) ) {
				gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				using( Pen pen = new Pen( colourFeint, 2.5f ) { StartCap = LineCap.Round, EndCap = LineCap.Round } ) {
					// maximal arc
					if( WifiRSSI.Excellent == Signal ) {
						pen.Color = colourExcellent;
					}
					gfx.DrawArc( pen, makeRect( 3.5f ), startAngle, sweepAngle );
					if( WifiRSSI.Good == Signal ) {
						pen.Color = colourGood;
					}
					gfx.DrawArc( pen, makeRect( 2.5f ), startAngle, sweepAngle );
					if( WifiRSSI.Weak == Signal ) {
						pen.Color = colourWeak;
					}
					gfx.DrawArc( pen, makeRect( 1.5f ), startAngle, sweepAngle );
					if( WifiRSSI.Terrible == Signal ) {
						pen.Color = colourTerrible;
					}
					gfx.DrawArc( pen, makeRect( 0.5f ), startAngle, sweepAngle );
				}
			}
			return bmp;
		}
	}
}
