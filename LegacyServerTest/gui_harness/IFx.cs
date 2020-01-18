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
		/// <summary>called to reset the effect, always called prior to first execution</summary>
		/// <param name="pixelCount">effector is expected to supply this number of pixels from Execute</param>
		void Initialize( int pixelCount );

		/// <summary></summary>
		/// <param name="ctx">current frame context</param>
		/// <returns>enumeration of destination colour data, this will be consumed up to pixelCount</returns>
		IEnumerable<Color> Execute( IFxContext ctx );

		/// <summary>indicates if Execute method should be called</summary>
		bool IsRunning { get; }
	}

	class FxContextUnbounded : IFxContext {
		public FxContextUnbounded( DateTime started ){
			mTimeNow = DateTime.Now - started;
		}
		readonly TimeSpan mTimeNow;

		public TimeSpan TimeNow => mTimeNow;
		public TimeSpan TimeLength => TimeSpan.Zero;
	}
}
