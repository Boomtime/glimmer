namespace gui_harness
{
	using ShadowCreatures.Glimmer;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using System.Net;
	using System.Collections.Generic;
	using System.Diagnostics;

	public partial class Main : Form
	{
		const int GlimPort = 1998;
		const int SmoothTick = 25; // 1000 / n = FPS (25 = 40FPS)
		const int HardTick = 1000;
		const double HueDuty = 0.002;
		const double HueDutyHeld = 0.01;
		const double HueStride = 0.01;
		const int PixelCountPerString = 150;

		bool ButtonHeld = false;
		ColorReal ButtonColor = null;

		public enum OutputFunc {
			Static,
			Rainbow,
			ChannelTest,
			PartyGame,
		}

		NetworkServer mNetwork;
		Dictionary<string, GlimDescriptor> EndPoints = new Dictionary<string,GlimDescriptor>();
		System.Windows.Forms.Timer CyclingTimer = new System.Windows.Forms.Timer();
		ColorReal CyclingColor = new ColorReal( Color.Red );
		OutputFunc Func;

		public Main()
		{
			InitializeComponent();

			//new Slider();

			CyclingTimer.Interval = SmoothTick;
			CyclingTimer.Tick += ColourCycling_Tick;

			cFuncStatic.Tag = OutputFunc.Static;
			cFuncStatic.CheckedChanged += ctl_funcCheckChanged;

			cFuncRainbow.Tag = OutputFunc.Rainbow;
			cFuncRainbow.CheckedChanged += ctl_funcCheckChanged;

			cFuncChannelTest.Tag = OutputFunc.ChannelTest;
			cFuncChannelTest.CheckedChanged += ctl_funcCheckChanged;
		}

		void Main_Load( object sender, EventArgs e )
		{
			mNetwork = new NetworkServer();
			mNetwork.PingReceived += NetworkPingReceived;
			mNetwork.PongReceived += NetworkPongReceived;
			mNetwork.ButtonStatusReceived += NetworkButtonStatusReceived;
			mNetwork.Listen();
		}

		void ColourCycling_Tick( object sender, EventArgs e )
		{
			if( ButtonHeld )
			{
				ButtonColor.Luminance += HueDuty;
				if( ButtonColor.Luminance > 0.5 )
					ButtonColor.Luminance = 0.1;

				// set all lamps to this colour
				var map = CreateCompletePixelMap();

				for( int p = 0 ; p < map.PixelCount ; p++ )
					map[p] = ButtonColor;

				// transmit all packets
				foreach( var g in EndPoints.Values )
					mNetwork.SendRGB( g.IPEndPoint, g.GetPacketData( 0 ) );

				return;
			}

			// not every function has a tick response
			switch( Func )
			{
				case OutputFunc.Rainbow:
					// Hue is unique in that it's circular
					CyclingColor.Hue += ( ButtonHeld ? HueDutyHeld : HueDuty );
					cColourSelected.BackColor = CyclingColor;
					SendColourWheel( CyclingColor );
					break;
				case OutputFunc.ChannelTest:
					if( Color.Red == cColourSelected.BackColor )
						cColourSelected.BackColor = Color.FromArgb( 0, 0xff, 0 ); // Color.Green;
					else if( Color.Blue == cColourSelected.BackColor )
						cColourSelected.BackColor = Color.Red;
					else
						cColourSelected.BackColor = Color.Blue;
					SendColour( NetworkMessage.RGB1, cColourSelected.BackColor );
					break;
			}
		}

		private void ColourPick_Click( object sender, EventArgs e )
		{
			var dlg = new ColorDialog { Color = cColourSelected.BackColor };

			if( DialogResult.OK == dlg.ShowDialog( this ) )
			{
				cColourSelected.BackColor = dlg.Color;
				SendColour( NetworkMessage.RGB1, cColourSelected.BackColor );
			}
		}

		private void ColourResend_Click( object sender, EventArgs e ) {
			SendColour( NetworkMessage.RGB1, cColourSelected.BackColor );
		}

		private void Hunt_Click( object sender, EventArgs e ) {
			SendPing();
		}

		void ProcessPingPong( NetworkMessage msg, NetworkPingEventArgs e ) {
			switch( e.HardwareType ) {
				case HardwareType.Server:
					PrintLine( "ping/pong of server type (probably us)" );
					break;
				case HardwareType.GlimV2:
				case HardwareType.GlimV3:
					PrintLine( "ping/pong from a " + e.HardwareType.ToString() );
					var pixelcount = Main.PixelCountPerString;
					// @todo: hacky hacky hack hack
					switch( e.Hostname ) {
						case "GlimSwarm-102":
							pixelcount = 50;
							break;
						case "GlimSwarm-103":
							pixelcount = 150;
							break;
						case "GlimSwarm-104":
							pixelcount = 50;
							break;
					}
					var glim = new GlimDescriptor( e.Hostname, pixelcount ) { IPEndPoint = e.IPEndPoint };
					PrintLine( ".. named " + glim.DeviceName );
					if( !EndPoints.ContainsKey( glim.DeviceName ) ) {
						PrintLine( "added to list" );
						EndPoints.Add( glim.DeviceName, glim );
						SafeCall( () => {
							cGlimList.Items.Clear();
							foreach( var g in EndPoints )
								cGlimList.Items.Add( g.Value.DeviceName );
						} );
					}
					// reply if appropriate
					if( NetworkMessage.Ping == msg )
						SendPong( e.IPEndPoint );
					else
						SendButtonGlimmer( glim );
					break;
				default:
					PrintLine( "unknown type ping" );
					break;
			}
		}

		void NetworkPingReceived( object sender, NetworkPingEventArgs e ) {
			ProcessPingPong( NetworkMessage.Ping, e );
		}

		void NetworkPongReceived( object sender, NetworkPingEventArgs e ) {
			ProcessPingPong( NetworkMessage.Pong, e );
		}

		void NetworkButtonStatusReceived( object sender, NetworkButtonStatusEventArgs e ) {
			if( ButtonStatus.Up == e.ButtonStatus ) {
				ButtonHeld = false;
			}
			else
			if( !ButtonHeld ) {
				// find the device
				foreach( var desc in EndPoints.Values ) {
					if( desc.IPEndPoint.Equals( e.IPEndPoint ) ) {
						PrintLine( string.Format( "button changed {0}", e.ButtonStatus ) );
						ButtonHeld = true;
						ButtonColor = Color.White;
						switch( desc.DeviceName ) {
							case "GlimSwarm-103":
								ButtonColor = Color.Blue;
								break;
							case "GlimSwarm-104":
								ButtonColor = Color.Red;
								break;
						}
						break;
					}
				}
			}
		}

		/// <summary>broadcast a ping, or direct to a specific glim if desired</summary>
		void SendPing( IPEndPoint ip = null ) {
			if( null == ip )
				ip = new IPEndPoint( new IPAddress( 0xffffffff ), GlimPort );
			mNetwork.SendPing( ip );
			PrintLine( "sent ping" );
		}

		void SendPong( IPEndPoint ip ) {
			Debug.Assert( null != ip, "must have an IP to answer a ping" );
			mNetwork.SendPong( ip );
		}

		double LuminanceMultiplier
		{
			get { return ( ( (double)ctl_lum.Value + 10 ) / (double)ctl_lum.Maximum ); }
		}

		double SaturationMultiplier
		{
			get { return ( ( (double)ctl_sat.Value + 10 ) / (double)ctl_sat.Maximum ); }
		}

		/// <summary>create a contiguous pixel map of all pixels</summary>
		/// <returns>the complete map</returns>
		GlimPixelMap CreateCompletePixelMap()
		{
			GlimPixelMap map = new GlimPixelMap();

			// @todo: hacky hacky hack hack
			if( EndPoints.ContainsKey( "GlimSwarm-102" ) &&
				EndPoints.ContainsKey( "GlimSwarm-103" ) &&
				EndPoints.ContainsKey( "GlimSwarm-104" ) )
			{
				// create a contiguous pixel map
				map.Add( EndPoints["GlimSwarm-102"].GetPacketData( 0 ) );
				map.Add( EndPoints["GlimSwarm-103"].GetPacketData( 0 ) );
				map.Add( EndPoints["GlimSwarm-104"].GetPacketData( 0 ) );
			}
			else
			{
				// create a contiguous pixel map
				foreach( var ep in EndPoints )
					map.Add( ep.Value.GetPacketData( 0 ) );
			}

			return map;
		}

		void SendColourWheel( Color start )
		{
			var map = CreateCompletePixelMap();

			// map a smooth rainbow across them all
			GenerateVectorColourWheel( map, start );

			// transmit all packets
			foreach( var g in EndPoints.Values )
				mNetwork.SendRGB( g.IPEndPoint, g.GetPacketData( 0 ) );
		}

		void SendButtonGlimmer( GlimDescriptor glim )
		{
			ColorReal min;
			ColorReal max;
			ColorReal onHeld;

			switch( glim.DeviceName )
			{
				case "GlimSwarm-103":
					min = Color.Blue;
					max = Color.Blue;
					onHeld = Color.Blue;
					break;
				case "GlimSwarm-104":
					min = Color.Red;
					max = Color.Red;
					onHeld = Color.Red;
					break;
				default:
					return;
			}

			min.Luminance = 0.1;
			max.Luminance = 0.3;
			onHeld.Luminance = 0.55;

			mNetwork.SendButtonColor( glim.IPEndPoint, min, max, 1024, onHeld );
		}

		void GenerateVectorColourWheel( IGlimPixelMap map, Color start )
		{
			ColorReal rc = start;

			rc.Luminance *= LuminanceMultiplier;
			rc.Saturation *= SaturationMultiplier;

			for( int pixel = 0 ; pixel < map.PixelCount ; pixel++ )
			{
				map[pixel] = rc;
				rc.Hue -= HueStride;
			}
		}

		void SendColour( NetworkMessage msg, Color clr )
		{
			foreach( var glim in EndPoints.Values )
			{
				var pkt = glim.GetPacketData( 0 );
				for( int p = 0 ; p < pkt.Device.PixelCount ; p++ )
					pkt[p] = clr;
				mNetwork.SendRGB( glim.IPEndPoint, pkt );
				break;
			}
		}

		delegate void SafeCallDelegate();

		void SafeCall( SafeCallDelegate func ) {
			if( InvokeRequired ) {
				try {
					Invoke( func );
				}
				catch( Exception ex ) {
					Debug.WriteLine( "*** EXCEPTION:" );
					Debug.WriteLine( ex.ToString() );
				}
			}
			else {
				func();
			}
		}

		void PrintLine( string msg )
		{
			SafeCall( () => ctl_debug.AppendText( msg + "\r\n" ) );
		}

		private void ctl_funcCheckChanged( object sender, EventArgs e )
		{
			Func = (OutputFunc)( sender as System.Windows.Forms.RadioButton ).Tag;

			CyclingTimer.Stop();

			switch( Func )
			{
				case OutputFunc.Static:
					// nothing
					break;
				case OutputFunc.Rainbow:
					CyclingTimer.Interval = SmoothTick;
					CyclingTimer.Start();
					break;
				case OutputFunc.ChannelTest:
					CyclingTimer.Interval = HardTick;
					CyclingTimer.Start();
					break;
			}
		}
	}
}
