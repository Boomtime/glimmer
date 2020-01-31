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
		const double HueStride = 0.01;
		const int PixelCountPerString = 50;
		const double ButtonTimeEpsilonSeconds = 0.25;
		const int AutoHuntInterval = 5000;

		internal interface IProgram {
			void Execute();

			void ButtonStateChanged( IGlimDevice src, ButtonStatus btn );

			double Luminance { set; }
			double Saturation { set; }
		}

		internal abstract class ProgramDefault : IProgram {
			public virtual double Luminance { set; protected get; }
			public virtual double Saturation { set; protected get; }

			public virtual void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
			}

			public static IEnumerable<Color> InfiniteColor( Color clr ) {
				while( true ) {
					yield return clr;
				}
			}

			public abstract void Execute();
		}

		public enum OutputFunc {
			None,
			Static,
			Rainbow,
			ChannelTest,
			PartyGame,
			PartyNoGame,
			Christmas,
			WindowTest
		}

		/// <summary>internal descriptor of a device, extends GlimDevice</summary>
		class GlimDescriptor : GlimDevice {
			public GlimDescriptor( string hostname, Color? partyColor = null ) {
				DeviceName = hostname;
				PartyColor = partyColor;
			}
			public readonly Color? PartyColor;
			public DateTime? ButtonDownTimestamp { get; set; }
			public FxComet FxFloodFill;
		}

		enum GameState {
			Null,
			SynchronizedShotsFired,
			BarrelShotFired,
			CoolDown,
		}

		NetworkServer mNetwork;
		readonly Dictionary<string, GlimDescriptor> mGlimDevices = new Dictionary<string,GlimDescriptor>();
		readonly System.Windows.Forms.Timer mAutoHuntTimer = new Timer();
		readonly System.Windows.Forms.Timer mFrameTimer = new System.Windows.Forms.Timer();
		readonly DateTime mPartyStart = DateTime.Now;
		OutputFunc mFunc = OutputFunc.None;
		IProgram mCurrentProgram;
		GlimDescriptor mGlimRedGun;
		GlimDescriptor mGlimBlueGun;
		GlimPixelMap mPixelMapStars;
		GlimPixelMap mPixelMapRedGun;
		GlimPixelMap mPixelMapBlueGun;
		GlimPixelMap mPixelMapBarrel;
		GlimPixelMap mPixelMapPerimeter;
		FxScale mFxPerimeterRainbow;
		FxScale mFxStarlight;
		DateTime? mButtonUpSynchronizeTimestamp;
		IFx mFxCannonTwinkle;
		GameState mGameState = GameState.Null;
		DateTime mGameCoolDownStart;
		IFx mFxBarrel;
		ProgramChristmas mProgramChristmas;

		public Main() {
			InitializeComponent();

			cUpdatesPerSecond.ValueChanged += CUpdatesPerSecond_ValueChanged;
			mFrameTimer.Interval = (int)( 1000 / cUpdatesPerSecond.Value );
			mFrameTimer.Tick += ColourCycling_Tick;
			mFrameTimer.Start();

			mAutoHuntTimer.Interval = AutoHuntInterval;
			mAutoHuntTimer.Tick += MAutoHuntTimer_Tick;
			cAutoHunt.CheckedChanged += CAutoHunt_CheckedChanged;

			AddFunctionRadio( "none", OutputFunc.None );
			AddFunctionRadio( "static", OutputFunc.Static );
			AddFunctionRadio( "rainbow cycle", OutputFunc.Rainbow );
			AddFunctionRadio( "channel test", OutputFunc.ChannelTest );
			AddFunctionRadio( "party game", OutputFunc.PartyGame );
			AddFunctionRadio( "party no game", OutputFunc.PartyNoGame );
			AddFunctionRadio( "christmas", OutputFunc.Christmas );
			AddFunctionRadio( "window test", OutputFunc.WindowTest );
		}

		void CUpdatesPerSecond_ValueChanged( object sender, EventArgs e ) {
			mFrameTimer.Interval = (int)( 1000 / cUpdatesPerSecond.Value );
		}

		void AddFunctionRadio( string text, OutputFunc func ) {
			var r = new RadioButton {
				Text = text,
				Tag = func,
				Checked = ( 0 == cFuncFlow.Controls.Count ),
				AutoSize = true
			};
			r.CheckedChanged += ctl_funcCheckChanged;
			cFuncFlow.Controls.Add( r );
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

			mProgramChristmas = new ProgramChristmas( gs103.PixelData, gs104.PixelData, gs101.PixelData );

			mPixelMapStars = new GlimPixelMap.Factory { gs102.PixelData };
			mPixelMapRedGun = new GlimPixelMap.Factory { { gs103.PixelData, 0, 100 } };
			mPixelMapBlueGun = new GlimPixelMap.Factory { { gs104.PixelData, 0, 100 } };
			mPixelMapBarrel = new GlimPixelMap.Factory { { gs103.PixelData, 100, 50 } };
			mPixelMapPerimeter = new GlimPixelMap.Factory { { gs103.PixelData, 0, 100 }, { gs104.PixelData, 100, -100 } };

			mFxPerimeterRainbow = new FxScale( new FxRainbow() );
			mFxStarlight = new FxScale( new FxStarlightTwinkle { BaseColor = Color.Yellow } ) { Saturation = 0.3 };

			// comets!
			FxComet t;
			t = new FxComet { BaseColor = Color.Red };
			t.Finished += CbFxCannonBallRed_Finished;
			mGlimRedGun.FxFloodFill = t;
			t = new FxComet { BaseColor = Color.Blue };
			t.Finished += CbFxCannonBallBlue_Finished;
			mGlimBlueGun.FxFloodFill = t;
			t = new FxComet { BaseColor = Color.FromArgb( 0xff, 0, 0xff ) };
			t.Finished += CbFxCannonBallBarrel_Finished;
			mFxBarrel = t;
			mFxCannonTwinkle = new FxStarlightTwinkle {
				BaseColor = Color.FromArgb( 0xff, 0, 0xff ), SpeedFactor = 15.0,
				LuminanceMinima = 0.2, LuminanceMaxima = 0.8 };

			mNetwork = new NetworkServer();
			mNetwork.PingReceived += NetworkPingReceived;
			mNetwork.PongReceived += NetworkPongReceived;
			mNetwork.ButtonStatusReceived += NetworkButtonStatusReceived;
			mNetwork.Listen();
		}

		private void CbFxCannonBallBarrel_Finished( object sender, EventArgs e ) {
			mGameState = GameState.CoolDown;
			mGameCoolDownStart = DateTime.Now;
		}

		private void CbFxCannonBallRed_Finished( object sender, EventArgs e ) {
			SendButtonGlimmer( mGlimRedGun );
			if( GameState.SynchronizedShotsFired == mGameState ) {
				mGameState = GameState.BarrelShotFired;
			}
		}

		private void CbFxCannonBallBlue_Finished( object sender, EventArgs e ) {
			SendButtonGlimmer( mGlimBlueGun );
			if( GameState.SynchronizedShotsFired == mGameState ) {
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
		GlimDescriptor FindOrCreateDevice( IGlimDevice d ) {
			var g = FindDevice( d.Hostname );
			if( null == g ) {
				g = new GlimDescriptor( g.Hostname ) { PixelCount = PixelCountPerString };
				mGlimDevices.Add( g.Hostname, g );
			}
			g.UpdateFromNetworkData( d );
			return g;
		}

		void ColourCycling_Tick( object sender, EventArgs e ) {
			// not every function has a tick response
			switch( mFunc ) {
				case OutputFunc.None:
					break;
				case OutputFunc.Rainbow:
				case OutputFunc.Static:
				case OutputFunc.ChannelTest:
				case OutputFunc.Christmas:
					mCurrentProgram.Luminance = LuminanceMultiplier;
					mCurrentProgram.Saturation = SaturationMultiplier;
					mCurrentProgram.Execute();
					TransmitAllPackets();
					break;
				case OutputFunc.PartyGame:
				case OutputFunc.PartyNoGame:
					var ctx = new FxContextContinuous( mPartyStart );
					mFxPerimeterRainbow.Luminance = PerimeterLuminanceMultiplier;
					mFxPerimeterRainbow.Saturation = SaturationMultiplier;
					mPixelMapPerimeter.Write( mFxPerimeterRainbow.Execute( ctx ) );
					mFxStarlight.Luminance = LuminanceStarlightMultiplier;
					mPixelMapStars.Write( mFxStarlight.Execute( ctx ) );

					if( mGlimRedGun.FxFloodFill.IsRunning ) {
						mPixelMapRedGun.Write( mGlimRedGun.FxFloodFill.Execute( ctx ) );
					}
					if( mGlimBlueGun.FxFloodFill.IsRunning ) {
						mPixelMapBlueGun.Write( mGlimBlueGun.FxFloodFill.Execute( ctx ) );
					}
					if( mFxBarrel.IsRunning ) {
						mPixelMapBarrel.Write( mFxBarrel.Execute( ctx ) );
					}
					AdjustForGameState( ctx );
					TransmitAllPackets();
					break;
			}
		}

		private void ColourPick_Click( object sender, EventArgs e ) {
			var dlg = new ColorDialog { Color = cColourSelected.BackColor };
			if( DialogResult.OK == dlg.ShowDialog( this ) ) {
				cColourSelected.BackColor = dlg.Color;
			}
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
					PrintLine( string.Format( "ping/pong from {0} ({1}) cpu {2}%, wifi strength {3} ({4}dbm)",
						e.Hostname,
						e.HardwareType.ToString(),
						(int)( e.CPU * 100 ),
						e.RSSI.ToString(),
						e.dBm
					) );
					int sc = mGlimDevices.Count;
					var g = FindOrCreateDevice( e );
					SafeCall( UIRebuildGlimList );
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

		void NetworkPingReceived( object sender, NetworkPingEventArgs e ) {
			ProcessPingPong( NetworkMessage.Ping, e );
		}

		void NetworkPongReceived( object sender, NetworkPingEventArgs e ) {
			ProcessPingPong( NetworkMessage.Pong, e );
		}

		void NetworkButtonStatusReceived( object sender, NetworkButtonStatusEventArgs e ) {
			var g = FindDevice( e.SourceAddress );
			if( null == g ) {
				PrintLine( string.Format( "mystery button press received from {0}: Button.{1}", e.SourceAddress.Address, e.ButtonStatus ) );
			}
			PrintLine( string.Format( "{0}: Button.{1}", g.DeviceName, e.ButtonStatus ) );

			if( null != mCurrentProgram ) {
				mCurrentProgram.ButtonStateChanged( g, e.ButtonStatus );
				return;
			}

			// only responding to blue and red guns
			if( null == g || null == g.PartyColor || null == g.FxFloodFill ) {
				return;
			}

			if( ButtonStatus.Up == e.ButtonStatus ) {
				if( OutputFunc.PartyGame == mFunc && g.ButtonDownTimestamp.HasValue ) {
					int tlimit = Math.Min( 100, (int)( ( DateTime.Now - g.ButtonDownTimestamp.Value ).TotalSeconds * 20 ) );
					g.FxFloodFill.Initialize( tlimit );
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
			else if( !g.ButtonDownTimestamp.HasValue ) {
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
		IGlimPixelMap CreateCompletePixelMap() {
			var f = new GlimPixelMap.Factory();
			// create a contiguous pixel map
			foreach( var ep in mGlimDevices ) {
				f.Add( ep.Value.PixelData );
			}
			return f.Compile();
		}

		double PerimeterLuminanceMultiplier {
			get {
				switch( mGameState ) {
					case GameState.Null:
					case GameState.SynchronizedShotsFired:
						return LuminanceMultiplier;
				}
				// take 2 seconds to return to full glow
				return LuminanceMultiplier * ( ( DateTime.Now - mGameCoolDownStart ).TotalSeconds - 1 ) / 2;
			}
		}

		void AdjustForGameState( IFxContext ctx ) {
			switch( mGameState ) {
				case GameState.Null:
				case GameState.SynchronizedShotsFired:
					return;
			}
			double lumscale = 0.0;
			if( GameState.CoolDown == mGameState ) {
				mPixelMapBarrel.Write( ProgramDefault.InfiniteColor( Color.Black ) );
				mPixelMapStars.Write( mFxCannonTwinkle.Execute( ctx ) );
				// take 2 seconds to return to full glow
				lumscale = ( ( DateTime.Now - mGameCoolDownStart ).TotalSeconds - 1 ) / 2;
			}
			if( lumscale < 1 ) {
				// legacy
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
				mNetwork.SendRGB( g.IPEndPoint, g.PixelData.Read() );
			}
		}

		void SendButtonGlimmer( GlimDescriptor g ) {
			ColorReal min;
			ColorReal max;
			ColorReal onHeld;
			if( g.PartyColor.HasValue && null != g.IPEndPoint ) {
				bool flooding = ( null == g.FxFloodFill ? false : g.FxFloodFill.IsRunning );
				if( ( OutputFunc.PartyGame == mFunc && !flooding && GameState.Null == mGameState ) || OutputFunc.Christmas == mFunc ) {
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
			if( !rb.Checked ) {
				return;
			}

			mFunc = (OutputFunc)rb.Tag;

			switch( mFunc ) {
				case OutputFunc.Static:
					mCurrentProgram = new ProgramStatic( cColourSelected, CreateCompletePixelMap() );
					break;
				case OutputFunc.ChannelTest:
					mCurrentProgram = new ProgramChannelTest( cColourSelected, CreateCompletePixelMap() );
					break;
				case OutputFunc.Christmas:
					mCurrentProgram = mProgramChristmas;
					break;
				case OutputFunc.Rainbow:
					mCurrentProgram = new ProgramRainbow( CreateCompletePixelMap(), 94 / 3, 8 );
					break;
				case OutputFunc.PartyGame:
				case OutputFunc.PartyNoGame:
				default:
					mCurrentProgram = null;
					break;
			}
			foreach( var g in AllSeenDevices() ) {
				SendButtonGlimmer( g );
			}
		}

		private void cPartyDebugShot_Click( object sender, EventArgs e ) {
			if( OutputFunc.PartyGame == mFunc && GameState.Null == mGameState ) {
				mGameState = GameState.SynchronizedShotsFired;
				mGlimBlueGun.FxFloodFill.Initialize( 100 );
				mGlimRedGun.FxFloodFill.Initialize( 100 );
				SendButtonGlimmer( mGlimBlueGun );
				SendButtonGlimmer( mGlimRedGun );
			}
		}
	}
}
