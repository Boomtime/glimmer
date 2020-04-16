namespace ShadowCreatures.Glimmer {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Net;

	class GlimDevice : IGlimDevice, IGlimPacket {
		NetworkMessage mBtnClr;
		bool mBtnClrChanged;
		NetworkMessage mPingReply;
		Color[] mPixelData;
		bool mPixelDataChanged;

		public GlimDevice( string hostname ) {
			Hostname = hostname;
			mBtnClr = ButtonColourDefault;
			mBtnClrChanged = false;
			mPingReply = null;
			mPixelDataChanged = false;
			PixelCount = 200;
		}

		public string Hostname { get; private set; }
		public IPEndPoint IPEndPoint { get; private set; }
		public HardwareType HardwareType { get; private set; }
		public TimeSpan Uptime { get; private set; }
		public float CPU { get; private set; }
		public int dBm { get; private set; }
		public WifiRSSI RSSI { get; private set; }

		public int BootCount { get; private set; }

		public int PixelCount {
			get { return mPixelData.Length; }
			set { mPixelData = new Color[value]; }
		}

		public void UpdateFromNetworkData( IGlimDevice args ) {
			if( 0 == BootCount || args.Uptime.TotalSeconds < Uptime.TotalSeconds ) {
				BootCount++;
				AssertButtonColour();
			}
			IPEndPoint = args.IPEndPoint;
			HardwareType = args.HardwareType;
			Uptime = args.Uptime;
			CPU = args.CPU;
			RSSI = args.RSSI;
			dBm = args.dBm;
		}

		static NetworkMessageButtonColour ButtonColourDefault {
			get { return new NetworkMessageButtonColour( Color.Black, Color.Black, 0, Color.Black ); }
		}

		/// <summary>next packet transmission contains a glimmer button packet</summary>
		public void SetButtonColour( Color min, Color max, short period, Color onHeld ) {
			mBtnClr = new NetworkMessageButtonColour( min, max, period, onHeld );
			mBtnClrChanged = true;
		}

		/// <summary>next packet transmission contains a glimmer button packet even if it hasn't changed</summary>
		public void AssertButtonColour() {
			mBtnClrChanged = true;
		}

		/// <summary>switch off the glimmer button</summary>
		public void ClearButtonColour() {
			mBtnClr = ButtonColourDefault;
			mBtnClrChanged = true;
		}

		/// <summary>next packet transmission contains a ping reply</summary>
		public void SendPingReply() {
			mPingReply = new NetworkMessagePingReply();
		}

		/// <summary>next packet transmission contains colour vector data even if it hasn't changed</summary>
		public void AssertPixelData() {
			mPixelDataChanged = true;
		}

		public IEnumerable<NetworkUdpPacket> MarshalNetworkPackets() {
			if( null != mPingReply ) {
				var pr = mPingReply;
				mPingReply = null;
				yield return new NetworkUdpPacket( IPEndPoint, pr );
			}
			if( mPixelDataChanged ) {
				mPixelDataChanged = false;
				yield return new NetworkUdpPacket( IPEndPoint, new NetworkMessageColourVector( mPixelData ) );
			}
			if( mBtnClrChanged ) {
				mBtnClrChanged = false;
				yield return new NetworkUdpPacket( IPEndPoint, mBtnClr );
			}
		}

		/// <summary>uses src-alpha to blend dst with 1 minus src-alpha</summary>
		/// <param name="dst">destination (alpha is ignored)</param>
		/// <param name="src">source colour with alpha</param>
		/// <returns>blend</returns>
		static Color Blend( Color dst, Color src ) {
			if( Byte.MaxValue == src.A ) {
				return src;
			}
			if( 0 == src.A ) {
				return dst;
			}
			double sa = (double)src.A / Byte.MaxValue;
			double omsa = 1.0 - sa;

			return Color.FromArgb( (int)( dst.R * omsa + src.R * sa ),
				(int)( dst.G * omsa + src.G * sa ), (int)( dst.B * omsa + src.B * sa ) );
		}

		/// <summary>set pixel preserving blend using src-alpha</summary>
		/// <param name="pixel"></param>
		/// <param name="src"></param>
		public void SetPixel( int pixel, Color src ) {
			mPixelData[pixel] = Blend( mPixelData[pixel], src );
			mPixelDataChanged = true;
		}
	}

	class DeviceAddedEventArgs : EventArgs {
		public DeviceAddedEventArgs( GlimDevice d ) {
			Device = d;
		}
		public GlimDevice Device { get; }
	}

	class GlimManager {
		readonly Dictionary<string, GlimDevice> mList = new Dictionary<string, GlimDevice>();

		public GlimDevice Find( string hostname ) {
			return mList.TryGetValue( hostname, out GlimDevice device ) ? device : null;
		}

		public GlimDevice Find( IPEndPoint sourceAddress ) {
			// @todo: is this used a lot? maybe dictionary the IPs too?
			foreach( var g in AllSeenDevices() ) {
				if( g.IPEndPoint.Equals( sourceAddress ) ) {
					return g;
				}
			}
			return null;
		}

		public event EventHandler<DeviceAddedEventArgs> DeviceAdded;

		public GlimDevice FindOrCreate( string hostname ) {
			var g = Find( hostname );
			if( null == g ) {
				g = new GlimDevice( hostname );
				mList.Add( g.Hostname, g );
				DeviceAdded?.Invoke( this, new DeviceAddedEventArgs( g ) );
			}
			return g;
		}

		public IEnumerable<GlimDevice> All() {
			return mList.Values;
		}

		/// <summary>enumerate all devices that have an observed IP address</summary>
		/// <returns></returns>
		public IEnumerable<GlimDevice> AllSeenDevices() {
			foreach( var g in mList.Values ) {
				if( null != g.IPEndPoint ) {
					yield return g;
				}
			}
		}

		/// <summary>create a contiguous map of all pixels from AllSeenDevices</summary>
		/// <returns>the complete map</returns>
		public IGlimPixelMap CreateCompletePixelMap() {
			// create a contiguous pixel map
			return new GlimPixelMap.Factory { AllSeenDevices() }.Compile();
		}
	}
}
