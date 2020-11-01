namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
    using System.Drawing;

    class SequenceChannelTest : SequenceDiagnostic {
		readonly FxSolid mColour;
		readonly ControlVariableColour mControl;

		public SequenceChannelTest() {
			Dwell = 1000;
			mColour = new FxSolid();
			mControl = new ControlVariableColour { Value = mColour.Colour };
			Controls.Add( "Colour", mControl );
		}

		public Color Colour {
			get => mControl.Value;
			set => mControl.Value = value;
		}

		public int Dwell { get; set; }

		public override void FrameExecute() {
			var ctx = MakeCurrentContext();
			double pos = 1000 * ( CurrentTime.TotalSeconds % ( 3 * Dwell / 1000 ) );
			if( pos < Dwell ) {
				mColour.Colour = Color.Red;
			}
			else if( pos < Dwell * 2 ) {
				mColour.Colour = Color.FromArgb( 0, 0xff, 0 ); // Color.Green is not pure;
			}
			else {
				mColour.Colour = Color.Blue;
			}
			Colour = mColour.Colour;
			PixelMap.Write( mColour.Execute( ctx ) );
		}
	}
}
