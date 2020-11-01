namespace ShadowCreatures.Glimmer {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Net;

	class GlimDevice : IGlimDevice {
		ButtonColour mBtnClr;
		NetworkMessage mPingReply;

		public GlimDevice( string hostname ) {
			HostName = hostname;
			mBtnClr = ButtonColour.Off;
			mPingReply = null;
		}

		public string HostName { get; private set; }
		public IPEndPoint IPEndPoint { get; private set; }
		public HardwareType HardwareType { get; private set; }
		public TimeSpan Uptime { get; private set; }
		public float CPU { get; private set; }
		public int dBm { get; private set; }
		public WifiRSSI RSSI { get; private set; }

		public int BootCount { get; private set; }

		public IDeviceBinding Binding { get; set; }

		//public event EventHandler Changed;

		public void UpdateFromNetworkData( IGlimDevice args ) {
			if( 0 == BootCount || args.Uptime.TotalSeconds < Uptime.TotalSeconds ) {
				BootCount++;
				mBtnClr = ButtonColour.Off; // reload from binding
			}
			IPEndPoint = args.IPEndPoint;
			HardwareType = args.HardwareType;
			Uptime = args.Uptime;
			CPU = args.CPU;
			RSSI = args.RSSI;
			dBm = args.dBm;
			//Changed?.Invoke( this, EventArgs.Empty );
		}

		/// <summary>next packet transmission contains a ping reply</summary>
		public void SendPingReply() {
			mPingReply = new NetworkMessagePingReply();
		}

		public IEnumerable<NetworkMessage> MarshalNetworkMessages() {
			if( null != mPingReply ) {
				var pr = mPingReply;
				mPingReply = null;
				yield return pr;
			}
			if( null == Binding ) {
				// clear down from last binding..
				if( mBtnClr != ButtonColour.Off ) {
					mBtnClr = ButtonColour.Off;
					yield return new NetworkMessageButtonColour( mBtnClr );
				}
			}
			else {
				// using the binding map
				if( Binding.ButtonColour != mBtnClr ) {
					mBtnClr = Binding.ButtonColour;
					yield return new NetworkMessageButtonColour( mBtnClr );
				}
				yield return new NetworkMessageColourVector( Binding.FrameBuffer );
			}
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
			lock( mList ) {
				return mList.TryGetValue( hostname, out GlimDevice device ) ? device : null;
			}
		}

		public GlimDevice Find( IPEndPoint sourceAddress ) {
			lock( mList ) {
				// @todo: is this used a lot? maybe dictionary the IPs too?
				foreach( var g in mList.Values ) {
					if( null != g.IPEndPoint && g.IPEndPoint.Equals( sourceAddress ) ) {
						return g;
					}
				}
			}
			return null;
		}

		public void ResetAllBindings( IEnumerable<IDeviceBinding> devices ) {
			lock( mList ) {
				foreach( var d in mList.Values ) {
					d.Binding = null;
				}
				foreach( var b in devices ) {
					if( mList.TryGetValue( b.HostName, out GlimDevice d ) ) {
						d.Binding = b;
					}
				}
			}
		}

		public event EventHandler<DeviceAddedEventArgs> DeviceAdded;

		public GlimDevice FindOrCreate( string hostname ) {
			var g = Find( hostname );
			if( null == g ) {
				g = new GlimDevice( hostname );
				lock( mList ) {
					mList.Add( g.HostName, g );
				}
				DeviceAdded?.Invoke( this, new DeviceAddedEventArgs( g ) );
			}
			return g;
		}

		public void ForEachDevice( Action<GlimDevice> f, bool onlyDevicesThatHavePinged = true ) {
			lock( mList ) {
				foreach( var g in mList.Values ) {
					if( !onlyDevicesThatHavePinged || null != g.IPEndPoint ) {
						f( g );
					}
				}
			}
		}
	}
}
