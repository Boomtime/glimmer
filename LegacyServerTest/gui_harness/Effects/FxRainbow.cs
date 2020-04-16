namespace ShadowCreatures.Glimmer.Effects {
	using System.Collections.Generic;
	using System.Drawing;

	class FxRainbow : IFx {

		[ConfigurableDouble( Minimum = 0.1, Maximum = 60 )]
		public double HueSecondsPerCycle = 12.5;

		[ConfigurableInteger( Minimum = 2, Maximum = 5000 )]
		public int HueCyclePixelLength = 100;

		public bool IsRunning => true;

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			ColorReal rc = Color.Red; // any RGB primary color will do as a seed
			double hueStridePerPixel = 1.0 / HueCyclePixelLength;
			if( ctx.TimeLength.TotalSeconds > 0.0 ) {
				rc.Hue = ctx.TimeNow.TotalSeconds / ctx.TimeLength.TotalSeconds;
			}
			else {
				rc.Hue = ( ctx.TimeNow.TotalSeconds % HueSecondsPerCycle ) / HueSecondsPerCycle;
			}

			while( true ) {
				yield return rc;
				rc.Hue -= hueStridePerPixel;
			}
		}
	}
}
