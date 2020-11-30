namespace ShadowCreatures.Glimmer.Effects {
	using System.Collections.Generic;
	using System.Drawing;

	class FxCandyCane : IFx {
		[Configurable]
		public Color ColourStripe { get; set; } = Color.Red;

		[Configurable]
		public Color ColourBackground { get; set; } = Color.LightGray;

		public byte[] Pattern { get; set; } = new byte[] { 1, 1, 0, 1, 1, 0, 0, 0, 0, 0 };

		[ConfigurableInteger( Max = 15, Min = 0 )]
		public int PeriodSeconds { get; set; } = 4;

		Color PatternColour( byte b ) {
			return b == 0 ? ColourBackground : ColourStripe;
		}

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			if( PeriodSeconds > 0 ) {
				// emit an initial sequence from a variable starting position based on time
				double ratio = ( ctx.TimeNow.TotalSeconds % (double)PeriodSeconds ) / (double)PeriodSeconds;
				int pos = (int)( (double)Pattern.Length * ratio );
				while( pos < Pattern.Length ) {
					yield return PatternColour( Pattern[pos] );
					pos++;
				}
			}
			while( true ) {
				foreach( var b in Pattern ) {
					yield return PatternColour( b );
				}
			}
		}
	}
}
