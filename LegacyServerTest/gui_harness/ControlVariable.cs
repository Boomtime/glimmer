namespace ShadowCreatures.Glimmer {
	using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    public enum ControlType {
		/// <summary>bool</summary>
		Boolean,
		/// <summary>Int32, can have min/max/step</summary>
		Integer,
		/// <summary>shothand for double with limit 0->1</summary>
		Ratio,
		/// <summary>double, can have min/max, may accept Int32 as get/set</summary>
		Double,
		/// <summary>exactly 1 colour, always System.Drawing.Color</summary>
		Colour,
	}

	public interface IControlVariable {
		ControlType Type { get; }

		dynamic Value { get; }

		event EventHandler ValueChanged;

		Control MakeUIControl( string label );
	}

	public interface IControlVariable<T> : IControlVariable {
		new T Value { get; set; }
	}

	public abstract class ControlVariable<T> : IControlVariable<T> {
		public abstract ControlType Type { get; }

		T mValue;

		public T Value {
			get {
				return mValue;
			}
			set {
				// want to force the local dv to retain the correct type
				// that is, clients who call "get" should observe appropriate type
				// clients that call "set" are forced to behave
				mValue = Constrain( value );
				var h = ValueChanged;
				h?.Invoke( this, EventArgs.Empty );
			}
		}

		dynamic IControlVariable.Value => Value;

		public event EventHandler ValueChanged;

		protected virtual T Constrain( T v ) {
			return v;
		}

		public abstract Control MakeUIControl( string label );
	}

	public interface IControlDictionary : IReadOnlyDictionary<string, IControlVariable> {
	}

	class ControlVariableBoolean : ControlVariable<bool> {
		public override ControlType Type => ControlType.Boolean;

		public override Control MakeUIControl( string label ) {
			throw new NotImplementedException();
		}
	}

	public abstract class ControlVariableNumber<T> : ControlVariable<T> where T : struct {
		public abstract T Min { get; }

		public abstract T Max { get; }
	}

	class ControlVariableInteger : ControlVariableNumber<int> {

		public ControlVariableInteger( int min, int max, int step ) {
			Min = min;
			Max = max;
			Step = step;
		}

		public override ControlType Type => ControlType.Integer;

		public override int Min { get; }

		public override int Max { get; }

		public int Step { get; }

		protected override int Constrain( int v ) {
			return Min + ( ( ( Math.Min( Math.Max( v, Min ), Max ) - Min ) / Step ) * Step );
		}

		public override Control MakeUIControl( string label ) {
			return new Controls.UIControlVariableInteger { Text = label, Variable = this };
		}
	}

	class ControlVariableDouble : ControlVariableNumber<double> {
		public ControlVariableDouble( double min, double max ) {
			Min = min;
			Max = max;
		}

		public override double Min { get; }

		public override double Max { get; }

		public override ControlType Type => ControlType.Double;

		protected override double Constrain( double v ) {
			return Math.Min( Math.Max( v, Min ), Max );
		}

		public override Control MakeUIControl( string label ) {
			return new Controls.UIControlVariableDouble { Text = label, Variable = this };
		}
	}

	class ControlVariableRatio : ControlVariableDouble {
		public override ControlType Type => ControlType.Ratio;

		public ControlVariableRatio()
			: base( 0.0, 1.0 ) {
		}
	}

	class ControlVariableColour : ControlVariable<Color> {
		public override ControlType Type => ControlType.Colour;

		public override Control MakeUIControl( string label ) {
			return new Controls.UIControlVariableColour { Text = label, Variable = this };
		}
	}
}
