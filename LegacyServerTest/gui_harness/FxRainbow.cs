namespace ShadowCreatures.Glimmer {
	using System.Collections.Generic;
	using System.Drawing;

	class FxRainbow : IFx {

		[ConfigurableDouble( Minimum = 0.01, Maximum = 1.0 )]
		public double HueCyclesPerSecond = 0.08;

		[ConfigurableDouble( Minimum = 0.001, Maximum = 1.0 )]
		public double HueStridePerPixel = 0.01;

		public bool IsRunning => true;

		public void Initialize( int pixelCount ) {
			// nothing to do
		}

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			ColorReal rc = Color.Red; // any RGB primary color will do as a seed
			if( ctx.TimeLength.TotalSeconds > 0.0 ) {
				rc.Hue = ctx.TimeNow.TotalSeconds / ctx.TimeLength.TotalSeconds;
			}
			else {
				var secondsPerCycle = ( 1.0 / HueCyclesPerSecond );
				rc.Hue = ( ctx.TimeNow.TotalSeconds % secondsPerCycle ) / secondsPerCycle;
			}

			while( true ) {
				yield return rc;
				rc.Hue -= HueStridePerPixel;
			}
		}
	}
}
