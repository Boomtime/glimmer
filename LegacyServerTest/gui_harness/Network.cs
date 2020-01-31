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

	/// <summary>base class for all event args from the server, always have a IPEndPoint SourceAddress</summary>
	class NetworkEventArgs : EventArgs {
		protected NetworkEventArgs( IPEndPoint source ){
			SourceAddress = source;
		}
		public readonly IPEndPoint SourceAddress;
	}

	/// <summary>gives paramters to ping/pong events</summary>
	class NetworkPingEventArgs : NetworkEventArgs, IGlimDevice {
		public NetworkPingEventArgs( IPEndPoint source, HardwareType hw, string hostname, TimeSpan uptime, float cpu, int dbm, uint net_recv ) : base( source ) {
			Hostname = hostname;
			HardwareType = hw;
			Uptime = uptime;
			CPU = cpu;
			dBm = dbm;
			RSSI = DBMtoRSSI( dbm );
			NetRecv = net_recv;
		}
		public virtual HardwareType HardwareType { get; }
		public virtual string Hostname { get; }
		public virtual IPEndPoint IPEndPoint => SourceAddress;
		public virtual TimeSpan Uptime { get; }
		public virtual float CPU { get; }
		public virtual int dBm { get; }
		public virtual WifiRSSI RSSI { get; }
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
			mSocket = new UdpClient( port ) { EnableBroadcast = true };
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
			var data = mSocket.EndReceive( ar, ref rhost );
			ProcessDatagram( rhost, new RxDatagram( data ) );
			AsyncDatagramReceive();
		}

		void AsyncDatagramReceive() {
			mSocket.BeginReceive( DataReceived, null );
		}

		class RxDatagram {
			byte[] mData;
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

		void ProcessDatagram( IPEndPoint rhost, RxDatagram dgram ) {
			Debug.WriteLine( "got some data from " + rhost.Address.ToString() );
			var msg = (NetworkMessage)dgram.ReadByte();
			switch( msg ) {
				case NetworkMessage.Ping:
				case NetworkMessage.Pong: {
						var hw = (HardwareType)dgram.ReadByte();
						var hname = dgram.ReadString();
						var uptime = TimeSpan.FromSeconds( dgram.ReadUint() );
						float cpu = (float)( dgram.ReadByte() ) / byte.MaxValue;
						int dbm = dgram.ReadByte() - byte.MaxValue;
						uint nrecv = dgram.ReadUint();
						var args = new NetworkPingEventArgs( rhost, hw, hname, uptime, cpu, dbm, nrecv );
						if( NetworkMessage.Ping == msg ) {
							OnPingReceived( args );
						}
						else {
							OnPongReceived( args );
						}
					}
					break;

				case NetworkMessage.ButtonStatus: {
						OnButtonStatusReceived( new NetworkButtonStatusEventArgs( rhost, (ButtonStatus)dgram.ReadByte() ) );
					}
					break;

				default: {
					Debug.WriteLine( string.Format( "mystery message ({0}) received (and ignored)", (byte)msg ) );
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

		abstract class TxDatagram : List<byte> {
			public abstract void AddColour( Color c );

			public void AddShort( short s ) {
				Add( (byte)( ( s & 0x7F00 ) >> 8 ) );
				Add( (byte)( s & 0xFF ) );
			}

			class TxRGB : TxDatagram {
				public override void AddColour( Color c ) {
					Add( c.R ); Add( c.G ); Add( c.B );
				}
			}

			class TxGBR : TxDatagram {
				public override void AddColour( Color c ) {
					Add( c.G ); Add( c.B ); Add( c.R );
				}
			}

			public static TxDatagram Create( NetworkColorOrder order ) {
				if( NetworkColorOrder.GBR == order ) {
					return new TxGBR();
				}
				return new TxRGB();
			}
		}

		public void SendRGB( IPEndPoint dst, IEnumerable<Color> vector, NetworkColorOrder order = NetworkColorOrder.RGB ) {
			var dgram = TxDatagram.Create( order );
			dgram.Add( (byte)NetworkMessage.RGB1 );
			foreach( var c in vector ) {
				dgram.AddColour( c );
			}
			mSocket.Send( dgram.ToArray(), dgram.Count, dst );
		}

		public void SendButtonColor( IPEndPoint dst, Color min, Color max, short period, Color onHeld, NetworkColorOrder order = NetworkColorOrder.RGB ) {
			var dgram = TxDatagram.Create( order );
			dgram.Add( (byte)NetworkMessage.ButtonColor );
			dgram.AddColour( min );
			dgram.AddColour( max );
			dgram.AddShort( period );
			dgram.AddColour( onHeld );
			mSocket.Send( dgram.ToArray(), dgram.Count, dst );
		}
	}
}
