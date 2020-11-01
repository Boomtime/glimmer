namespace ShadowCreatures.Glimmer.Controls {
	class UIControlVariableDouble : LabelledTrackBar {

		ControlVariableDouble mVariable;

		public ControlVariableDouble Variable {
			set {
				mVariable = value;
				mVariable.ValueChanged += ( s, e ) => Value = mVariable.Value;
				Value = mVariable.Value;
				ctlTrackBar.ValueChanged += ( s, e ) => mVariable.Value = Value;
			}
		}

		double Value {
			get {
				return mVariable.Min + ( (double)ctlTrackBar.Value / (double)ctlTrackBar.Maximum ) * Range;
			}
			set {
				ctlTrackBar.Value = (int)( ctlTrackBar.Maximum * ( value - mVariable.Min ) / Range );
				ctlEdit.Text = string.Format( "{0:0.00}", Value );
			}
		}

		double Range => mVariable.Max - mVariable.Min;
	}
}
