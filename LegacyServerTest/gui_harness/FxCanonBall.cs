namespace ShadowCreatures.Glimmer {
	using System;
	using System.Drawing;

	class FxCannonBall : FxBase {
		DateTime? mStarted;
		int mPixelCount;
		bool mFinished;

		public FxCannonBall( IGlimPixelMap map ) : base( map ) {
			mStarted = null;
			BaseColor = Color.White;
		}

		/// <summary>begins the shot</summary>
		/// <param name="pixelCount">number of pixels to cover</param>
		public void Start( int pixelCount ) {
			mStarted = DateTime.Now;
			mPixelCount = pixelCount;
			mFinished = false;
		}

		/// <summary>will no longer floodfill</summary>
		public void Stop() {
			mStarted = null;
		}

		public bool IsRunning {
			get { return mStarted.HasValue && !mFinished; }
		}

		public event EventHandler Finished;

		public ColorReal BaseColor { get; set; }

		public override void Execute( IFxContext ctx ) {
			if( !mStarted.HasValue ) {
				return;
			}

			TimeSpan runningTime = DateTime.Now - mStarted.Value;
			int pixel = 0;

			while( runningTime.TotalSeconds > (double)pixel * 0.015 ) {
				if( pixel >= PixelMap.PixelCount || pixel > mPixelCount ) {
					// got some remaining, but have otherwise finished
					if( !mFinished ) {
						mFinished = true;
						Finished?.Invoke( this, new EventArgs() );
					}
					break;
				}

				PixelMap[pixel].CopyFrom( BaseColor );

				// generate a fading trail
				for( int tail = pixel - 1 ; tail >= 0 ; tail-- )
					PixelMap[tail].Luminance -= 0.05;

				pixel++;
			}
		}
	}
}
