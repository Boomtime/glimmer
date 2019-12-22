namespace gui_harness {
	using System;
	using ShadowCreatures.Glimmer;

	class ProgramChristmas {
		readonly FxChristmasTwinkle mFxTwinkle;
		readonly FxBase mFxRainbow;
		readonly FxScale mFxScale;
		readonly DateTime mStart = DateTime.Now;
		bool mColourMode = false;

		public ProgramChristmas( GlimDevice tree, GlimDevice garland ) {
			var map = new GlimDeviceMap { { garland }, { tree } }.Compile();
			mFxTwinkle = new FxChristmasTwinkle( map );
			mFxRainbow = new FxRainbow( map );
			mFxScale = new FxScale( map );
		}

		public void Execute( double lum, double sat ) {
			var cctx = new FxContextUnbounded( mStart );
			if( mColourMode ) {
				mFxRainbow.Execute( cctx );
			}
			else {
				mFxTwinkle.Execute( cctx );
			}
			mFxScale.LuminanceScale = lum;
			mFxScale.SaturationScale = sat;
			mFxScale.Execute( cctx );
		}

		internal void ButtonStateChanged( ButtonStatus buttonStatus ) {
			if( ButtonStatus.Up == buttonStatus ) {
				mColourMode = !mColourMode;
			}
		}
	}
}
