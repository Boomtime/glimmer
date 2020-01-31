namespace ShadowCreatures.Glimmer {
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	class FxComet : IFx {
		DateTime? mStarted = null;
		int mPixelCount = 0;
		bool mFinished;

		[ConfigurableInteger( Minimum = 5, Maximum = 200 )]
		public int SpeedPixelsPerSecond = 60;

		[ConfigurableInteger( Minimum = 1, Maximum = 50)]
		public int TailPixelLength = 20;

		[Configurable]
		public Color BaseColor = Color.White;

		public bool IsRunning {
			get { return mStarted.HasValue && !mFinished; }
		}

		public event EventHandler Finished;

		/// <summary>begins the shot</summary>
		/// <param name="pixelCount">number of pixels to cover</param>
		public void Initialize( int pixelCount ) {
			mPixelCount = pixelCount;
			mStarted = DateTime.Now;
			mFinished = false;
		}

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			if( !mStarted.HasValue ) {
				for( int j = 0 ; j < mPixelCount ; j++ ) {
					yield return Color.Transparent;
				}
				yield break;
			}
			// the comet running
			TimeSpan runningTime = DateTime.Now - mStarted.Value;
			int ballPixel = (int)( runningTime.TotalSeconds * SpeedPixelsPerSecond );
			int cometTailStartPixel = (int)( ballPixel - TailPixelLength );
			if( ballPixel > mPixelCount ) {
				// got some remaining, but have otherwise finished
				if( !mFinished ) {
					mFinished = true;
					Finished?.Invoke( this, new EventArgs() );
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
				pixel++;
			}
		}
	}
}
