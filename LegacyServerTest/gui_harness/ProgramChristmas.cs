namespace gui_harness {
	using System;
	using ShadowCreatures.Glimmer;

	class ProgramChristmas {
		readonly IGlimPixelMap mPixelMap;
		readonly FxScale mFxTwinkle;
		readonly FxScale mFxRainbow;
		readonly DateTime mStart = DateTime.Now;
		bool mColourMode = false;

		public ProgramChristmas( GlimDevice tree, params GlimDevice[] garland ) {
			mPixelMap = new GlimDeviceMap { tree, garland }.Compile();
			mFxTwinkle = new FxScale( new FxChristmasTwinkle() );
			mFxTwinkle.Initialize( mPixelMap.PixelCount );
			mFxRainbow = new FxScale( new FxRainbow() );
			mFxRainbow.Initialize( mPixelMap.PixelCount );
		}

		public void Execute( double lum, double sat ) {
			var cctx = new FxContextUnbounded( mStart );
			if( mColourMode ) {
				mFxRainbow.LuminanceScale = lum;
				mFxRainbow.SaturationScale = sat;
				mPixelMap.Write( mFxRainbow.Execute( cctx ) );
			}
			else {
				mFxTwinkle.LuminanceScale = lum;
				mFxTwinkle.SaturationScale = sat;
				mPixelMap.Write( mFxTwinkle.Execute( cctx ) );
			}
		}

		internal void ButtonStateChanged( ButtonStatus buttonStatus ) {
			if( ButtonStatus.Up == buttonStatus ) {
				mColourMode = !mColourMode;
			}
		}
	}
}
