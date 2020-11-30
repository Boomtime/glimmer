namespace ShadowCreatures.Glimmer {
	using System.Collections.Generic;
	using Effects;

	class SequenceChristmas2020 : SequenceMinimum {
		readonly SequenceDeviceBasic devDining = new SequenceDeviceBasic( "Dining", "GlimSwarm-103", 94 * 2 );
		readonly SequenceDeviceBasic devLounge = new SequenceDeviceBasic( "Lounge", "GlimSwarm-104", 94 + 35 );
		readonly SequenceDeviceBasic devRoof = new SequenceDeviceBasic( "Roof", "GlimSwarm-106", 150 );
		readonly IGlimPixelMap pixWindows;
		readonly IGlimPixelMap pixRoof;
		readonly FxCandyCane fxCandy;
		readonly FxScale fxAll;
		readonly FxScale fxWindows;
		readonly ControlVariableRatio windowsLuminance;
		readonly ControlVariableInteger periodSeconds;
		readonly ControlVariableColour stripeColour;
		readonly ControlVariableColour fillColour;

		public SequenceChristmas2020() {
			fxCandy = new FxCandyCane();
			fxAll = new FxScale( fxCandy );
			fxWindows = new FxScale( fxAll );
			windowsLuminance = new ControlVariableRatio { Value = 0.2 };
			periodSeconds = new ControlVariableInteger( 0, 15, 1 ) { Value = fxCandy.PeriodSeconds };
			stripeColour = new ControlVariableColour { Value = fxCandy.ColourStripe };
			fillColour = new ControlVariableColour { Value = fxCandy.ColourBackground };
			pixWindows = new GlimPixelMap.Factory { devLounge, devDining }.Compile();
			pixRoof = new GlimPixelMap.Factory { devRoof }.Compile();

			AddLuminanceControl( v => fxAll.Luminance = v );
			AddSaturationControl( v => fxAll.Saturation = v );
			Controls.Add( "Windows Lum", windowsLuminance );
			windowsLuminance.ValueChanged += ( s, e ) => fxWindows.Luminance = windowsLuminance.Value;
			Controls.Add( "Period (S)", periodSeconds );
			periodSeconds.ValueChanged += ( s, e ) => fxCandy.PeriodSeconds = periodSeconds.Value;
			//Controls.Add( "Colour Stripe",  );
		}

		public override IEnumerable<IDeviceBinding> Devices {
			get {
				yield return devDining;
				yield return devLounge;
				yield return devRoof;
			}
		}

		public override void FrameExecute() {
			var ctx = MakeCurrentContext();
			pixWindows.Write( fxWindows.Execute( ctx ) );
			pixRoof.Write( fxAll.Execute( ctx ) );
		}
	}
}
