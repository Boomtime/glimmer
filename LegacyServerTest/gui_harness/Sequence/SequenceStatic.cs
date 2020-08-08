namespace ShadowCreatures.Glimmer {
    using ShadowCreatures.Glimmer.Effects;
    using System;
    using System.Drawing;

	class SequenceNull : SequenceDefault {
		public override void Execute() {
		}
	}

	class SequenceStatic : SequenceDefault {
		readonly IGlimPixelMap mMap;
		readonly Func<Color> fClr;
		readonly FxSolid mColour;

		public SequenceStatic( GlimManager mgr, Func<Color> clr ) {
			fClr = clr;
			mMap = mgr.CreateCompletePixelMap();
			mColour = new FxSolid();
		}

		public override void Execute() {
			var ctx = MakeCurrentContext();
			mColour.Colour = fClr();
			mMap.Write( mColour.Execute( ctx ) );
		}
	}
}
