namespace ShadowCreatures.Glimmer {
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Collections.Generic;
	using System.Text;

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

	class NetworkServer {

		public const int DefaultPort = 1998;

		UdpClient mSocket;
		DateTime mStartDatetime;

		public NetworkServer( int port = DefaultPort ) {
			mSocket = new UdpClient( port );
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

		public void SendPing( IPEndPoint dst ) {
			var ping = ConstructPing( true );
			mSocket.Send( ping, ping.Length, dst );
		}

		public void SendPong( IPEndPoint dst ) {
			var ping = ConstructPing( false );
			mSocket.Send( ping, ping.Length, dst );
		}

		public void SendRGB( IPEndPoint dst, IEnumerable<ColorReal> vector, ColorOrder order = ColorOrder.RGB ) {
			var dgram = new List<byte>();
			dgram.Add( (byte)NetworkMessage.RGB1 );
			if( ColorOrder.GBR == order ) {
				foreach( var c in vector )
					dgram.AddRange( c.ToGBRArray() );
			}
			else {
				foreach( var c in vector )
					dgram.AddRange( c.ToRGBArray() );
			}
			mSocket.Send( dgram.ToArray(), dgram.Count, dst );
		}

		public void SendButtonColor( IPEndPoint dst, ColorReal min, ColorReal max, short period, ColorReal onHeld, ColorOrder order = ColorOrder.RGB ) {
			var dgram = new List<byte>();
			dgram.Add( (byte)NetworkMessage.ButtonColor );

			// low/high
			if( ColorOrder.GBR == order ) {
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
			if( ColorOrder.GBR == order )
				dgram.AddRange( onHeld.ToGBRArray() );
			else
				dgram.AddRange( onHeld.ToRGBArray() );

			mSocket.Send( dgram.ToArray(), dgram.Count, dst );
		}
		/*

<packet> ::= <rgb1> | <rgb2> | <ping> | <pong> | <btns> | <btnc>

<rgb1> ::= 1 <rgb-payload>
<rgb2> ::= 2 <rgb-payload>
<ping> ::= 3 <ping-payload>
<pong> ::= 4 <ping-payload>
<btns> ::= 5 <btn-state>
<btnc> ::= 6 <rgb> <rgb> <short> <rgb> ; glimmer minima, maxima, period, on-held


<ping-payload> ::= <hw-type> <hostname> <uptime>
<rgb-payload> ::= <rgb> | <rgb> <rgb-payload>
<rgb> ::= <byte> <byte> <byte>
<hw-type> ::= <server> | <glim-v2> | <glim-v3>
<hostname> ::= <string>
<uptime> ::= <int> ; seconds since start (~68 years before wrap)

<btn-state> ::= 1 | 2 | 3 ; down, held, up respectively (held is sent every 100ms)

<server> ::= 1
<glim-v2> ::= 2  ; 6 (2xRGB) open-collector high power outputs, use <rgb1> and <rgb2> as single pixel each
<glim-v3> ::= 3  ; 2 strings of WS2812 (if WS2811 is used, send the triplets as GBR), RGB1 and RGB2 outputs are each a vector of pixels

<byte> ::= 0-255
<int> ::= <byte> <byte> <byte> <byte> ; int (signed) big endian
<short> ::= <byte> <byte> ; short (signed) big endian
<string> ::= 0 | <strlen> <char-vector>
<strlen> ::= <byte>
<char-vector> ::= <char> | <char> <char-vector>
		 */
	}
}
