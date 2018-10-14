namespace ShadowCreatures.Glimmer
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Net;
	using System.Drawing;
	using System.Net.Sockets;

	class GlimDescriptor : IGlimDevice
	{
		public IPEndPoint IPEndPoint;

		readonly string mHostname;
		readonly int mPixelCount;
		IGlimPacket[] mPacket;

		public GlimDescriptor( string hostname, int pixelcount )
		{
			mHostname = hostname;
			mPacket = new IGlimPacket[2];
			mPixelCount = pixelcount;
		}

		public string DeviceName
		{
			get { return mHostname; }
		}

		public string AliasName
		{
			get { return mHostname; }
		}

		public int ChannelCount
		{
			// @todo: really
			get { return 1; }
		}

		public int PixelCount
		{
			get { return mPixelCount; }
		}

		public IGlimPacket GetPacketData( int c )
		{
			if( null == mPacket[c] )
				mPacket[c] = new GlimPacket( this );

			return mPacket[c];
		}

		/// <summary>compile and transmit all constructed colour packets on skt</summary>
		public void Transmit( UdpClient skt )
		{
			for( int ch = 0 ; ch < ChannelCount ; ch++ )
			{
				if( null != mPacket[ch] )
				{
					var payload = new List<byte>();

					// @todo: RGB1 or RGB2 depending on channel
					payload.Add( (byte)NetworkMessage.RGB1 );

					foreach( Color clr in mPacket[ch] )
						payload.AddRange( new byte[] { clr.R, clr.G, clr.B } );

					skt.Send( payload.ToArray(), payload.Count, IPEndPoint );
				}
			}
		}
	}

}
