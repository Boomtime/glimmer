namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;

    class SequenceChristmas : SequenceDiagnostic {
		readonly FxScale mFxTwinkle = new FxScale( new FxChristmasTwinkle() );
		readonly FxScale mFxRainbow = new FxScale( new FxRainbow() );
		bool mColourMode = false;

		public SequenceChristmas() {
			AddLuminanceControl( v => mFxRainbow.Luminance = mFxTwinkle.Luminance = v );
			AddSaturationControl( v => mFxRainbow.Saturation = mFxTwinkle.Saturation = v );
		}

		public override void FrameExecute() {
			if( mColourMode ) {
				PixelMap.Write( mFxRainbow.Execute( MakeCurrentContext() ) );
			}
			else {
				PixelMap.Write( mFxTwinkle.Execute( MakeCurrentContext() ) );
			}
		}

		public override void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
			if( ButtonStatus.Up == btn ) {
				mColourMode = !mColourMode;
			}
		}
	}
}
