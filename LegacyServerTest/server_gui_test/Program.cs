namespace server_gui_test
{
	using System;
	using System.Net.Sockets;
	using System.Net;
	using System.Text;
	using System.Threading;
	using System.Drawing;
	using System.Collections.Generic;
	using ShadowCreatures.Luminosity;

	class Program
	{
		static UdpClient skt;
		static List<Flare> flare = new List<Flare>();
		static Random prng = new Random();
		
		const int GlimPort = 1998;
		const int Tick = 25; // 1000 / n = FPS (25 = 40FPS)
		const int Step = 10;

		static ColorReal cylonclr = new ColorReal( Color.Red );
		static int cylonpos = 0;
		static int cylondir = 1;
		static double cylonclrstep = 0.05;

		static List<IPEndPoint> EndPoints = new List<IPEndPoint>();
		/*{
		   //new IPEndPoint( new IPAddress( new byte[] { 192, 168, 1, 105 } ), 1998 ),
		   //new IPEndPoint( new IPAddress( new byte[] { 192, 168, 1, 111 } ), 1998 ),
		   //new IPEndPoint( new IPAddress( new byte[] { 192, 168, 1, 132 } ), 1998 ),
		   new IPEndPoint( new IPAddress( new byte[] { 192, 168, 1, 137 } ), 1998 )
		};*/

		static void Main( string[] args )
		{
			skt = new UdpClient( GlimPort );
			skt.DontFragment = true;
			skt.EnableBroadcast = true;

			GenerateFlare();
			GenerateFlare();
			GenerateFlare();

			cylonclr.Luminance = 0.3;

			foreach( Color clr in ColorWheel( Step ) )
			{
				if( Console.KeyAvailable )
					break;

				CheckForReceived();

				//SendCylon();
				//SendFlares();
				//Send( clr, Color.FromArgb( byte.MaxValue - clr.R, byte.MaxValue - clr.G, byte.MaxValue - clr.B ) );
				Send( clr );
				Thread.Sleep( Tick );
			}
		}

		private static void CheckForReceived()
		{
			IPEndPoint rhost = null;
			byte[] dgram;

			while( skt.Available > 0 )
			{
				dgram = skt.Receive( ref rhost );
				Console.WriteLine( "got some data from " + rhost.Address.ToString() );
			}
		}

		static void Send( byte[] p_payload )
		{
			foreach( var ip in EndPoints )
				skt.Send( p_payload, p_payload.Length, ip );
		}

		static void SendCylon()
		{
			var payload = new byte[150];

			cylonpos += cylondir;
			if( cylonpos >= 49 )
				cylondir = -1;
			else
			if( cylonpos <= 0 )
				cylondir = 1;

			cylonclr.Luminance += cylonclrstep;
			if( cylonclr.Luminance > 0.5 || cylonclr.Luminance < 0.01 )
				cylonclrstep = 0 - cylonclrstep;

			ColorReal overbright = new ColorReal( cylonclr );
			overbright.Luminance += 0.3;
			Color clr = cylonclr;

			int ordinal = cylonpos * 3;

			if( ordinal > 0 )
			{
				payload[ordinal - 3 + 0] = clr.B;
				payload[ordinal - 3 + 1] = clr.R;
				payload[ordinal - 3 + 2] = clr.G;
			}

			if( ordinal <= 144 )
			{
				payload[ordinal + 3 + 0] = clr.B;
				payload[ordinal + 3 + 1] = clr.R;
				payload[ordinal + 3 + 2] = clr.G;
			}

			clr = overbright;
			payload[ordinal + 0] = clr.B;
			payload[ordinal + 1] = clr.R;
			payload[ordinal + 2] = clr.G;

			Send( payload );
		}

		static void Send( Color clr, Color? clr2 = null )
		{
			var payload = new byte[] { clr.R, clr.G, clr.B };

			if( clr2.HasValue )
				payload = new byte[] { clr.R, clr.G, clr.B, clr2.Value.R, clr2.Value.G, clr2.Value.B };

			//Console.WriteLine( "Sending {0},{1},{2}", clr.R, clr.G, clr.B );
			Send( payload );
		}

		static IEnumerable<Color> ColorWheel( int step )
		{
			while( true )
			{
				yield return Color.Red;

				for( int g = 0 ; g < byte.MaxValue ; g += step )
					yield return Color.FromArgb( byte.MaxValue - g, g, 0 );

				yield return Color.FromArgb( 0, byte.MaxValue, 0 ); // Color.Green is FF000800 -- not full green

				for( int b = 0 ; b < byte.MaxValue ; b += step )
					yield return Color.FromArgb( 0, byte.MaxValue - b, b );

				yield return Color.Blue;

				for( int r = 0 ; r < byte.MaxValue ; r += step )
					yield return Color.FromArgb( r, 0, byte.MaxValue - r );
			}
		}

		static void GenerateFlare()
		{
			flare.Add( new Flare(
				prng.Next( 0, 50 ),
				prng.Next( 0, 2 ) == 0 ? Color.FromArgb( 0, 255, 0 ) : Color.Blue,
				prng.NextDouble() / 50 + 0.001 ) );
		}

		/// <summary>updates then sends the 150 bytes of flare data</summary>
		/// <returns></returns>
		static void SendFlares()
		{
			var payload = new byte[150];

			// roll to add new flares
			if( 0 == prng.Next( 0, 20 ) )
				GenerateFlare();

			flare.ForEach( f => f.Step() );
			flare.RemoveAll( f => ( 0 == f.Value.Luminance ) );
			flare.ForEach( f => f.WriteToBuffer( payload ) );

			Send( payload );
		}
	}
}
