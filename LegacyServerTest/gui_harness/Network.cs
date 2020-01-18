namespace ShadowCreatures.Glimmer {
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using System.Collections.Generic;
	using System.Text;
	using System.Diagnostics;
    using System.Drawing;

    /// <summary>order of colour triples</summary>
    public enum NetworkColorOrder {
		RGB, // WS2812 (Glim orders the output modulator for this goal)
		GBR, // WS2811 (bizarro world)
	}

	/// <summary>top-level network message identifier</summary>
	public enum NetworkMessage : byte {
		RGB1 = 1,
		RGB2 = 2,
		Ping = 3,
		Pong = 4,
		ButtonStatus = 5,
		ButtonColor = 6,
	}

	/// <summary>hardware type byte</summary>
	public enum HardwareType : byte {
		Server = 1,
		GlimV2 = 2,
		GlimV3 = 3,
		GlimV4 = 4,
	}

	/// <summary>glim device button status byte</summary>
	public enum ButtonStatus : byte {
		Down = 1,
		Held = 2,
		Up = 3,
	}

	public enum WifiRSSI : int {
		None = -200, // everything beyond -72dbm or unknown, not likely to get a packet through if it exists at all
		Terrible = -87, // (down to), will get some comms, but might be tragic
		Weak = -75, // (down to), reliable only if it's consistent
		Good = -57, // (down to), very solid and reliable, can handle variation
		Excellent = -30, // down to -30, on top of the router, may have caught fire
	}

	/// <summary>base class for all event args from the server, always have a IPEndPoint SourceAddress</summary>
	class NetworkEventArgs : EventArgs {
		protected NetworkEventArgs( IPEndPoint source ){
			SourceAddress = source;
		}
		public readonly IPEndPoint SourceAddress;
	}

	/// <summary>gives paramters to ping/pong events</summary>
	class NetworkPingEventArgs : NetworkEventArgs {
		public NetworkPingEventArgs( IPEndPoint source, HardwareType hw, string hostname, TimeSpan uptime, float cpu, int dbm ) : base( source ) {
			HardwareType = hw;
			Hostname = hostname;
			Uptime = uptime;
			CPU = cpu;
			dBm = dbm;
			RSSI = DBMtoRSSI( dbm );
		}
		public readonly HardwareType HardwareType;
		public readonly string Hostname;
		public readonly TimeSpan Uptime;
		public readonly float CPU;
		public readonly int dBm;
		public readonly WifiRSSI RSSI;

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
		public NetworkButtonStatusEventArgs( IPEndPoint source, ButtonStatus status ) : base( source ) {
			ButtonStatus = status;
		}
		public readonly ButtonStatus ButtonStatus;
	}

	class NetworkServer {

		public const int DefaultPort = 1998;

		UdpClient mSocket;
		bool mAsyncReceiveEnabled;
		readonly DateTime mStartDatetime;

		public NetworkServer( int port = DefaultPort ) {
			mSocket = new UdpClient( port ) {
				EnableBroadcast = true
			};
			mAsyncReceiveEnabled = false;
			mStartDatetime = DateTime.Now;
		}

		byte[] ConstructPing( bool ping ) {
			var result = new List<byte>();
			string hostname = System.Environment.MachineName;
			long uptime = (long)( DateTime.Now - mStartDatetime ).TotalSeconds;

			result.Add( (byte)( ping ? NetworkMessage.Ping : NetworkMessage.Pong ) );
			result.Add( (byte)( HardwareType.Server ) );
			result.Add( (byte)( hostname.Length ) );
			result.AddRange( Encoding.ASCII.GetBytes( hostname ) );
			result.Add( (byte)( ( uptime & 0x7F000000 ) >> 24 ) );
			result.Add( (byte)( ( uptime & 0xFF0000 ) >> 16 ) );
			result.Add( (byte)( ( uptime & 0xFF00 ) >> 8 ) );
			result.Add( (byte)( uptime & 0xFF ) );

			return result.ToArray();
		}

		public void Listen() {
			if( !mAsyncReceiveEnabled ) {
				AsyncDatagramReceive();
				mAsyncReceiveEnabled = true;
			}
		}

		void DataReceived( IAsyncResult ar ) {
			var rhost = new IPEndPoint( IPAddress.Any, 0 );
			byte[] dgram = mSocket.EndReceive( ar, ref rhost );

			ProcessDatagram( rhost, dgram );

			AsyncDatagramReceive();
		}

		void AsyncDatagramReceive() {
			mSocket.BeginReceive( DataReceived, null );
		}

		string ExtractPascalString( byte[] payload, int index, out int strend ) {
			int count = payload[index];

			strend = index + count + 1;

			if( count > payload.Length - index )
				return null;

			return Encoding.ASCII.GetString( payload, index + 1, count );
		}

		int ExtractInteger( byte[] dgram, int index ) {
			int ret;

			if( dgram.Length < index + 4 )
				return 0;

			ret = dgram[index] << 24;
			ret += dgram[index + 1] << 16;
			ret += dgram[index + 2] << 8;
			ret += dgram[index + 3];
			return ret;
		}

		/// <summary>called whenever a ping message is received</summary>
		public event EventHandler<NetworkPingEventArgs> PingReceived;

		protected virtual void OnPingReceived( NetworkPingEventArgs e ) {
			PingReceived?.Invoke( this, e );
		}

		/// <summary>called whenever a ping message is received</summary>
		public event EventHandler<NetworkPingEventArgs> PongReceived;

		protected virtual void OnPongReceived( NetworkPingEventArgs e ) {
			PongReceived?.Invoke( this, e );
		}

		/// <summary>called whenever a button status message is received</summary>
		public event EventHandler<NetworkButtonStatusEventArgs> ButtonStatusReceived;

		protected virtual void OnButtonStatusReceived( NetworkButtonStatusEventArgs e ) {
			ButtonStatusReceived?.Invoke( this, e );
		}

		void ProcessDatagram( IPEndPoint rhost, byte[] dgram ) {
			Debug.WriteLine( "got some data from " + rhost.Address.ToString() );

			var msg = (NetworkMessage)dgram[0];

			switch( msg ) {
				case NetworkMessage.Ping:
				case NetworkMessage.Pong: {
						var hname = ExtractPascalString( dgram, 2, out int strend );
						var uptime = TimeSpan.FromSeconds( ExtractInteger( dgram, strend ) );
						float cpu = dgram.Length > strend + 4 ? (float)( dgram[strend + 4] ) / byte.MaxValue : 0;
						int dbm = dgram.Length > strend + 5 ? (int)dgram[strend + 5] - byte.MaxValue : -200;
						var args = new NetworkPingEventArgs( rhost, (HardwareType)dgram[1], hname, uptime, cpu, dbm );
						if( NetworkMessage.Ping == msg ) {
							OnPingReceived( args );
						}
						else {
							OnPongReceived( args );
						}
					}
					break;

				case NetworkMessage.ButtonStatus: {
						OnButtonStatusReceived( new NetworkButtonStatusEventArgs( rhost, (ButtonStatus)dgram[1] ) );
					}
					break;

				default: {
						Debug.WriteLine( string.Format( "mystery message ({0}) received (and ignored)", dgram[0] ) );
					}
					break;
			}
		}

		public void SendPing( IPEndPoint dst ) {
			var ping = ConstructPing( true );
			mSocket.Send( ping, ping.Length, dst );
		}

		public void SendPong( IPEndPoint dst ) {
			var ping = ConstructPing( false );
			mSocket.Send( ping, ping.Length, dst );
		}

		class DGramList : List<byte> {
			public void AddRGB( Color c ) { Add( c.R ); Add( c.G ); Add( c.B ); }
			public void AddGBR( Color c ) { Add( c.G ); Add( c.B ); Add( c.R ); }
		}

		public void SendRGB( IPEndPoint dst, IEnumerable<Color> vector, NetworkColorOrder order = NetworkColorOrder.RGB ) {
			var dgram = new DGramList();
			dgram.Add( (byte)NetworkMessage.RGB1 );
			if( NetworkColorOrder.GBR == order ) {
				foreach( var c in vector ) {
					dgram.AddGBR( c );
				}
			}
			else {
				foreach( var c in vector ) {
					dgram.AddRGB( c );
				}
			}
			mSocket.Send( dgram.ToArray(), dgram.Count, dst );
		}

		public void SendButtonColor( IPEndPoint dst, Color min, Color max, short period, Color onHeld, NetworkColorOrder order = NetworkColorOrder.RGB ) {
			var dgram = new DGramList();
			dgram.Add( (byte)NetworkMessage.ButtonColor );

			// low/high
			if( NetworkColorOrder.GBR == order ) {
				dgram.AddGBR( min );
				dgram.AddGBR( max );
			}
			else {
				dgram.AddRGB( min );
				dgram.AddRGB( max );
			}

			// period (1024ms)
			dgram.Add( (byte)( ( period & 0x7F00 ) >> 8 ) );
			dgram.Add( (byte)( period & 0xFF ) );

			// on-held
			if( NetworkColorOrder.GBR == order ) {
				dgram.AddGBR( onHeld );
			}
			else {
				dgram.AddRGB( onHeld );
			}

			mSocket.Send( dgram.ToArray(), dgram.Count, dst );
		}
	}
}
