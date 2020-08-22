namespace ShadowCreatures.Glimmer {
	using System;
    using System.Collections.Generic;
    using System.Drawing;

    public enum ControlType {
		/// <summary>Int32, can have min/max/step</summary>
		Integer,
		/// <summary>shothand for double with limit 0->1, don't try to read it as an Int32</summary>
		Ratio,
		/// <summary>double, can have min/max, may accept Int32 as get/set</summary>
		Double,
		/// <summary>exactly 1 colour, always System.Drawing.Color</summary>
		Colour,
	}

	public class ControlVariableEventArgs : EventArgs {
		public ControlVariableEventArgs( dynamic v ) {
			Value = v;
		}

		public dynamic Value { get; }
	}

	public delegate void ControlVariableChanged( object s, ControlVariableEventArgs a );

	/// <summary>control variables are means of storing scalar data during sequence runtime, possibly user visible and controllable</summary>
	public abstract class ControlVariable {
		public abstract ControlType Type { get; }

		dynamic mDV;

		protected abstract dynamic Constrain( dynamic v );

		public dynamic Value {
			get {
				return mDV;
			}
			set {
				// want to force the local dv to retain the correct type
				// that is, clients who call "get" should observe appropriate type
				// clients that call "set" are forced to behave
				mDV = Constrain( value );
				var h = ValueChanged;
				h?.Invoke( this, new ControlVariableEventArgs( mDV ) );
			}
		}

		public event ControlVariableChanged ValueChanged;
	}

	public interface IControlDictionary : IReadOnlyDictionary<string, ControlVariable> {
	}

	public abstract class ControlVariableNumber : ControlVariable {
		public abstract dynamic Min { get; }

		public abstract dynamic Max { get; }
	}

	class ControlVariableInteger : ControlVariableNumber {

		public ControlVariableInteger( int min, int max, int step ) {
			Min = min;
			Max = max;
			Step = step;
		}

		public override dynamic Min { get; }

		public override dynamic Max { get; }

		public int Step { get; }

		public override ControlType Type => ControlType.Integer;

		protected override dynamic Constrain( dynamic v ) {
			return Min + ( ( Math.Min( Math.Max( (int)v, Min ), Max ) % Step ) * Step );
		}
	}

	class ControlVariableDouble : ControlVariableNumber {
		public ControlVariableDouble( double min, double max ) {
			Min = min;
			Max = max;
		}

		public override dynamic Min { get; }

		public override dynamic Max { get; }

		public override ControlType Type => ControlType.Double;

		protected override dynamic Constrain( dynamic v ) {
			return Math.Min( Math.Max( (double)v, Min ), Max );
		}
	}

	class ControlVariableRatio : ControlVariableDouble {
		public ControlVariableRatio()
			: base( 0.0, 1.0 ) {
		}
	}

	class ControlVariableColour : ControlVariable {
		public override ControlType Type => ControlType.Colour;

		protected override dynamic Constrain( dynamic v ) {
			return (Color)v;
		}
	}
}
