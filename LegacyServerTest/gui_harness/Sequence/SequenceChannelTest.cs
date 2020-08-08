namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
	using System;
    using System.Drawing;

    class SequenceChannelTest : SequenceDefault {
		readonly IGlimPixelMap mMap;
		readonly Action<Color> fClr;
		readonly FxSolid mColour;

		public SequenceChannelTest( GlimManager mgr, Action<Color> clr ) {
			fClr = clr;
			mMap = mgr.CreateCompletePixelMap();
			Dwell = 1000;
			mColour = new FxSolid();
		}

		public int Dwell { get; set; }

		public override void Execute() {
			var ctx = MakeCurrentContext();
			double pos = 1000 * ( CurrentTime.TotalSeconds % ( 3 * Dwell / 1000 ) );
			if( pos < Dwell ) {
				mColour.Colour = Color.Red;
			}
			else if( pos < Dwell * 2 ) {
				mColour.Colour = Color.FromArgb( 0, 0xff, 0 ); // Color.Green is not pure;
			}
			else {
				mColour.Colour = Color.Blue;
			}
			fClr( mColour.Colour );
			mMap.Write( mColour.Execute( ctx ) );
		}
	}
}
