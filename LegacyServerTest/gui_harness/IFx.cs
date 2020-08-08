namespace ShadowCreatures.Glimmer {
	using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;

    public enum Adjustment {
		[Description( "Allows adjustment of the property value prior to runtime." )]
		Initialization,
		[Description( "Allows dynamic adjustment of the property value during runtime." )]
		Dynamic,
	}

	[AttributeUsage( AttributeTargets.Property )]
	public class ConfigurableAttribute : Attribute {
		public Adjustment Adjustment { get; set; } = Adjustment.Initialization;

		public virtual void SetObjectProperty( object dst, PropertyInfo pi, dynamic value, bool throwOnError ) {
			pi.SetValue( dst, value );
		}
	}

	class ConfigurableDoubleAttribute : ConfigurableAttribute {
		public double Min { get; set; } = double.MinValue;
		public double Max { get; set; } = double.MaxValue;

		public override void SetObjectProperty( object dst, PropertyInfo pi, dynamic value, bool throwOnError ) {
			try {
				double vorig = (double)value;
				double vclamp = Math.Min( Math.Max( vorig, Min ), Max );
				if( throwOnError && vorig != vclamp ) {
					throw new ArgumentOutOfRangeException( pi.Name, string.Format( "Expecting value between {0} and {1}", Min, Max ) );
				}
				pi.SetValue( dst, vclamp );
			}
			catch( Exception ) {
				if( throwOnError ) {
					throw;
				}
			}
		}
	}

	class ConfigurableRatioAttribute : ConfigurableDoubleAttribute {
		public ConfigurableRatioAttribute() {
			Min = 0.0;
			Max = 1.0;
		}
	}

	class ConfigurableIntegerAttribute : ConfigurableAttribute {
		public int Min { get; set; } = int.MinValue;
		public int Max { get; set; } = int.MaxValue;
		public int Step { get; set; } = 1;

		public override void SetObjectProperty( object dst, PropertyInfo pi, dynamic value, bool throwOnError ) {
			try {
				int vorig = (int)value;
				int vclamp = Math.Min( Math.Max( vorig, Min ), Max );
				if( throwOnError && vorig != vclamp ) {
					throw new ArgumentOutOfRangeException( pi.Name, string.Format( "Expecting value between {0} and {1}", Min, Max ) );
				}
				vclamp = Min + ( ( vclamp - Min ) % Step ) * Step;
				pi.SetValue( dst, vclamp );
			}
			catch( Exception ) {
				if( throwOnError ) {
					throw;
				}
			}
		}
	}

	interface IFxContext {
		/// <summary>time since sequence start</summary>
		TimeSpan TimeNow { get; }

		/// <summary>sequence length, TimeSpan.Zero means unbounded, otherwise TimeNow should always be less</summary>
		TimeSpan TimeLength { get; }
	}

	interface IFx {
		/// <summary></summary>
		/// <param name="ctx">target frame context</param>
		/// <returns>enumeration of destination colour data, this will be consumed up to some pixel limit</returns>
		IEnumerable<Color> Execute( IFxContext ctx );
	}

	/// <summary>effects that modify pixels in a stream can accept input sources by implementing this interface</summary>
	interface IFxPipe : IFx {

		/// <summary>add one source effect</summary>
		/// <param name="src">effect to add, this effect won't be bound anywhere else, call Execute to enumerate the source</param>
		void AddSource( IFx src );
	}

	class FxContextSimple : IFxContext {
		public FxContextSimple( TimeSpan now ) {
			TimeNow = now;
		}

		public TimeSpan TimeNow { get; private set; }

		public TimeSpan TimeLength => TimeSpan.Zero;
	}
}
