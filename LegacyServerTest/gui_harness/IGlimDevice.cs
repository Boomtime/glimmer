namespace ShadowCreatures.Glimmer {
	using System;
	using System.Net;

	/// <summary>hardware type byte</summary>
	public enum HardwareType : byte {
		Server = 1,
		GlimV2 = 2,
		GlimV3 = 3,
		GlimV4 = 4,
	}

	/// <summary>glim device button status byte</summary>
	public enum ButtonStatus : byte {
		Down = 1,
		Held = 2,
		Up = 3,
	}

	public enum WifiRSSI : int {
		None = -200, // everything beyond Terrible or unknown, not likely to get a packet through if it exists at all
		Terrible = -87, // (down to), will get some comms, but might be tragic
		Weak = -75, // (down to), reliable only if it's consistent
		Good = -57, // (down to), very solid and reliable, can handle variation
		Excellent = -33, // down to -33, on top of the router, may have caught fire
	}

	public interface IGlimDevice {
		string Hostname { get; }
		IPEndPoint IPEndPoint { get; }
		HardwareType HardwareType { get; }
		TimeSpan Uptime { get; }
		float CPU { get; }
		int dBm { get; }
		WifiRSSI RSSI { get; }
	}
}
