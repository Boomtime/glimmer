namespace ShadowCreatures.Glimmer {
	using System;
    using System.Collections.Generic;

    interface IFxContext {
		/// <summary>time since sequence start</summary>
		TimeSpan TimeNow { get; }

		/// <summary>sequence length, TimeSpan.Zero means unbounded, otherwise TimeNow should always be less</summary>
		TimeSpan TimeLength { get; }
	}

	interface IFx {
		/// <summary>emit effects for current frame</summary>
		void Execute( IFxContext ctx );
	}

	interface IFxr {
		/// <summary>called to reset the effect, always called prior to first execution</summary>
		/// <param name="pixelCount"></param>
		void Configure( int pixelCount );

		/// <summary></summary>
		/// <param name="ctx">current frame context</param>
		/// <param name="src">source colour data, contains at least the pixelCount of colours, can be ignored</param>
		/// <returns>enumeration of destination colour data, this will be consumed up to pixelCount</returns>
		IEnumerable<ColorReal> Execute( IFxContext ctx, IEnumerable<ColorReal> src );

		/// <summary>True to indicate this effect has completed, Execute is no longer valid</summary>
		bool Finished { get; }
	}

	class FxContextUnbounded : IFxContext {
		public FxContextUnbounded( DateTime started ){
			mTimeNow = DateTime.Now - started;
		}
		readonly TimeSpan mTimeNow;

		public TimeSpan TimeNow => mTimeNow;
		public TimeSpan TimeLength => TimeSpan.Zero;
	}

	abstract class FxBase : IFx {
		protected readonly IGlimPixelMap PixelMap;

		public FxBase( IGlimPixelMap map ) {
			PixelMap = map;
		}

		public abstract void Execute( IFxContext ctx );
	}
}
