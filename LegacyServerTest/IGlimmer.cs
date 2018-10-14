using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlimmerPlay {

	class ColourReal {
		// placeholder for ColourReal class
	}

	interface IGlimDevice {

		/// <summary>name by which the device knows itself, generally from the EEPROM</summary>
		string DeviceName { get; }

		/// <summary>alias name, defaults to DeviceName but can be changed for UI</summary>
		string AliasName { get; }

		/// <summary>number of channels supported, should be at least 1, but is probably 2</summary>
		int ChannelCount { get; }

		/// <summary>get the number of pixels in a channel</summary>
		/// <param name="c">channel index, from zero (inclusive) to ChannelCount (exclusive)</param>
		/// <returns>number of pixels in that channel</returns>
		/// <exception cref="IndexOutOfRangeException">c is not between zero (inclusive) to ChannelCount (exclusive)</exception>
		int GetPixelCount( int c );
	}

	interface IGlimPixelVector {

		/// <summary>get/set the colour data for the given pixel index</summary>
		/// <param name="pixel">index of the pixel to address</param>
		/// <returns>ColourReal of pixel data</returns>
		ColourReal this[int pixel] { get; set; }

		/// <summary>number of pixels in the vector</summary>
		int PixelCount { get; }
	}

	/// <summary>one packet of data for a single channel of a device</summary>
	interface IGlimPacket : IGlimPixelVector {
		
		/// <summary>device that will receive this packet</summary>
		IGlimDevice Device { get; }

		/// <summary>output channel to which the device will emit the colour data, zero indexed</summary>
		int Channel { get; }
	}

	/// <summary>implementation of a pixel vector that can map pixels contiguously across devices</summary>
	class GlimPixelMap : IGlimPixelVector {

		class GlimPacketItem {
			/// <summary>Packet destination when writing/sourcing pixel data</summary>
			public readonly IGlimPacket Packet;

			/// <summary>first addressable pixel</summary>
			public readonly int PixelStart;

			/// <summary>number of contiguous addressable pixels</summary>
			public readonly int PixelCount;

			public GlimPacketItem( IGlimPacket packet, int pixelStart, int pixelCount ) {
				Packet = packet;
				PixelStart = pixelStart;
				PixelCount = pixelCount;
			}
		}

		List<GlimPacketItem> packetList = new List<GlimPacketItem>();

		public ColourReal this[int pixel] {
			get {
				foreach( var pi in packetList ) {
					if( pi.PixelCount > pixel )
						return pi.Packet[pi.PixelStart + pixel];

					pixel -= pi.PixelCount;
				}

				throw new IndexOutOfRangeException( "Pixel index is out of range" );
			}
			set {
				foreach( var pi in packetList ) {
					if( pi.PixelCount > pixel ) {
						pi.Packet[pi.PixelStart + pixel] = value;
						break;
					}
					pixel -= pi.PixelCount;
				}
			}
		}

		public int PixelCount {
			get {
				return packetList.Aggregate( 0, ( sum, cur ) => sum += cur.PixelCount );
			}
		}

		public void Add( IGlimPacket packet, int pixelStart, int pixelCount ) {
			if( pixelStart < 0 || pixelStart + pixelCount > packet.Device.GetPixelCount( packet.Channel ) )
				throw new ArgumentOutOfRangeException( "pixelStart or pixelCount are beyond the packet device capabilities" );

			packetList.Add( new GlimPacketItem( packet, pixelStart, pixelCount ) );
		}
	}
}
