namespace ShadowCreatures.Glimmer.Effects {
    using System;
    using System.Collections.Generic;
	using System.Drawing;

	class FxWave : IFx {

		[Configurable]
		public Color ColourTrough { get; set; } = Color.Green;

		[Configurable]
		public Color ColourPeak { get; set; } = Color.Purple;

		[ConfigurableInteger( Min = 2, Max = 180 )]
		public int LoopPixelLength { get; set; } = 50;

		[ConfigurableInteger( Min = 5, Max = 200 )]
		public int PeriodSeconds { get; set; } = 10;

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			Color dst = ColourTrough;
			Color src = ColourPeak;
			double offset = ( ctx.TimeNow.TotalSeconds % (double)PeriodSeconds ) / (double)PeriodSeconds;
			while( true ) {
				for( int j = 0 ; j < LoopPixelLength ; j++ ) {
					double ratiostep = (double)j / (double)LoopPixelLength;
					double sa = ( 1.0 + Math.Sin( ( ratiostep + offset ) * Math.PI * 2 ) ) / 2.0;
					double omsa = 1.0 - sa;
					yield return Color.FromArgb( (int)( dst.R * omsa + src.R * sa ),
						(int)( dst.G * omsa + src.G * sa ), (int)( dst.B * omsa + src.B * sa ) );
				}
			}
		}
	}
}
namespace ShadowCreatures.Glimmer {
	using System.Collections.Generic;
	using Effects;

	class SequenceHalloween2020 : SequenceMinimum {
		readonly SequenceDeviceBasic devDining = new SequenceDeviceBasic( "Dining", "GlimSwarm-104", 94 * 2 );
		readonly SequenceDeviceBasic devLounge = new SequenceDeviceBasic( "Lounge", "GlimSwarm-103", 94 + 35 );
		readonly SequenceDeviceBasic devRoof = new SequenceDeviceBasic( "Roof", "GlimSwarm-101", 100 );
		readonly IGlimPixelMap pixWindows;
		readonly IGlimPixelMap pixRoof;
		readonly FxWave fxWave;
		readonly FxScale fxAll;
		readonly FxScale fxWindows;
		readonly ControlVariableRatio windowsLuminance = new ControlVariableRatio { Value = 0.7 };
		readonly ControlVariableInteger periodSeconds = new ControlVariableInteger( 2, 20, 1 ) { Value = 10 };

		public SequenceHalloween2020() {
			pixWindows = new GlimPixelMap.Factory { devLounge, devDining }.Compile();
			pixRoof = new GlimPixelMap.Factory { devRoof }.Compile();
			fxWave = new FxWave();
			fxAll = new FxScale( fxWave );
			fxWindows = new FxScale( fxAll );

			AddLuminanceControl( v => fxAll.Luminance = v );
			AddSaturationControl( v => fxAll.Saturation = v );
			Controls.Add( "Windows Lum", windowsLuminance );
			windowsLuminance.ValueChanged += ( s, e ) => fxWindows.Luminance = windowsLuminance.Value;
			Controls.Add( "Period (S)", periodSeconds );
			periodSeconds.ValueChanged += ( s, e ) => fxWave.PeriodSeconds = periodSeconds.Value;
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
