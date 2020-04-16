namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
	using System;

	class SequenceChristmas : SequenceDefault {
		readonly IGlimPixelMap mPixelMap;
		readonly FxScale mFxTwinkle = new FxScale( new FxChristmasTwinkle() );
		readonly FxScale mFxRainbow = new FxScale( new FxRainbow() );
		bool mColourMode = false;

		public SequenceChristmas( GlimManager mgr ) {
			mPixelMap = mgr.CreateCompletePixelMap();
		}

		public override double Luminance {
			set { mFxRainbow.Luminance = mFxTwinkle.Luminance = value; }
		}
		public override double Saturation {
			set { mFxRainbow.Saturation = mFxTwinkle.Saturation = value; }
		}

		public override void Execute() {
			if( mColourMode ) {
				mPixelMap.Write( mFxRainbow.Execute( MakeCurrentContext() ) );
			}
			else {
				mPixelMap.Write( mFxTwinkle.Execute( MakeCurrentContext() ) );
			}
		}

		public override void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
			if( ButtonStatus.Up == btn ) {
				mColourMode = !mColourMode;
			}
		}
	}
}
