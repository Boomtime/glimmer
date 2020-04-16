namespace ShadowCreatures.Glimmer {
	using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;

	public enum Adjustment {
		[Description( "Allows adjustment of the property value prior to runtime." )]
		Initialization,
		[Description( "Allows dynamic adjustment of the property value during runtime." )]
		Dynamic,
	}

	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class ConfigurableAttribute : Attribute {
		public ConfigurableAttribute( Adjustment adj = Adjustment.Initialization ) {
			Adjustment = adj;
		}
		public Adjustment Adjustment { get; }
	}

	class ConfigurableDoubleAttribute : ConfigurableAttribute {
		public double Minimum { get; set; }
		public double Maximum { get; set; }
		public double Default { get; set; }
	}

	class ConfigurableIntegerAttribute : ConfigurableAttribute {
		public int Minimum { get; set; }
		public int Maximum { get; set; }
		public int Default { get; set; }
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

		/// <summary>indicates if Execute method should be called, continuous effects can just return true, one-shot effects can indicate when done</summary>
		bool IsRunning { get; }
	}

	class FxContextSimple : IFxContext {
		public FxContextSimple( TimeSpan now ) {
			TimeNow = now;
		}

		public TimeSpan TimeNow { get; private set; }

		public TimeSpan TimeLength => TimeSpan.Zero;
	}
}
