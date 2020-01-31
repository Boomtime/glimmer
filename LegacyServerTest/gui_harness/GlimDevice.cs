namespace ShadowCreatures.Glimmer {
    using System;
    using System.Net;

	class GlimDevice : IGlimDevice {
		IGlimPacket mPacket;

		public virtual string Hostname { get; private set; }
		public virtual IPEndPoint IPEndPoint { get; private set; }
		public virtual HardwareType HardwareType { get; private set; }
		public virtual TimeSpan Uptime { get; private set; }
		public virtual float CPU { get; private set; }
		public virtual int dBm { get; private set; }
		public virtual WifiRSSI RSSI { get; private set; }

		public int BootCount { get; private set; }

		public string DeviceName { get; set; }

		public int PixelCount { get; set; }

		public IGlimPacket PixelData {
			get {
				if( null == mPacket ) {
					mPacket = new GlimPacket( PixelCount );
				}
				return mPacket;
			}
		}

		public void UpdateFromNetworkData( IGlimDevice args ) {
			if( 0 == BootCount || args.Uptime.TotalSeconds < Uptime.TotalSeconds ) {
				BootCount++;
			}
			Hostname = args.Hostname;
			IPEndPoint = args.IPEndPoint;
			HardwareType = args.HardwareType;
			Uptime = args.Uptime;
			CPU = args.CPU;
			RSSI = args.RSSI;
			dBm = args.dBm;
		}
	}
}
