namespace ShadowCreatures.Glimmer {
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Collections;
    using System.Drawing;

	class GlimPixelMap : IGlimPixelMap {

		class Element {
			/// <summary>Packet destination when writing/sourcing pixel data</summary>
			public readonly IGlimPacket Packet;

			/// <summary>first addressable pixel</summary>
			protected readonly int PixelStart;

			/// <summary>number of contiguous addressable pixels</summary>
			public readonly int PixelCount;

			public Element( IGlimPacket packet, int pixelStart, int pixelCount ) {
				Packet = packet;
				PixelStart = pixelStart;
				PixelCount = pixelCount;
			}

			/// <summary>writes colour data into the map in the configured direction</summary>
			/// <param name="src">colour data to consume</param>
			/// <returns>true if all configured destination pixels were filled</returns>
			public virtual bool Write( IEnumerator<Color> src ) {
				int pos = PixelStart;
				int limit = PixelStart + PixelCount;
				while( pos < limit ) {
					if( !src.MoveNext() ) {
						return false;
					}
					Packet.SetPixel( pos, src.Current );
					pos++;
				}
				return true;
			}
		}

		class ElementBackwards : Element {
			public ElementBackwards( IGlimPacket packet, int pixelStart, int pixelCount )
				: base( packet, pixelStart, pixelCount ) {
			}
			public override bool Write( IEnumerator<Color> src ) {
				int pos = PixelStart + PixelCount;
				int limit = PixelStart;
				while( pos > limit ) {
					if( !src.MoveNext() ) {
						return false;
					}
					pos--;
					Packet.SetPixel( pos, src.Current );
				}
				return true;
			}
		}

		public class Factory : IEnumerable<IGlimPacket> {

			readonly List<Element> mPacketList = new List<Element>();

			/// <summary>add packet with limit parameters</summary>
			/// <param name="packet"></param>
			/// <param name="pixelStart"></param>
			/// <param name="pixelCount"></param>
			public void Add( IGlimPacket packet, int pixelStart, int pixelCount ) {
				if( pixelStart < 0 || pixelStart + pixelCount > packet.PixelCount ) {
					throw new ArgumentOutOfRangeException( "pixelStart or pixelCount are beyond the packet device capabilities" );
				}
				if( pixelCount < 0 ) {
					mPacketList.Add( new ElementBackwards( packet, pixelStart + pixelCount, 0 - pixelCount ) );
				}
				else {
					mPacketList.Add( new Element( packet, pixelStart, pixelCount ) );
				}
			}
			public void Add( IGlimPacket packet ) {
				Add( packet, 0, packet.PixelCount );
			}
			public void Add( params IGlimPacket[] packets ) {
				foreach( var p in packets ) {
					Add( p );
				}
			}
			public void Add( IEnumerable<IGlimPacket> packets ) {
				foreach( var p in packets ) {
					Add( p );
				}
			}

			/// <summary>compile to a GlimPixelMap, the resulting map is unaffected by further changes to this</summary>
			/// <returns></returns>
			public static implicit operator GlimPixelMap( Factory f ) {
				return new GlimPixelMap( f.mPacketList );
			}

			public IGlimPixelMap Compile() {
				return new GlimPixelMap( mPacketList );
			}

			public IEnumerator<IGlimPacket> GetEnumerator() {
				foreach( var e in mPacketList ) {
					yield return e.Packet;
				}
			}

			IEnumerator IEnumerable.GetEnumerator() {
				foreach( var e in mPacketList ) {
					yield return e.Packet;
				}
			}
		}

		readonly List<Element> mPacketList;

		GlimPixelMap( List<Element> list ) {
			mPacketList = list;
			PixelCount = mPacketList.Aggregate( 0, ( sum, cur ) => sum += cur.PixelCount );
		}

		public void Write( IEnumerable<Color> src ) {
			var ce = src.GetEnumerator();
			foreach( var pi in mPacketList ) {
				if( !pi.Write( ce ) ) {
					break;
				}
			}
		}

		public int PixelCount { get; }
	}
}
