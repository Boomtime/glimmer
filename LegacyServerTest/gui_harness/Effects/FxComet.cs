namespace ShadowCreatures.Glimmer.Effects {
	using System;
	using System.Collections.Generic;
    using System.Drawing;

	class FxComet : IFx {
		readonly int mPixelCount;
		TimeSpan mStarted = TimeSpan.Zero;
		bool mFinished = false;

		[ConfigurableInteger( Minimum = 5, Maximum = 200 )]
		public int SpeedPixelsPerSecond = 60;

		[ConfigurableInteger( Minimum = 1, Maximum = 50)]
		public int TailPixelLength = 20;

		[Configurable]
		public Color BaseColor = Color.White;

		public bool IsRunning {
			get { return !mFinished; }
		}

		/// <summary>begins the shot</summary>
		/// <param name="pixelCount">number of pixels to cover</param>
		public FxComet( int pixelCount ) {
			mPixelCount = pixelCount;
		}

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			if( TimeSpan.Zero == mStarted ) {
				mStarted = ctx.TimeNow;
			}
			// the comet running
			TimeSpan runningTime = ctx.TimeNow - mStarted;
			int ballPixel = (int)( runningTime.TotalSeconds * SpeedPixelsPerSecond );
			int cometTailStartPixel = (int)( ballPixel - TailPixelLength );
			if( ballPixel > mPixelCount ) {
				// got some remaining, but have otherwise finished
				if( !mFinished ) {
					mFinished = true;
				}
				yield break;
			}
			for( int pixel = 0 ; pixel < mPixelCount ; pixel++ ) {
				if( pixel < cometTailStartPixel ) {
					// @todo: why always Black behind the comet?
					yield return Color.Black;
				}
				else if( pixel > ballPixel ) {
					yield return Color.Transparent;
				}
				else {
					// in the comet or tail...
					yield return new ColorReal( BaseColor ) { Luminance = (double)( pixel - cometTailStartPixel ) / TailPixelLength };
				}
			}
		}
	}
}
