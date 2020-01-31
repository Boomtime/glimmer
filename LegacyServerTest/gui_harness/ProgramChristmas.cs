namespace gui_harness {
	using System;
	using ShadowCreatures.Glimmer;

	class ProgramChristmas : Main.IProgram {
		readonly IGlimPixelMap mPixelMap;
		readonly FxScale mFxTwinkle = new FxScale( new FxChristmasTwinkle() );
		readonly FxScale mFxRainbow = new FxScale( new FxRainbow() );
		readonly DateTime mStarted = DateTime.Now;
		bool mColourMode = false;

		public ProgramChristmas( IGlimPacket tree, params IGlimPacket[] garland ) {
			mPixelMap = new GlimPixelMap.Factory { tree, garland }.Compile();
		}

		public double Luminance {
			set { mFxRainbow.Luminance = mFxTwinkle.Luminance = value; }
		}
		public double Saturation {
			set { mFxRainbow.Saturation = mFxTwinkle.Saturation = value; }
		}

		public void Execute() {
			var cctx = new FxContextContinuous( mStarted );
			if( mColourMode ) {
				mPixelMap.Write( mFxRainbow.Execute( cctx ) );
			}
			else {
				mPixelMap.Write( mFxTwinkle.Execute( cctx ) );
			}
		}

		public void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
			if( ButtonStatus.Up == btn ) {
				mColourMode = !mColourMode;
			}
		}
	}
}
