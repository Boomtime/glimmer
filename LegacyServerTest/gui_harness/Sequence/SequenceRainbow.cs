namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;

    class SequenceRainbow : SequenceDefault {
		readonly IGlimPixelMap mMap;
		readonly FxScale mFxRainbowCycle;

		public SequenceRainbow( GlimManager mgr, int hueCyclePixelLength = 100, double hueSecondsPerCycle = 12.5 ) {
			mMap = mgr.CreateCompletePixelMap();
			mFxRainbowCycle = new FxScale( new FxRainbow { HueCyclePixelLength = hueCyclePixelLength, HueSecondsPerCycle = hueSecondsPerCycle } );
		}

		public override void Execute() {
			mFxRainbowCycle.Luminance = Luminance.Value;
			mFxRainbowCycle.Saturation = Saturation.Value;
			mMap.Write( mFxRainbowCycle.Execute( MakeCurrentContext() ) );
		}
	}
}
