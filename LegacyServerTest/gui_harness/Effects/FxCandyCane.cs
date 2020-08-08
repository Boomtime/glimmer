namespace ShadowCreatures.Glimmer.Effects {
	using System.Collections.Generic;
	using System.Drawing;

	class FxCandyCane : IFx {
		[ConfigurableInteger( Min = 1, Max = 12 )]
		public int PixelsPerStripe { get; set; } = 5;

		[Configurable]
		public Color ColourStripe { get; set; } = Color.Red;

		[Configurable]
		public Color ColourBackground { get; set; } = Color.LightGray;

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
