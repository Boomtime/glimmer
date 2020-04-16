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

		public SequenceStatic( GlimManager mgr, Func<Color> clr ) {
			fClr = clr;
			mMap = mgr.CreateCompletePixelMap();
		}

		public override void Execute() {
			mMap.Write( FxUtils.InfiniteColor( fClr() ) );
		}
	}
}
