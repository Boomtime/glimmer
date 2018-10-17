namespace GlimFake {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using System.Text;
	using System.Threading;

	class Program {
		static void Main( string[] args ) {
			const int GlimPort = 1998;
			var skt = new UdpClient();

			while( !Console.KeyAvailable ) {
				Console.WriteLine( "Any key to exit" );

				var ping = new List<byte>();

				ping.Add( 3 ); // PING
				ping.Add( 3 ); // hw glim-v3 -- should there be a fake hw type?

				ping.Add( 8 ); // char count
				ping.AddRange( Encoding.ASCII.GetBytes( "GlimFake" ) );

				// uptime
				ping.Add( 0 );
				ping.Add( 0 );
				ping.Add( 0 );
				ping.Add( 0 );

				skt.Send( ping.ToArray(), ping.Count, new IPEndPoint( IPAddress.Broadcast, GlimPort ) );

				Thread.Sleep( 1000 );
			}
		}
	}
}
