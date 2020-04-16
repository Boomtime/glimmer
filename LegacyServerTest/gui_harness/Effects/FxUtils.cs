namespace ShadowCreatures.Glimmer.Effects {
	using System.Collections.Generic;
	using System.Drawing;

	public static class FxUtils {

		public static IEnumerable<Color> InfiniteColor( Color clr ) {
			while( true ) {
				yield return clr;
			}
		}

		/// <summary>skips a specified numer of pixels before streaming the remainder</summary>
		/// <param name="src">pixel source</param>
		/// <param name="skip">skip this number of pixels</param>
		public static IEnumerable<Color> SkipPixels( IEnumerable<Color> src, int skip ) {
			var e = src.GetEnumerator();
			while( skip > 0 && e.MoveNext() ) {
				skip--;
			}
			while( e.MoveNext() ) {
				yield return e.Current;
			}
			foreach( var pix in src ) {
				if( 0 == skip ) {
					yield return pix;
				}
				else {
					skip--;
				}
			}
		}

		/// <summary>emits the pixels given in order, and repeats forever</summary>
		/// <param name="set">set of pixels to repeat</param>
		public static IEnumerable<Color> Looper( Color[] set ) {
			while( true ) {
				foreach( var c in set ) {
					yield return c;
				}
			}
		}
	}
}