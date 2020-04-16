namespace ShadowCreatures.Glimmer {
	using System.Collections.Generic;
    using System.Drawing;

	interface IGlimPacket {

		/// <summary>get the number of pixels in the packet</summary>
		int PixelCount { get; }

		/// <summary>set pixel colour by blending against existing using src-alpha</summary>
		/// <param name="pixel"></param>
		/// <param name="src"></param>
		void SetPixel( int pixel, Color src );
	}

	/// <summary>maps a contiguous vector of pixels to potentially disparate pixels in underlying packets</summary>
	interface IGlimPixelMap {

		/// <summary>total count of pixels in the map</summary>
		int PixelCount { get; }

		/// <summary>write colour data into the pixel map</summary>
		/// <param name="src"></param>
		void Write( IEnumerable<Color> src );
	}
}
