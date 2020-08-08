namespace ShadowCreatures.Glimmer.Effects {
	using System.Collections.Generic;
	using System.Drawing;

	class FxRainbow : IFx {

		[ConfigurableDouble( Min = 0.1, Max = 300 )]
		public double HueSecondsPerCycle { get; set; } = 12.5;

		[ConfigurableInteger( Min = 2, Max = 5000 )]
		public int HueCyclePixelLength { get; set; } = 100;

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
