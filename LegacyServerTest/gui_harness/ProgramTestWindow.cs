namespace gui_harness {
	using ShadowCreatures.Glimmer;

	class ProgramTestWindow : Main.ProgramDefault {
		FxScale mFxRainbow = new FxScale( new FxRainbow { HueCyclePixelLength = 94 / 3, HueSecondsPerCycle = 8 } );
		IGlimPixelMap mMapRainbow;
		IGlimPixelMap mEdgeLeft;
		IGlimPixelMap mEdgeRight;

		public ProgramTestWindow( GlimDevice windowDevice ) {
		}

		public override void Execute() {
		}
	}
}
