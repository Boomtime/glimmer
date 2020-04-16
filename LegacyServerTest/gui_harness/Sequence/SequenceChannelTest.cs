namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
	using System;
    using System.Drawing;

    class SequenceChannelTest : SequenceDefault {
		readonly IGlimPixelMap mMap;
		readonly Action<Color> fClr;

		public SequenceChannelTest( GlimManager mgr, Action<Color> clr ) {
			fClr = clr;
			mMap = mgr.CreateCompletePixelMap();
			Dwell = 1000;
		}

		public int Dwell { get; set; }

		public override void Execute() {
			double pos = 1000 * ( CurrentTime.TotalSeconds % ( 3 * Dwell / 1000 ) );
			Color clr;
			if( pos < Dwell ) {
				clr = Color.Red;
			}
			else if( pos < Dwell * 2 ) {
				clr = Color.FromArgb( 0, 0xff, 0 ); // Color.Green is not pure;
			}
			else {
				clr = Color.Blue;
			}
			fClr( clr );
			mMap.Write( FxUtils.InfiniteColor( clr ) );
		}
	}
}
