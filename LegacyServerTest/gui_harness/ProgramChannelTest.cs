namespace gui_harness {
    using ShadowCreatures.Glimmer;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    class ProgramChannelTest : Main.ProgramDefault {
		readonly DateTime mStarted = DateTime.Now;
		readonly IGlimPixelMap mMap;
		readonly Control mClr;

		public ProgramChannelTest( Control disp, IGlimPixelMap map ) {
			mClr = disp;
			Dwell = 1000;
			mMap = map;
		}

		public int Dwell { get; set; }

		public override void Execute() {
			var cctx = new FxContextContinuous( mStarted );
			double pos = 1000 * ( cctx.TimeNow.TotalSeconds % ( 3 * Dwell / 1000 ) );
			if( pos < Dwell ) {
				mClr.BackColor = Color.Red;
			}
			else if( pos < Dwell * 2 ) {
				mClr.BackColor = Color.FromArgb( 0, 0xff, 0 ); // Color.Green is not pure;
			}
			else {
				mClr.BackColor = Color.Blue;
			}
			mMap.Write( InfiniteColor( new ColorReal( mClr.BackColor ) /*{ Luminance = mLum, Saturation = mSat }*/ ) );
		}
	}
}
