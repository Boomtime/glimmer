namespace ShadowCreatures.Glimmer.Controls {
	class UIControlVariableInteger : LabelledTrackBar {

		ControlVariableInteger mVariable;

		public ControlVariableInteger Variable {
			set {
				mVariable = value;
				mVariable.ValueChanged += ( s, e ) => ctlTrackBar.Value = mVariable.Value;
				ctlTrackBar.Minimum = mVariable.Min;
				ctlTrackBar.Maximum = mVariable.Max;
				ctlTrackBar.SmallChange = mVariable.Step;
				ctlTrackBar.LargeChange = ( mVariable.Max - mVariable.Min ) / 10;
				ctlTrackBar.Value = mVariable.Value;
				ctlTrackBar.ValueChanged += CtlTrackBar_ValueChanged;
				ctlEdit.Text = string.Format( "{0}", mVariable.Value );
			}
		}

		void CtlTrackBar_ValueChanged( object sender, System.EventArgs e ) {
			mVariable.Value = ctlTrackBar.Value;
			ctlEdit.Text = string.Format( "{0}", mVariable.Value );
		}
	}
}
