namespace ShadowCreatures.Glimmer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	interface IGlimDevice {

		/// <summary>name by which the device knows itself, generally from the EEPROM</summary>
		string DeviceName { get; }

		/// <summary>alias name, defaults to DeviceName but can be changed for UI</summary>
		string AliasName { get; }

		/// <summary>get the expected number of pixels</summary>
		int PixelCount { get; }
	}

	enum ColorOrder {
		RGB, // WS2812 (Glim orders the output modulator for this goal)
		GBR, // WS2811 (bizarro world)
	}

	interface IGlimPacket : IEnumerable<ColorReal> {
		
		/// <summary>device that will receive this packet</summary>
		IGlimDevice Device { get; }

		/// <summary>get/set the colour data for the given pixel index</summary>
		/// <param name="pixel">index of the pixel to address</param>
		/// <returns>ColourReal of pixel data</returns>
		ColorReal this[int pixel] { get; set; }
	}

	/// <summary>basic implementation of a colour vector packet</summary>
	class GlimPacket : IGlimPacket
	{
		readonly IGlimDevice mDevice;
		ColorReal[] mData;

		public GlimPacket( IGlimDevice device )
		{
			mDevice = device;
			mData = new ColorReal[device.PixelCount];
		}

		public IGlimDevice Device
		{
			get { return mDevice; }
		}

		public ColorReal this[int pixel]
		{
			get { return mData[pixel]; }
			set { mData[pixel] = new ColorReal( value ); }
		}

		public IEnumerator<ColorReal> GetEnumerator()
		{
			foreach( var c in mData )
				yield return c;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mData.GetEnumerator();
		}
	}

	/// <summary>maps a contiguous vector of pixels to potentially disparate pixels in underlying devices</summary>
	interface IGlimPixelMap : IEnumerable<ColorReal> {
		
		int PixelCount { get; }

		ColorReal this[int pixel] { get; set; }
	}

	class GlimPixelMap : IGlimPixelMap {

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

		public ColorReal this[int pixel] {
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
			if( pixelStart < 0 || pixelStart + pixelCount > packet.Device.PixelCount )
				throw new ArgumentOutOfRangeException( "pixelStart or pixelCount are beyond the packet device capabilities" );

			packetList.Add( new GlimPacketItem( packet, pixelStart, pixelCount ) );
		}

		public void Add( IGlimPacket packet ) {
			Add( packet, 0, packet.Device.PixelCount );
		}

		public IEnumerator<ColorReal> GetEnumerator()
		{
			foreach( var item in packetList )
			{
				for( var pixel = item.PixelStart ; pixel < item.PixelStart + item.PixelCount ; pixel++ )
					yield return item.Packet[pixel];
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
