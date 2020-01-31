namespace gui_harness {
	using ShadowCreatures.Glimmer;
    using System;

    class ProgramRainbow : Main.ProgramDefault {
		readonly IGlimPixelMap mMap;
		readonly FxScale mFxRainbowCycle;
		readonly DateTime mStarted = DateTime.Now;

		public ProgramRainbow( IGlimPixelMap map, int hueCyclePixelLength = 100, double hueSecondsPerCycle = 12.5 ) {
			mMap = map;
			mFxRainbowCycle = new FxScale( new FxRainbow { HueCyclePixelLength = hueCyclePixelLength, HueSecondsPerCycle = hueSecondsPerCycle } );
		}

		public override void Execute() {
			mFxRainbowCycle.Luminance = Luminance;
			mFxRainbowCycle.Saturation = Saturation;
			mMap.Write( mFxRainbowCycle.Execute( new FxContextContinuous( mStarted ) ) );
		}
	}
}
