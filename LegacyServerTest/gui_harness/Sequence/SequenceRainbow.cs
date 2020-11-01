namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;

    class SequenceRainbow : SequenceDiagnostic {
		readonly FxScale mFxRainbowCycle;

		public SequenceRainbow() {
			var fxr = new FxRainbow {
				HueCyclePixelLength = 94 / 3,
				HueSecondsPerCycle = 8
			};
			mFxRainbowCycle = new FxScale( fxr );

			AddLuminanceControl( v => mFxRainbowCycle.Luminance = v );
			AddSaturationControl( v => mFxRainbowCycle.Saturation = v );

			var ictl = new ControlVariableInteger( 2, 200, 1 ) { Value = fxr.HueCyclePixelLength };
			ictl.ValueChanged += ( s, e ) => fxr.HueCyclePixelLength = ictl.Value;
			Controls.Add( "Length", ictl );

			var dctl = new ControlVariableDouble( 0.1, 30 ) { Value = fxr.HueSecondsPerCycle };
			dctl.ValueChanged += ( s, e ) => fxr.HueSecondsPerCycle = dctl.Value;
			Controls.Add( "Period", dctl );
		}

		public override void FrameExecute() {
			var ctx = MakeCurrentContext();
			PixelMap.Write( mFxRainbowCycle.Execute( ctx ) );
		}
	}
}
