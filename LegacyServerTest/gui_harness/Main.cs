namespace gui_harness {
	using ShadowCreatures.Glimmer;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using System.Net;
	using System.Collections.Generic;
	using System.Diagnostics;

	public partial class Main : Form {
		const int SmoothTick = 25; // 1000 / n = FPS (25 = 40FPS)
		const int ChannelTestTick = 1000;
		const double HueDuty = 0.002;
		const double HueStride = 0.01;
		const int PixelCountPerString = 50;
		const double ButtonTimeEpsilonSeconds = 0.25;
		const int AutoHuntInterval = 5000;

		public enum OutputFunc {
			Static,
			Rainbow,
			ChannelTest,
			PartyGame,
			PartyNoGame,
			Christmas,
		}

		/// <summary>internal descriptor of a device, extends GlimDevice</summary>
		class GlimDescriptor : GlimDevice {
			public GlimDescriptor( string hostname, Color? partyColor = null ) : base( hostname ) {
				PartyColor = partyColor;
			}
			public readonly Color? PartyColor;
			public DateTime? ButtonDownTimestamp { get; set; }
			public FxCannonBall FxFloodFill;

			public int BootCount = 0;
			public TimeSpan BootTime = TimeSpan.Zero;
		}

		NetworkServer mNetwork;
		Dictionary<string, GlimDescriptor> mGlimDevices = new Dictionary<string,GlimDescriptor>();
		IGlimPixelMap mGlimPixelMapContiguous = new GlimPixelMap();
		System.Windows.Forms.Timer mAutoHuntTimer = new Timer();
		System.Windows.Forms.Timer mCyclingTimer = new System.Windows.Forms.Timer();
		ColorReal mCyclingColor = new ColorReal( Color.Red );
		DateTime mPartyStart = DateTime.Now;
		OutputFunc mFunc;
		GlimDescriptor mGlimRedGun;
		GlimDescriptor mGlimBlueGun;
		GlimPixelMap mPixelMapStars;
		GlimPixelMap mPixelMapRedGun;
		GlimPixelMap mPixelMapBlueGun;
		GlimPixelMap mPixelMapBarrel;
		GlimPixelMap mPixelMapPerimeter;
		FxBase mFxPerimeterRainbow;
		FxScale mFxPerimeterScale;
		FxStarlightTwinkle mFxStarlight;
		FxScale mFxStarlightScale;
		DateTime? mButtonUpSynchronizeTimestamp;
		FxStarlightTwinkle mFxCannonTwinkle;
		ProgramChristmas mProgramChristmas;

		enum GameState {
			Null,
			SynchronizedShotsFired,
			BarrelShotFired,
			CoolDown,
		}
		GameState mGameState = GameState.Null;
		DateTime mGameCoolDownStart;
		FxCannonBall mFxBarrel;

		public Main() {
			InitializeComponent();

			mCyclingTimer.Interval = SmoothTick;
			mCyclingTimer.Tick += ColourCycling_Tick;

			mAutoHuntTimer.Interval = AutoHuntInterval;
			mAutoHuntTimer.Tick += MAutoHuntTimer_Tick;
			cAutoHunt.CheckedChanged += CAutoHunt_CheckedChanged;

			cFuncStatic.Tag = OutputFunc.Static;
			cFuncStatic.CheckedChanged += ctl_funcCheckChanged;

			cFuncRainbow.Tag = OutputFunc.Rainbow;
			cFuncRainbow.CheckedChanged += ctl_funcCheckChanged;

			cFuncChannelTest.Tag = OutputFunc.ChannelTest;
			cFuncChannelTest.CheckedChanged += ctl_funcCheckChanged;

			cFuncPartyGame.Tag = OutputFunc.PartyGame;
			cFuncPartyGame.CheckedChanged += ctl_funcCheckChanged;

			cFuncPartyNoGame.Tag = OutputFunc.PartyNoGame;
			cFuncPartyNoGame.CheckedChanged += ctl_funcCheckChanged;

			cFuncChristmas.Tag = OutputFunc.Christmas;
			cFuncChristmas.CheckedChanged += ctl_funcCheckChanged;
		}

		private void CAutoHunt_CheckedChanged( object sender, EventArgs e ) {
			if( ( sender as CheckBox ).Checked ) {
				mAutoHuntTimer.Start();
				SendPing();
			}
			else {
				mAutoHuntTimer.Stop();
			}
		}

		private void MAutoHuntTimer_Tick( object sender, EventArgs e ) {
			SendPing();
		}

		void Main_Load( object sender, EventArgs e ) {
			var gs101 = new GlimDescriptor( "GlimSwarm-101", Color.Aqua ) { PixelCount = 200 };
			var gs102 = new GlimDescriptor( "GlimSwarm-102" ) { PixelCount = 100 };
			var gs103 = mGlimRedGun = new GlimDescriptor( "GlimSwarm-103", Color.Red ) { PixelCount = 150 };
			var gs104 = mGlimBlueGun = new GlimDescriptor( "GlimSwarm-104", Color.Blue ) { PixelCount = 200 };

			mGlimDevices.Add( gs101.DeviceName, gs101 );
			mGlimDevices.Add( gs102.DeviceName, gs102 );
			mGlimDevices.Add( gs103.DeviceName, gs103 );
			mGlimDevices.Add( gs104.DeviceName, gs104 );

			mProgramChristmas = new ProgramChristmas( gs103, gs104, gs101 );

			mPixelMapStars = new GlimDeviceMap { gs102 }.Compile();
			mPixelMapRedGun = new GlimDeviceMap { { gs103, 0, 100 } }.Compile();
			mPixelMapBlueGun = new GlimDeviceMap { { gs104, 0, 100 } }.Compile();
			mPixelMapBarrel = new GlimDeviceMap { { gs103, 100, 50 } }.Compile();
			mPixelMapPerimeter = new GlimDeviceMap { { gs103, 0, 100 }, { gs104, 100, -100 } }.Compile();

			mFxPerimeterRainbow = new FxRainbow( mPixelMapPerimeter );
			mFxPerimeterScale = new FxScale( mPixelMapPerimeter );
			mFxStarlight = new FxStarlightTwinkle( mPixelMapStars ) { BaseColor = Color.Yellow };
			mFxStarlightScale = new FxScale( mPixelMapStars ) { SaturationScale = 0.3 };
			mGlimRedGun.FxFloodFill = new FxCannonBall( mPixelMapRedGun ) { BaseColor = Color.Red };
			mGlimRedGun.FxFloodFill.Finished += CbFxCannonBallRed_Finished;
			mGlimBlueGun.FxFloodFill = new FxCannonBall( mPixelMapBlueGun ) { BaseColor = Color.Blue };
			mGlimBlueGun.FxFloodFill.Finished += CbFxCannonBallBlue_Finished;
			mFxBarrel = new FxCannonBall( mPixelMapBarrel ) { BaseColor = Color.FromArgb( 0xff, 0, 0xff ) };
			mFxBarrel.Finished += CbFxCannonBallBarrel_Finished;
			mFxCannonTwinkle = new FxStarlightTwinkle( mPixelMapStars ) {
				BaseColor = Color.FromArgb( 0xff, 0, 0xff ), SpeedFactor = 15.0,
				LuminanceMinima = 0.2, LuminanceMaxima = 0.8 };

			mNetwork = new NetworkServer();
			mNetwork.PingReceived += NetworkPingReceived;
			mNetwork.PongReceived += NetworkPongReceived;
			mNetwork.ButtonStatusReceived += NetworkButtonStatusReceived;
			mNetwork.Listen();
		}

		private void CbFxCannonBallBarrel_Finished( object sender, EventArgs e ) {
			( sender as FxCannonBall ).Stop();
			mGameState = GameState.CoolDown;
			mGameCoolDownStart = DateTime.Now;
		}

		private void CbFxCannonBallRed_Finished( object sender, EventArgs e ) {
			( sender as FxCannonBall ).Stop();
			SendButtonGlimmer( mGlimRedGun );
			if( GameState.SynchronizedShotsFired == mGameState ) {
				mFxBarrel.Start( 50 );
				mGameState = GameState.BarrelShotFired;
			}
		}

		private void CbFxCannonBallBlue_Finished( object sender, EventArgs e ) {
			( sender as FxCannonBall ).Stop();
			SendButtonGlimmer( mGlimBlueGun );
			if( GameState.SynchronizedShotsFired == mGameState ) {
				mFxBarrel.Start( 50 );
				mGameState = GameState.BarrelShotFired;
			}
		}

		/// <summary>enumerate all devices that have an observed IP address</summary>
		/// <returns></returns>
		IEnumerable<GlimDescriptor> AllSeenDevices() {
			foreach( var g in mGlimDevices.Values ) {
				if( null != g.IPEndPoint ) {
					yield return g;
				}
			}
		}

		/// <summary>find a device by IPAddress</summary>
		/// <param name="sourceAddress"></param>
		/// <returns></returns>
		GlimDescriptor FindDevice( IPEndPoint sourceAddress ) {
			foreach( var g in AllSeenDevices() ) {
				if( g.IPEndPoint.Equals( sourceAddress ) ) {
					return g;
				}
			}
			return null;
		}

		/// <summary>find a device by hostname</summary>
		/// <param name="hostname"></param>
		/// <returns></returns>
		GlimDescriptor FindDevice( string hostname ) {
			return mGlimDevices.TryGetValue( hostname, out GlimDescriptor dev ) ? dev : null;
		}

		/// <summary>fine by hostname, create if necessary, update the sourceAddress when located</summary>
		/// <param name="hostname">name of the device to find or create</param>
		/// <param name="sourceAddress">optional, if not null, authoritative sourceAddress (has been seen at this address)</param>
		/// <returns></returns>
		GlimDescriptor FindOrCreateDevice( string hostname, IPEndPoint sourceAddress ) {
			var g = FindDevice( hostname );
			if( null == g ) {
				g = new GlimDescriptor( hostname ) { IPEndPoint = sourceAddress, PixelCount = PixelCountPerString };
				mGlimDevices.Add( hostname, g );
			}
			else {
				g.IPEndPoint = sourceAddress;
			}
			return g;
		}

		void ColourCycling_Tick( object sender, EventArgs e ) {
			// not every function has a tick response
			switch( mFunc ) {
				case OutputFunc.Rainbow:
					// Hue is unique in that it's circular
					mCyclingColor.Hue += HueDuty;
					cColourSelected.BackColor = mCyclingColor;
					// map a smooth rainbow across them all
					GenerateVectorColourWheel( mGlimPixelMapContiguous, mCyclingColor );
					TransmitAllPackets();
					break;
				case OutputFunc.ChannelTest:
					if( Color.Red == cColourSelected.BackColor )
						cColourSelected.BackColor = Color.FromArgb( 0, 0xff, 0 ); // Color.Green;
					else if( Color.Blue == cColourSelected.BackColor )
						cColourSelected.BackColor = Color.Red;
					else
						cColourSelected.BackColor = Color.Blue;
					SendColour( cColourSelected.BackColor );
					break;
				case OutputFunc.PartyGame:
				case OutputFunc.PartyNoGame:
					var ctx = new FxContextUnbounded( mPartyStart );
					mFxPerimeterRainbow.Execute( ctx );
					mFxPerimeterScale.LuminanceScale = LuminanceMultiplier;
					mFxPerimeterScale.SaturationScale = SaturationMultiplier;
					mFxPerimeterScale.Execute( ctx );
					mFxStarlight.Execute( ctx );
					mFxStarlightScale.LuminanceScale = LuminanceStarlightMultiplier;
					mFxStarlightScale.Execute( ctx );
					mGlimRedGun.FxFloodFill.Execute( ctx );
					mGlimBlueGun.FxFloodFill.Execute( ctx );
					mFxBarrel.Execute( ctx );
					AdjustForGameState( ctx );
					TransmitAllPackets();
					break;
				case OutputFunc.Christmas:
					mProgramChristmas.Execute( LuminanceMultiplier, SaturationMultiplier );
					TransmitAllPackets();
					break;
			}
		}

		private void ColourPick_Click( object sender, EventArgs e ) {
			var dlg = new ColorDialog { Color = cColourSelected.BackColor };

			if( DialogResult.OK == dlg.ShowDialog( this ) ) {
				cColourSelected.BackColor = dlg.Color;
				SendColour( cColourSelected.BackColor );
			}
		}

		private void ColourResend_Click( object sender, EventArgs e ) {
			SendColour( cColourSelected.BackColor );
		}

		private void Hunt_Click( object sender, EventArgs e ) {
			SendPing();
		}

		/// <summary>direct call to UI objects to rebuild the glim list</summary>
		void UIRebuildGlimList() {
			cGlimList.Items.Clear();
			foreach( var g in AllSeenDevices() ) {
				cGlimList.Items.Add( string.Format( "{0} ({1})", g.DeviceName, g.BootCount ) );
			}
		}

		void ProcessPingPong( NetworkMessage msg, NetworkPingEventArgs e ) {
			switch( e.HardwareType ) {
				case HardwareType.Server:
					PrintLine( "ping/pong of server type (probably us)" );
					break;
				case HardwareType.GlimV2:
				case HardwareType.GlimV3:
				case HardwareType.GlimV4:
					PrintLine( string.Format( "ping/pong from {0} ({1}) cpu {2}%, wifi strength {3}",
						e.Hostname,
						e.HardwareType.ToString(),
						(int)( e.CPU * 100 ),
						e.RSSI.ToString()
					) );
					var g = FindOrCreateDevice( e.Hostname, e.SourceAddress );
					UpdateDevice( g, e.Uptime );
					mGlimPixelMapContiguous = CreateCompletePixelMap();
					// reply if appropriate
					if( NetworkMessage.Ping == msg ) {
						SendPong( e.SourceAddress );
					}
					SendButtonGlimmer( g );
					break;
				default:
					PrintLine( "unknown type ping" );
					break;
			}
		}

		private void UpdateDevice( GlimDescriptor g, TimeSpan uptime ) {
			if( 0 == g.BootCount || uptime.TotalSeconds < g.BootTime.TotalSeconds ) {
				g.BootCount ++;
				PrintLine( "added to display list or device crashed" );
				SafeCall( UIRebuildGlimList );
			}
			g.BootTime = uptime;
		}

		void NetworkPingReceived( object sender, NetworkPingEventArgs e ) {
			ProcessPingPong( NetworkMessage.Ping, e );
		}

		void NetworkPongReceived( object sender, NetworkPingEventArgs e ) {
			ProcessPingPong( NetworkMessage.Pong, e );
		}

		void NetworkButtonStatusReceived( object sender, NetworkButtonStatusEventArgs e ) {
			var g = FindDevice( e.SourceAddress );

			if( OutputFunc.Christmas == mFunc ) {
				mProgramChristmas.ButtonStateChanged( e.ButtonStatus );
				return;
			}

			// only responding to blue and red guns
			if( null == g || null == g.PartyColor || null == g.FxFloodFill ) {
				return;
			}
			if( ButtonStatus.Up == e.ButtonStatus ) {
				if( OutputFunc.PartyGame == mFunc ) {
					int tlimit = Math.Min( 100, (int)( ( DateTime.Now - g.ButtonDownTimestamp.Value ).TotalSeconds * 20 ) );
					g.FxFloodFill.Start( tlimit );
					SendButtonGlimmer( g );

					if( mButtonUpSynchronizeTimestamp.HasValue ) {
						// compare current time to Up sync time to see if it's game on
						double diffsec = ( DateTime.Now - mButtonUpSynchronizeTimestamp.Value ).TotalSeconds;
						if( ButtonTimeEpsilonSeconds > diffsec && 100 == tlimit ) {
							mGameState = GameState.SynchronizedShotsFired;
							PrintLine( "mGameState -> " + mGameState.ToString() );
						}
						else {
							mButtonUpSynchronizeTimestamp = null;
						}
					}
					else if( mGlimBlueGun.ButtonDownTimestamp.HasValue && mGlimRedGun.ButtonDownTimestamp.HasValue ) {
						// both buttons were down.. check for epilson sync
						double diffsec = ( mGlimRedGun.ButtonDownTimestamp.Value - mGlimBlueGun.ButtonDownTimestamp.Value ).TotalSeconds;
						if( ButtonTimeEpsilonSeconds > Math.Abs( diffsec ) ) {
							mButtonUpSynchronizeTimestamp = DateTime.Now;
							PrintLine( "down push was synchronized" );
						}
					}
				}
				g.ButtonDownTimestamp = null;
			}
			else
			if( !g.ButtonDownTimestamp.HasValue ) {
				PrintLine( string.Format( "{0} button changed {1}", g.DeviceName, e.ButtonStatus ) );
				g.ButtonDownTimestamp = DateTime.Now;
			}
		}

		/// <summary>broadcast a ping, or direct to a specific glim if desired</summary>
		void SendPing( IPEndPoint ip = null ) {
			if( null == ip ) {
				ip = new IPEndPoint( IPAddress.Broadcast, NetworkServer.DefaultPort );
			}
			mNetwork.SendPing( ip );
			PrintLine( "sent ping" );
		}

		void SendPong( IPEndPoint ip ) {
			Debug.Assert( null != ip, "must have an IP to answer a ping" );
			mNetwork.SendPong( ip );
		}

		double LuminanceStarlightMultiplier {
			get { return ( ( (double)cStarlightLum.Value + 10 ) / (double)cStarlightLum.Maximum ); }
		}

		double LuminanceMultiplier {
			get { return ( ( (double)cLuminance.Value + 10 ) / (double)cLuminance.Maximum ); }
		}

		double SaturationMultiplier {
			get { return ( ( (double)cSaturation.Value + 10 ) / (double)cSaturation.Maximum ); }
		}

		/// <summary>create a contiguous pixel map of all pixels</summary>
		/// <returns>the complete map</returns>
		GlimPixelMap CreateCompletePixelMap() {
			GlimPixelMap map = new GlimPixelMap();
			// create a contiguous pixel map
			foreach( var ep in mGlimDevices ) {
				map.Add( ep.Value.PixelData );
			}
			return map;
		}

		void AdjustForGameState( IFxContext ctx ) {
			switch( mGameState ) {
				case GameState.Null:
				case GameState.SynchronizedShotsFired:
					return;
			}
			double lumscale = 0.0;
			if( GameState.CoolDown == mGameState ) {
				foreach( var c in mPixelMapBarrel ) {
					c.CopyFrom( Color.Black );
				}
				mFxCannonTwinkle.Execute( ctx );
				lumscale = ( ( DateTime.Now - mGameCoolDownStart ).TotalSeconds - 1 ) / 2;
			}
			if( lumscale < 1 ) {
				mFxPerimeterScale.LuminanceScale = lumscale;
				mFxPerimeterScale.SaturationScale = 1.0;
				mFxPerimeterScale.Execute( ctx );
				//foreach( var c in mPixelMapPerimeter ) {
				//	c.Luminance -= lumdiff;
				//}
			}
			else {
				mGameState = GameState.Null;
				SendButtonGlimmer( mGlimBlueGun );
				SendButtonGlimmer( mGlimRedGun );
			}
		}

		void TransmitAllPackets() {
			// transmit all packets
			foreach( var g in AllSeenDevices() ) {
				mNetwork.SendRGB( g.IPEndPoint, g.PixelData );
			}
		}

		void SendButtonGlimmer( GlimDescriptor g ) {
			ColorReal min;
			ColorReal max;
			ColorReal onHeld;
			if( g.PartyColor.HasValue ) {
				if( ( OutputFunc.PartyGame == mFunc && !g.FxFloodFill.IsRunning && GameState.Null == mGameState ) ||
					OutputFunc.Christmas == mFunc ) {
					min = g.PartyColor.Value;
					min.Luminance = 0.1;
					max = g.PartyColor.Value;
					max.Luminance = 0.3;
					onHeld = g.PartyColor.Value;
					onHeld.Luminance = 0.55;
				}
				else {
					min = Color.Black;
					max = Color.Black;
					onHeld = Color.Black;
				}
				mNetwork.SendButtonColor( g.IPEndPoint, min, max, 1024, onHeld );
			}
		}

		void GenerateVectorColourWheel( IGlimPixelMap map, Color start ) {
			ColorReal rc = start;

			rc.Luminance *= LuminanceMultiplier;
			rc.Saturation *= SaturationMultiplier;

			for( int pixel = 0 ; pixel < map.PixelCount ; pixel++ )
			{
				map[pixel] = rc;
				rc.Hue -= HueStride;
			}
		}

		void SendColour( Color clr ) {
			foreach( var g in AllSeenDevices() ) {
				var pkt = g.PixelData;
				for( int p = 0 ; p < pkt.Device.PixelCount ; p++ ) {
					pkt[p] = clr;
				}
				mNetwork.SendRGB( g.IPEndPoint, pkt );
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

		void PrintLine( string msg ) {
			SafeCall( () => ctl_debug.AppendText( msg + "\r\n" ) );
		}

		void ctl_funcCheckChanged( object sender, EventArgs e ) {
			var rb = sender as RadioButton;
			if( !rb.Checked )
				return;

			mFunc = (OutputFunc)rb.Tag;
			mCyclingTimer.Stop();

			switch( mFunc ) {
				case OutputFunc.Static:
					// nothing
					mCyclingTimer.Stop();
					break;
				case OutputFunc.ChannelTest:
					mCyclingTimer.Interval = ChannelTestTick;
					mCyclingTimer.Start();
					break;
				case OutputFunc.Rainbow:
				case OutputFunc.PartyGame:
				case OutputFunc.PartyNoGame:
					mCyclingTimer.Interval = SmoothTick;
					mCyclingTimer.Start();
					break;
				case OutputFunc.Christmas:
					mCyclingTimer.Interval = SmoothTick;
					mCyclingTimer.Start();
					break;
			}
			foreach( var g in AllSeenDevices() ) {
				SendButtonGlimmer( g );
			}
		}

		private void cPartyDebugShot_Click( object sender, EventArgs e ) {
			if( OutputFunc.PartyGame == mFunc && GameState.Null == mGameState ) {
				mGameState = GameState.SynchronizedShotsFired;
				mGlimBlueGun.FxFloodFill.Start( 100 );
				mGlimRedGun.FxFloodFill.Start( 100 );
				SendButtonGlimmer( mGlimBlueGun );
				SendButtonGlimmer( mGlimRedGun );
			}
		}
	}
}
