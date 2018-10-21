namespace ShadowCreatures.Glimmer {
	using System;

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
