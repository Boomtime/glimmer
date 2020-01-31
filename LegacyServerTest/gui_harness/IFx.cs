namespace ShadowCreatures.Glimmer {
	using System;
    using System.Collections.Generic;
    using System.Drawing;

	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	class ConfigurableAttribute : Attribute {
	}

	class ConfigurableDoubleAttribute : ConfigurableAttribute {
		public double Minimum { get; set; }
		public double Maximum { get; set; }
	}

	class ConfigurableIntegerAttribute : ConfigurableAttribute {
		public int Minimum { get; set; }
		public int Maximum { get; set; }
	}

	interface IFxContext {
		/// <summary>time since sequence start</summary>
		TimeSpan TimeNow { get; }

		/// <summary>sequence length, TimeSpan.Zero means unbounded, otherwise TimeNow should always be less</summary>
		TimeSpan TimeLength { get; }
	}

	interface IFx {
		/// <summary></summary>
		/// <param name="ctx">current frame context</param>
		/// <returns>enumeration of destination colour data, this will be consumed up to pixelCount</returns>
		IEnumerable<Color> Execute( IFxContext ctx );

		/// <summary>indicates if Execute method should be called, continuous effects can just return true, one-shot effects can indicate when done</summary>
		bool IsRunning { get; }
	}

	class FxContextContinuous : IFxContext {
		public FxContextContinuous( DateTime started ){
			TimeNow = DateTime.Now - started;
		}

		public TimeSpan TimeNow { get; }
		public TimeSpan TimeLength => TimeSpan.Zero;
	}
}
