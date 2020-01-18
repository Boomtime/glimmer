using System.Collections.Generic;
using System.Drawing;

namespace ShadowCreatures.Glimmer {

	class FxScale : IFx {

		readonly IFx mSrc;

		public FxScale( IFx src ) {
			mSrc = src;
		}

		public double LuminanceScale { get; set; }
		public double SaturationScale { get; set; }

		public bool IsRunning => mSrc.IsRunning;

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			foreach( ColorReal pix in mSrc.Execute( ctx ) ) {
				pix.Luminance *= LuminanceScale;
				pix.Saturation *= SaturationScale;
				yield return pix;
			}
		}

		public void Initialize( int pixelCount ) {
			mSrc.Initialize( pixelCount );
		}
	}
}
