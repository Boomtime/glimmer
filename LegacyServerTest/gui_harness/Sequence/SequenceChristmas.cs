namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
	using System;
    using System.Reflection;

    class SequenceChristmas : SequenceDefault {
		readonly IGlimPixelMap mPixelMap;
		readonly FxScale mFxTwinkle = new FxScale( new FxChristmasTwinkle() );
		readonly FxScale mFxRainbow = new FxScale( new FxRainbow() );
		bool mColourMode = false;

		public SequenceChristmas( GlimManager mgr ) {
			mPixelMap = mgr.CreateCompletePixelMap();
			Luminance.ValueChanged += ( s, e ) => mFxRainbow.Luminance = mFxTwinkle.Luminance = e.Value;
			Saturation.ValueChanged += ( s, e ) => mFxRainbow.Saturation = mFxTwinkle.Saturation = e.Value;
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
