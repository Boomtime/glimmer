namespace ShadowCreatures.Glimmer {
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Collections.Generic;
	using System.Text;
	using System.Diagnostics;
    using System.Drawing;
	using System.Collections;

	/// <summary>order of colour triples</summary>
	public enum NetworkColourOrder {
		RGB, // WS2812 (Glim orders the output modulator for this goal)
		GBR, // WS2811 (bizarro world)
	}

	abstract class NetworkMessage {

		static protected readonly DateTime StartDatetime = DateTime.Now;

		abstract class TxDatagram : List<byte> {
			public abstract void AddColour( Color c );

			class RGB : TxDatagram {
				public override void AddColour( Color c ) {
					Add( c.R ); Add( c.G ); Add( c.B );
				}
			}

			class GBR : TxDatagram {
				public override void AddColour( Color c ) {
					Add( c.G ); Add( c.B ); Add( c.R );
				}
			}

			public static TxDatagram Create( NetworkColourOrder order ) {
				if( NetworkColourOrder.GBR == order ) {
					return new GBR();
				}
				return new RGB();
			}
		}

		readonly TxDatagram data;

		protected NetworkMessage( NetworkColourOrder order = NetworkColourOrder.RGB ) {
			data = TxDatagram.Create( order );
		}

		protected void AddByte( byte b ) {
			data.Add( b );
		}

		protected void AddShort( short s ) {
			AddByte( (byte)( ( s & 0x7F00 ) >> 8 ) );
			AddByte( (byte)( s & 0xFF ) );
		}

		protected void AddLong( long l ) {
			AddByte( (byte)( ( l & 0x7F000000 ) >> 24 ) );
			AddByte( (byte)( ( l & 0xFF0000 ) >> 16 ) );
			AddByte( (byte)( ( l & 0xFF00 ) >> 8 ) );
			AddByte( (byte)( l & 0xFF ) );
		}

		protected void AddString( string s ) {
			if( string.IsNullOrEmpty( s ) ) {
				AddByte( 0 );
			}
			else {
				AddByte( (byte)( s.Length ) );
				data.AddRange( Encoding.ASCII.GetBytes( s ) );
			}
		}

		protected void AddColour( Color c ) {
			data.AddColour( c );
		}

		public byte[] MakePacket() {
			return data.ToArray();
		}
	}

	class NetworkMessagePing : NetworkMessage {
		protected NetworkMessagePing( bool isPingReply = false, TimeSpan? uptime = null ) {
			TimeSpan supt = uptime ?? DateTime.Now - StartDatetime;
			AddByte( (byte)( isPingReply ? NetworkMessageType.Pong : NetworkMessageType.Ping ) );
			AddByte( (byte)( HardwareType.Server ) );
			AddString( System.Environment.MachineName );
			AddLong( (long)( supt.TotalSeconds ) );
		}

		public NetworkMessagePing( TimeSpan? uptime = null ) : this( false, uptime ) {
		}
	}

	class NetworkMessagePingReply : NetworkMessagePing {
		public NetworkMessagePingReply( TimeSpan? uptime = null ) : base( true, uptime ) {
		}
	}

	class NetworkMessageButtonColour : NetworkMessage {
		public NetworkMessageButtonColour( Color min, Color max, short period, Color onHeld, NetworkColourOrder order = NetworkColourOrder.RGB ) : base( order ) {
			AddByte( (byte)NetworkMessageType.ButtonColour );
			AddColour( min );
			AddColour( max );
			AddShort( period );
			AddColour( onHeld );
		}
	}

	class NetworkMessageColourVector : NetworkMessage {
		public NetworkMessageColourVector( IEnumerable<Color> vector, NetworkColourOrder order = NetworkColourOrder.RGB, bool primaryChannel = true ) : base( order ) {
			AddByte( (byte)( primaryChannel ? NetworkMessageType.RGB1 : NetworkMessageType.RGB2 ) );
			foreach( var c in vector ) {
				AddColour( c );
			}
		}
	}

	/// <summary>top-level network message identifier</summary>
	public enum NetworkMessageType : byte {
		RGB1 = 1,
		RGB2 = 2,
		Ping = 3,
		Pong = 4,
		ButtonStatus = 5,
		ButtonColour = 6,
	}

	class NetworkException : Exception {
		public NetworkException( IPEndPoint src, string msg ) : base( msg ) {
			SourceAddress = src;
		}

		public IPEndPoint SourceAddress { get; }
	}

	/// <summary>base class for all event args from the server, always have a IPEndPoint SourceAddress</summary>
	class NetworkEventArgs : EventArgs {
		protected NetworkEventArgs( NetworkMessageType type, IPEndPoint source ){
			MessageType = type;
			SourceAddress = source;
		}
		
		public IPEndPoint SourceAddress { get; }

		public NetworkMessageType MessageType { get; }
	}

	/// <summary>gives paramters to ping/pong events</summary>
	class NetworkPingEventArgs : NetworkEventArgs, IGlimDevice {
		public NetworkPingEventArgs( NetworkMessageType type, IPEndPoint source, HardwareType hw, string hostname, TimeSpan uptime, float cpu, int dbm, uint net_recv ) : base( type, source ) {
			Hostname = hostname;
			HardwareType = hw;
			Uptime = uptime;
			CPU = cpu;
			dBm = dbm;
			NetRecv = net_recv;
		}
		public HardwareType HardwareType { get; }
		public string Hostname { get; }
		public IPEndPoint IPEndPoint => SourceAddress;
		public TimeSpan Uptime { get; }
		public float CPU { get; }
		public int dBm { get; }
		public WifiRSSI RSSI => DBMtoRSSI( dBm );
		public uint NetRecv { get; }

		static WifiRSSI DBMtoRSSI( int dbm ) {
			if( dbm >= (int)WifiRSSI.Excellent ) {
				return WifiRSSI.Excellent;
			}
			if( dbm >= (int)WifiRSSI.Good ) {
				return WifiRSSI.Good;
			}
			if( dbm >= (int)WifiRSSI.Weak ) {
				return WifiRSSI.Weak;
			}
			if( dbm >= (int)WifiRSSI.Terrible ) {
				return WifiRSSI.Terrible;
			}
			return WifiRSSI.None;
		}
	}

	class NetworkButtonStatusEventArgs : NetworkEventArgs {
		public NetworkButtonStatusEventArgs( IPEndPoint source, ButtonStatus status ) : base( NetworkMessageType.ButtonStatus, source ) {
			ButtonStatus = status;
		}

		public ButtonStatus ButtonStatus { get; }
	}

	class NetworkUdpPacket {
		public NetworkUdpPacket( IPEndPoint dst, NetworkMessage msg ) {
			Destination = dst;
			Payload = msg.MakePacket();
		}

		public IPEndPoint Destination { get; private set; }
		public byte[] Payload { get; private set; }
	}

	class NetworkUdpFrame : List<NetworkUdpPacket> {
		public void Add( IPEndPoint dst, NetworkMessage msg ) {
			Add( new NetworkUdpPacket( dst, msg ) );
		}
	}

	class NetworkServer {

		public const int DefaultPort = 1998;

		readonly UdpClient mSocket;

		public NetworkServer( int port = DefaultPort ) {
			mSocket = new UdpClient( port ) { EnableBroadcast = true };
		}

		class RxDatagram {
			readonly byte[] mData;
			int mIndex;

			public RxDatagram( byte[] data ) {
				mData = data;
				mIndex = 0;
			}

			public bool IsDataRemaining {
				get { return mIndex < mData.Length; }
			}

			public byte ReadByte() {
				if( mData.Length < mIndex + 1 ) {
					return default;
				}
				return mData[mIndex++];
			}

			public uint ReadUint() {
				uint ret;
				if( mData.Length < mIndex + 4 ) {
					mIndex = mData.Length;
					return 0;
				}
				ret =  (uint)mData[mIndex++] << 24;
				ret += (uint)mData[mIndex++] << 16;
				ret += (uint)mData[mIndex++] << 8;
				ret += (uint)mData[mIndex++];
				return ret;
			}

			public string ReadString() {
				if( !IsDataRemaining ) {
					return null;
				}
				int length = ReadByte();
				if( 0 >= length ) {
					return String.Empty;
				}
				if( length > mData.Length - mIndex ) {
					// packet is broken, end it
					mIndex = mData.Length;
					return null;
				}
				var ret = Encoding.ASCII.GetString( mData, mIndex, length );
				mIndex += length;
				return ret;
			}

			public NetworkEventArgs ParseMessage( IPEndPoint rhost ) {
				var msg = (NetworkMessageType)ReadByte();
				switch( msg ) {
					case NetworkMessageType.ButtonStatus:
						return new NetworkButtonStatusEventArgs( rhost, (ButtonStatus)ReadByte() );
					case NetworkMessageType.Ping:
					case NetworkMessageType.Pong:
						var hw = (HardwareType)ReadByte();
						var hname = ReadString();
						var uptime = TimeSpan.FromSeconds( ReadUint() );
						float cpu = (float)( ReadByte() ) / byte.MaxValue;
						int dbm = ReadByte() - byte.MaxValue;
						uint nrecv = ReadUint();
						return new NetworkPingEventArgs( msg, rhost, hw, hname, uptime, cpu, dbm, nrecv );
				}
				throw new NetworkException( rhost, string.Format( "Failed to parse message type {0}", (byte)msg ) );
			}

			static public NetworkEventArgs ParseMessage( IPEndPoint rhost, byte[] data ) {
				return new RxDatagram( data ).ParseMessage( rhost );
			}
		}

		public void Send( IEnumerable<NetworkUdpPacket> pkts ) {
			lock( mSocket ) {
				foreach( var p in pkts ) {
					mSocket.Send( p.Payload, p.Payload.Length, p.Destination );
				}
			}
		}

		public void Send( params NetworkUdpPacket[] pkts ) {
			Send( pkts as IEnumerable<NetworkUdpPacket> );
		}

		public void Send( IPEndPoint dst, NetworkMessage msg ) {
			Send( new NetworkUdpPacket( dst, msg ) );
		}

		public NetworkEventArgs Receive() {
			if( 0 == mSocket.Available ) {
				return null;
			}
			var rhost = new IPEndPoint( IPAddress.Any, 0 );
			byte[] data;
			lock( mSocket ) {
				data = mSocket.Receive( ref rhost );
			}
			var dgram = new RxDatagram( data );
			return dgram.ParseMessage( rhost );
		}
	}
}
