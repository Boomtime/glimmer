namespace ShadowCreatures.Glimmer {
	using System.Drawing;

	class FxRainbow : FxBase {

		public FxRainbow( IGlimPixelMap map ) : base( map ) {
		}

		public double HueCyclesPerSecond = 0.08;

		public double HueStridePerPixel = 0.01;

		/// <summary>write current frame data to pixel map</summary>
		public override void Execute( IFxContext ctx ) {
			ColorReal rc = Color.Red; // any RGB primary color will do as a seed
			if( ctx.TimeLength.TotalSeconds > 0.0 ) {
				rc.Hue = ctx.TimeNow.TotalSeconds / ctx.TimeLength.TotalSeconds;
			}
			else {
				var secondsPerCycle = ( 1.0 / HueCyclesPerSecond );
				rc.Hue = ( ctx.TimeNow.TotalSeconds % secondsPerCycle ) / secondsPerCycle;
			}

			foreach( var pix in PixelMap ) {
				pix.CopyFrom( rc );
				rc.Hue -= HueStridePerPixel;
			}
		}
	}
}
