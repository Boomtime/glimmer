namespace ShadowCreatures.Glimmer {
	using System.Collections.Generic;

	interface IGlimDevice {

		/// <summary>name by which the device knows itself, generally from the EEPROM, also usually announces itself like this on the network</summary>
		string DeviceName { get; }

		/// <summary>alias name, defaults to DeviceName but can be changed for UI</summary>
		string AliasName { get; }

		/// <summary>get the expected number of pixels</summary>
		int PixelCount { get; }
	}

	interface IGlimPacket : IEnumerable<ColorReal> {
		
		/// <summary>device that will receive this packet</summary>
		IGlimDevice Device { get; }

		/// <summary>get/set the colour data for the given pixel index</summary>
		/// <param name="pixel">index of the pixel to address</param>
		/// <returns>ColourReal of pixel data</returns>
		ColorReal this[int pixel] { get; set; }
	}

	/// <summary>maps a contiguous vector of pixels to potentially disparate pixels in underlying devices</summary>
	interface IGlimPixelMap : IEnumerable<ColorReal> {
		
		int PixelCount { get; }

		ColorReal this[int pixel] { get; set; }
	}
}
