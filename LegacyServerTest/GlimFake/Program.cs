namespace GlimFake {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using System.Text;
	using System.Threading;

	class Program {
		static void Main() {
			const string HostName = "Daniel's GlimFake";
			const int GlimPort = 1998;
			var skt = new UdpClient();
			var prnd = new Random();
			byte cpu = 50;

			while( !Console.KeyAvailable ) {
				Console.WriteLine( "Any key to exit" );

				var ping = new List<byte> {
					3,	// PING
					4   // hw glim-v4 -- should there be a fake hw type?
				};
				ping.Add( (byte)HostName.Length ); // char count
				ping.AddRange( Encoding.ASCII.GetBytes( HostName ) );

				// uptime
				ping.Add( 0 );
				ping.Add( 0 );
				ping.Add( 0 );
				ping.Add( 0 );

				cpu = Math.Max( (byte)20, Math.Min( (byte)( cpu + prnd.Next( -5, 6 ) ), (byte)200 ) );
				ping.Add( cpu ); // cpu

				ping.Add( 250 ); // wifi

				// net-recv
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
