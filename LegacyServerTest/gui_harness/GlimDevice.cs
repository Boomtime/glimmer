namespace ShadowCreatures.Glimmer {
	using System.Net;

	class GlimDevice : IGlimDevice {
		readonly string mHostname;
		IGlimPacket mPacket;

		public GlimDevice( string hostname ) {
			mHostname = hostname;
		}

		public IPEndPoint IPEndPoint { get; set; }

		public string DeviceName {
			get { return mHostname; }
		}

		public virtual string AliasName {
			get { return mHostname; }
		}

		public virtual int PixelCount { get; set; }

		public IGlimPacket PixelData {
			get {
				if( null == mPacket ) {
					mPacket = new GlimPacket( this );
				}
				return mPacket;
			}
		}
	}
}
