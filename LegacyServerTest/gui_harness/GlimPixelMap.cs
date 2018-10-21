namespace ShadowCreatures.Glimmer {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Collections;

	/// <summary>factory initializer for IGlimPixelMap</summary>
	class GlimDeviceMap : IEnumerable<GlimDevice> {

		struct GlimDeviceMapElement {
			/// <summary>Packet destination when writing/sourcing pixel data</summary>
			public GlimDevice Device;

			/// <summary>first addressable pixel</summary>
			public int PixelStart;

			/// <summary>number of contiguous addressable pixels</summary>
			public int PixelCount;
		}

		List<GlimDeviceMapElement> mDeviceList = new List<GlimDeviceMapElement>();

		/// <summary>add GlimDevice with limit parameters</summary>
		/// <param name="device"></param>
		/// <param name="pixelStart"></param>
		/// <param name="pixelCount"></param>
		public void Add( GlimDevice device, int pixelStart, int pixelCount ) {
			mDeviceList.Add( new GlimDeviceMapElement { Device = device, PixelStart = pixelStart, PixelCount = pixelCount } );
		}
		public void Add( GlimDevice device ) {
			Add( device, 0, device.PixelCount );
		}

		/// <summary>compile to a GlimPixelMap, the resulting map is unaffected by further changes to this</summary>
		/// <returns></returns>
		public GlimPixelMap Compile() {
			var res = new GlimPixelMap();
			foreach( var e in mDeviceList ) {
				res.Add( e.Device.PixelData, e.PixelStart, e.PixelCount );
			}
			return res;
		}

		/// <summary>down-convert to GlimPixelMap, the resulting map is unaffected by changes to this</summary>
		/// <param name="map"></param>
		public static implicit operator GlimPixelMap( GlimDeviceMap map ) {
			return map.Compile();
		}

		public IEnumerator<GlimDevice> GetEnumerator() {
			foreach( var e in mDeviceList )
				yield return e.Device;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			foreach( var e in mDeviceList )
				yield return e.Device;
		}
	}

	class GlimPixelMap : IGlimPixelMap {

		enum GlimPixelMapDirection {
			Forwards,
			Backwards,
		}

		class GlimPixelMapElement {
			/// <summary>Packet destination when writing/sourcing pixel data</summary>
			public readonly IGlimPacket Packet;

			/// <summary>first addressable pixel</summary>
			public readonly int PixelStart;

			/// <summary>number of contiguous addressable pixels</summary>
			public readonly int PixelCount;

			/// <summary>how are pixels addressed</summary>
			public readonly GlimPixelMapDirection Direction;

			public GlimPixelMapElement( IGlimPacket packet, int pixelStart, int pixelCount ) {
				Packet = packet;
				if( pixelCount < 0 ) {
					PixelStart = pixelStart + pixelCount;
					PixelCount = 0 - pixelCount;
					Direction = GlimPixelMapDirection.Backwards;
				}
				else {
					PixelStart = pixelStart;
					PixelCount = pixelCount;
					Direction = GlimPixelMapDirection.Forwards;
				}
			}
		}

		List<GlimPixelMapElement> packetList = new List<GlimPixelMapElement>();

		public ColorReal this[int pixel] {
			get {
				foreach( var pi in packetList ) {
					if( pi.PixelCount > pixel ) {
						if( GlimPixelMapDirection.Forwards == pi.Direction )
							return pi.Packet[pi.PixelStart + pixel];
						else
							return pi.Packet[pi.PixelStart + pi.PixelCount - pixel - 1];
					}

					pixel -= pi.PixelCount;
				}

				throw new IndexOutOfRangeException( "Pixel index is out of range" );
			}
			set {
				foreach( var pi in packetList ) {
					if( pi.PixelCount > pixel ) {
						if( GlimPixelMapDirection.Forwards == pi.Direction )
							pi.Packet[pi.PixelStart + pixel] = value;
						else
							pi.Packet[pi.PixelStart + pi.PixelCount - pixel - 1] = value;
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

			packetList.Add( new GlimPixelMapElement( packet, pixelStart, pixelCount ) );
		}

		public void Add( IGlimPacket packet ) {
			Add( packet, 0, packet.Device.PixelCount );
		}

		public IEnumerator<ColorReal> GetEnumerator() {
			foreach( var item in packetList ) {
				if( GlimPixelMapDirection.Forwards == item.Direction ) {
					for( var pixel = item.PixelStart ; pixel < item.PixelStart + item.PixelCount ; pixel++ )
						yield return item.Packet[pixel];
				}
				else {
					for( var pixel = item.PixelStart + item.PixelCount - 1 ; pixel >= item.PixelStart ; pixel-- )
						yield return item.Packet[pixel];
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}
	}
}
