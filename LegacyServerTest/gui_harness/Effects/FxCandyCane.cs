namespace ShadowCreatures.Glimmer.Effects {
	using System.Collections.Generic;
	using System.Drawing;

	class FxCandyCane : IFx {
		[ConfigurableInteger( Minimum = 1, Maximum = 12 )]
		public int PixelsPerStripe = 5;

		[Configurable]
		public Color ColourStripe = Color.Red;

		[Configurable]
		public Color ColourBackground = Color.LightGray;

		public bool IsRunning => true;

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			while( true ) {
				for( int j = 0 ;  j < PixelsPerStripe ; j++ ) {
					yield return ColourBackground;
				}
				for( int j = 0 ; j < PixelsPerStripe ; j++ ) {
					yield return ColourStripe;
				}
			}
		}
	}
}
