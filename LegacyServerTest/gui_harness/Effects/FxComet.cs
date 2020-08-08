namespace ShadowCreatures.Glimmer.Effects {
	using System;
	using System.Collections.Generic;
    using System.Drawing;

	class FxComet : IFx {
		TimeSpan mStarted = TimeSpan.Zero;
		bool mFinished = false;

		[ConfigurableInteger( Min = 5, Max = 200 )]
		public int SpeedPixelsPerSecond { get; set; } = 60;

		[ConfigurableInteger( Min = 1, Max = 50)]
		public int TailPixelLength { get; set; } = 20;

		[Configurable]
		public Color BaseColor { get; set; } = Color.White;

		[ConfigurableInteger( Min = 1, Max = 500 )]
		public int PixelCount { get; set; } = 300;

		public bool IsRunning {
			get { return !mFinished; }
		}

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			if( TimeSpan.Zero == mStarted ) {
				mStarted = ctx.TimeNow;
			}
			// the comet running
			TimeSpan runningTime = ctx.TimeNow - mStarted;
			int ballPixel = (int)( runningTime.TotalSeconds * SpeedPixelsPerSecond );
			int cometTailStartPixel = (int)( ballPixel - TailPixelLength );
			if( ballPixel > PixelCount ) {
				// got some remaining, but have otherwise finished
				if( !mFinished ) {
					mFinished = true;
				}
				yield break;
			}
			for( int pixel = 0 ; pixel < PixelCount ; pixel++ ) {
				if( pixel < cometTailStartPixel ) {
					// @todo: why always Black behind the comet?
					yield return Color.Black;
				}
				else if( pixel > ballPixel ) {
					yield return Color.Transparent;
				}
				else {
					// in the comet or tail...
					yield return new ColorReal( BaseColor ) {
						Luminance = (double)( pixel - cometTailStartPixel ) / TailPixelLength
					};
				}
			}
		}
	}
}
