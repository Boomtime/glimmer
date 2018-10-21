namespace ShadowCreatures.Glimmer {
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Collections.Generic;
	using System.Text;
	using System.Diagnostics;

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
	}

	/// <summary>glim device button status byte</summary>
	public enum ButtonStatus : byte {
		Down = 1,
		Held = 2,
		Up = 3,
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
		public NetworkPingEventArgs( IPEndPoint source, HardwareType hw, string hostname, TimeSpan uptime ) : base( source ) {
			HardwareType = hw;
			Hostname = hostname;
			Uptime = uptime;
		}
		public readonly HardwareType HardwareType;
		public readonly string Hostname;
		public readonly TimeSpan Uptime;
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
					var args = new NetworkPingEventArgs( rhost, (HardwareType)dgram[1],
						ExtractPascalString( dgram, 2, out int strend ),
						TimeSpan.FromSeconds( ExtractInteger( dgram, strend ) ) );
					if( NetworkMessage.Ping == msg )
						OnPingReceived( args );
					else
						OnPongReceived( args );
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

		public void SendRGB( IPEndPoint dst, IEnumerable<ColorReal> vector, NetworkColorOrder order = NetworkColorOrder.RGB ) {
			var dgram = new List<byte>();
			dgram.Add( (byte)NetworkMessage.RGB1 );
			if( NetworkColorOrder.GBR == order ) {
				foreach( var c in vector )
					dgram.AddRange( c.ToGBRArray() );
			}
			else {
				foreach( var c in vector )
					dgram.AddRange( c.ToRGBArray() );
			}
			mSocket.Send( dgram.ToArray(), dgram.Count, dst );
		}

		public void SendButtonColor( IPEndPoint dst, ColorReal min, ColorReal max, short period, ColorReal onHeld, NetworkColorOrder order = NetworkColorOrder.RGB ) {
			var dgram = new List<byte>();
			dgram.Add( (byte)NetworkMessage.ButtonColor );

			// low/high
			if( NetworkColorOrder.GBR == order ) {
				dgram.AddRange( min.ToGBRArray() );
				dgram.AddRange( max.ToGBRArray() );
			}
			else {
				dgram.AddRange( min.ToRGBArray() );
				dgram.AddRange( max.ToRGBArray() );
			}

			// period (1024ms)
			dgram.Add( (byte)( ( period & 0x7F00 ) >> 8 ) );
			dgram.Add( (byte)( period & 0xFF ) );

			// on-held
			if( NetworkColorOrder.GBR == order )
				dgram.AddRange( onHeld.ToGBRArray() );
			else
				dgram.AddRange( onHeld.ToRGBArray() );

			mSocket.Send( dgram.ToArray(), dgram.Count, dst );
		}
	}
}
