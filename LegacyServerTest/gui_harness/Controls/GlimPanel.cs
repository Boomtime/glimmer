namespace ShadowCreatures.Glimmer {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using ShadowCreatures.Glimmer.Utility;
	using LineCap = System.Drawing.Drawing2D.LineCap;

	partial class GlimPanel : UserControl {
		static WifiBitmaps mBitmaps = null;

		GlimDevice mDevice;
		readonly Histogram mHistogram;
		int lastBootCount = 1; // init to 1 will prevent the leading red spike

		public GlimPanel() {
			InitializeComponent();
			mHistogram = new Histogram( cName.Size );
			cWifi.Image = BitmapForSignal( WifiRSSI.None );
		}

		Bitmap BitmapForSignal( WifiRSSI signal ) {
			if( null == mBitmaps ) {
				mBitmaps = new WifiBitmaps( cWifi.Size.Width );
			}
			return mBitmaps[signal];
		}

		public GlimDevice Device {
			set {
				mDevice = value;
				cName.Text = value.HostName;
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
				cWifi.Image = BitmapForSignal( mDevice.RSSI );
			}
		}
	}

	class WifiBitmaps {

		static readonly Color colourBackground = Color.Transparent;
		static readonly Color colourFeint = Color.LightGray;
		static readonly Color colourExcellent = Color.DarkGreen;
		static readonly Color colourGood = Color.DarkGreen;
		static readonly Color colourWeak = Color.DarkOrange;
		static readonly Color colourTerrible = Color.DarkRed;
		static readonly Color colourCross = Color.Red;

		Dictionary<WifiRSSI, Bitmap> mBitmaps;

		public WifiBitmaps( int pixelSize ) {
			PixelSize = pixelSize;
			mBitmaps = new Dictionary<WifiRSSI, Bitmap>();
			foreach( WifiRSSI lvl in Enum.GetValues( typeof( WifiRSSI ) ) ) {
				mBitmaps.Add( lvl, GenerateBitmap( pixelSize, lvl ) );
			}
		}

		public int PixelSize { get; }

		public Bitmap this[WifiRSSI signal] { get => mBitmaps[signal]; }

		static Bitmap GenerateBitmap( int pixelSize, WifiRSSI signal ) {
			var bmp = new Bitmap( pixelSize, pixelSize );
			Point origin = new Point( pixelSize / 2, pixelSize / 8 * 7 );
			Size step = new Size( pixelSize / 5, pixelSize / 5 );
			const float startAngle = 230;
			const float sweepAngle = 80;
			Rectangle makeRect( float scale ) {
				return new Rectangle(
					origin.X - (int)( scale * step.Width ), origin.Y - (int)( scale * step.Height ),
						(int)( scale * 2 * step.Width ), (int)( scale * 2 * step.Height ) );
			}
			using( var gfx = Graphics.FromImage( bmp ) ) {
				gfx.Clear( colourBackground );
				gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				using( Pen pen = new Pen( colourFeint, 2.5f ) { StartCap = LineCap.Round, EndCap = LineCap.Round } ) {
					void DrawArc( float scale ) {
						gfx.DrawArc( pen, makeRect( scale ), startAngle, sweepAngle );
					}
					// maximal arc
					if( WifiRSSI.Excellent == signal ) {
						pen.Color = colourExcellent;
					}
					DrawArc( 3.5f );
					if( WifiRSSI.Good == signal ) {
						pen.Color = colourGood;
					}
					DrawArc( 2.5f );
					if( WifiRSSI.Weak == signal ) {
						pen.Color = colourWeak;
					}
					DrawArc( 1.5f );
					if( WifiRSSI.Terrible == signal ) {
						pen.Color = colourTerrible;
					}
					DrawArc( 0.5f );
					if( WifiRSSI.None == signal ) {
						pen.Color = colourCross;
						pen.Width = 2.0f;
						gfx.DrawLine( pen, step.Width, pixelSize - step.Height, step.Width * 2, pixelSize - ( step.Height * 2 ) );
						gfx.DrawLine( pen, step.Width, pixelSize - ( step.Height * 2 ), step.Width * 2, pixelSize - step.Height );
					}
				}
			}
			return bmp;
		}
	}
}
