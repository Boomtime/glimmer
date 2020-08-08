namespace ShadowCreatures.Glimmer.Effects {
	using System.Collections.Generic;
	using System.Drawing;

	class FxSolid : IFx {
		/// <summary>set the colour to repeat</summary>
		[Configurable]
		public Color Colour { get; set; }

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			while( true ) {
				yield return Colour;
			}
		}
	}
}
