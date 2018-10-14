namespace ShadowCreatures.Glimmer
{
	/// <summary>top-level network message identifier</summary>
	public enum NetworkMessage : byte
	{
		RGB1 = 1,
		RGB2 = 2,
		Ping = 3,
		Pong = 4,
		ButtonStatus = 5,
		ButtonColor = 6,
	}

	/// <summary>hardware type byte</summary>
	public enum HardwareType : byte
	{
		Server = 1,
		GlimV2 = 2,
		GlimV3 = 3,
	}

	/// <summary>glim device button status byte</summary>
	public enum ButtonStatus : byte
	{
		Down = 1,
		Held = 2,
		Up = 3,
	}
}
