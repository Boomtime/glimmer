namespace ShadowCreatures.Glimmer {
    using ShadowCreatures.Glimmer.Effects;
    using System.Collections.Generic;

    class SequenceNull : SequenceMinimum {
		public override void FrameExecute() {
		}

		public override IEnumerable<IDeviceBinding> Devices {
			get { yield break; }
		}
	}

	class SequenceStatic : SequenceDiagnostic {
		readonly FxSolid mFxColour;
		readonly ControlVariableColour mControl;

		public SequenceStatic() {
			mFxColour = new FxSolid();
			mControl = new ControlVariableColour { Value = mFxColour.Colour };
			mControl.ValueChanged += ( s, e ) => mFxColour.Colour = mControl.Value;
			Controls.Add( "Colour", mControl );
		}

		public override void FrameExecute() {
			var ctx = MakeCurrentContext();
			PixelMap.Write( mFxColour.Execute( ctx ) );
		}
	}
}
