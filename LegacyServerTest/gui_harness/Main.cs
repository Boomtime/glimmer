namespace ShadowCreatures.Glimmer {
	using System;
	using System.Net;
	using System.Windows.Forms;
	using System.Diagnostics;
	using System.Threading;
    using ShadowCreatures.Glimmer.Json;

    public partial class Main : Form {
		const int AutoHuntInterval = 1000;

		public enum OutputFunc {
			None,
			Static,
			Rainbow,
			ChannelTest,
			PartyGame,
			PartyNoGame,
			Christmas,
			WindowTest,
			Custom,
		}

		readonly Engine mEngine;
		readonly System.Windows.Forms.Timer mAutoHuntTimer = new System.Windows.Forms.Timer();
		OutputFunc mFunc = OutputFunc.None;
		volatile ISequence mCurrentProgram = new SequenceNull();


		/******************************************************************/

		// party stuff
		SequenceParty mParty;

		ISequence mCustomProgram;

		/*******************************************************************************/

		public Main() {
			InitializeComponent();

			mEngine = new Engine { HistogramSize = cFrameLatency.Size };
			mEngine.HistogramChanged += XHistogramChanged;
			mEngine.Devices.DeviceAdded += XDeviceAdded;
			mEngine.FrameTimeInterval = TimeSpan.FromMilliseconds( 1000.0 / (double)cUpdatesPerSecond.Value );
			cUpdatesPerSecond.ValueChanged += ( s, e ) => mEngine.FrameTimeInterval = TimeSpan.FromMilliseconds( 1000.0 / (double)cUpdatesPerSecond.Value );

			mAutoHuntTimer.Interval = AutoHuntInterval;
			mAutoHuntTimer.Tick += ( s, e ) => mEngine.PingTriggerNextFrame();
			cAutoHunt.CheckedChanged += XAutoHuntCheckedChanged;
			XAutoHuntCheckedChanged( cAutoHunt, EventArgs.Empty );

			AddFunctionRadio( "none", OutputFunc.None );
			AddFunctionRadio( "static", OutputFunc.Static );
			AddFunctionRadio( "rainbow cycle", OutputFunc.Rainbow );
			AddFunctionRadio( "channel test", OutputFunc.ChannelTest );
			AddFunctionRadio( "party game", OutputFunc.PartyGame );
			AddFunctionRadio( "party no game", OutputFunc.PartyNoGame );
			AddFunctionRadio( "christmas", OutputFunc.Christmas );
			AddFunctionRadio( "window test", OutputFunc.WindowTest );
			AddFunctionRadio( "custom...", OutputFunc.Custom );

			cLuminance.ValueChanged += ( s, e ) => mCurrentProgram.Luminance = UILuminanceMultiplier;
			cSaturation.ValueChanged += ( s, e ) => mCurrentProgram.Saturation = UISaturationMultiplier;

			FormClosing += XMainFormClosing;
		}

		void AddFunctionRadio( string text, OutputFunc func ) {
			var r = new RadioButton {
				Text = text,
				Tag = func,
				Checked = ( 0 == cFuncFlow.Controls.Count ),
				AutoSize = true
			};
			r.CheckedChanged += XFuncCheckChanged;
			cFuncFlow.Controls.Add( r );
		}

		double UILuminanceMultiplier => ( (double)cLuminance.Value + 10 ) / (double)cLuminance.Maximum;

		double UISaturationMultiplier => ( (double)cSaturation.Value + 10 ) / (double)cSaturation.Maximum;

		void XAutoHuntCheckedChanged( object sender, EventArgs e ) {
			if( ( sender as CheckBox ).Checked ) {
				mEngine.PingTriggerNextFrame();
				mAutoHuntTimer.Start();
			}
			else {
				mAutoHuntTimer.Stop();
			}
		}

		void XHuntClick( object sender, EventArgs e ) {
			mEngine.PingTriggerNextFrame();
		}

		void XMainLoad( object sender, EventArgs e ) {
			mEngine.Devices.FindOrCreate( "GlimSwarm-101" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-102" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-103" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-104" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-105" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-106" );

			mParty = new SequenceParty( mEngine.Devices );

			mCustomProgram = new SequenceNull();
			cBtnLoad.Click += XBtnLoad_Click;
			cBtnSave.Click += XBtnSave_Click;

			mEngine.Start();
		}

		void XMainFormClosing( object sender, FormClosingEventArgs e ) {
			mEngine.Stop();
		}

		void XBtnSave_Click( object sender, EventArgs e ) {
		}

		void XBtnLoad_Click( object sender, EventArgs e ) {
			var dlg = new OpenFileDialog();
			if( DialogResult.OK == dlg.ShowDialog() ) {
				try {
					using( var file = dlg.OpenFile() ) {
						mCustomProgram = SequenceJson.Load( mEngine.Devices, file );
					}
				}
				catch( Exception ex ) {
					var msg = string.Format( "Error occurred loading JSON. The message was:\r\n{0}", ex.Message );
					if( ex is JsonException ) {
						msg += string.Format( "\r\n\r\nAt:\r\n{0}", ( ex as JsonException ).JsonPath.FullPath );
					}
					MessageBox.Show( msg, "Error loading JSON", MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
			}
		}

		void XDeviceAdded( object sender, DeviceAddedEventArgs e ) {
			UISafeCall( () => cDevices.Controls.Add( new GlimPanel { Device = e.Device } ) );
		}

		void XHistogramChanged( object sender, EngineHistorgramUpdatedEventArgs e ) {
			UISafeCall( () => {
				cFrameLatency.Image = e.Bitmap;
				foreach( GlimPanel d in cDevices.Controls ) {
					d.UpdateStats();
				}
			} );
		}

		void XColourPickClick( object sender, EventArgs e ) {
			using( var dlg = new ColorDialog { Color = cColourSelected.BackColor } ) {
				if( DialogResult.OK == dlg.ShowDialog( this ) ) {
					cColourSelected.BackColor = dlg.Color;
				}
			}
		}

		void UISafeCall( Action func ) {
			if( InvokeRequired ) {
				try {
					ThreadPool.QueueUserWorkItem( f => { try { Invoke( f as Action ); } catch { } }, func );
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

		void XFuncCheckChanged( object sender, EventArgs e ) {
			var rb = sender as RadioButton;
			if( !rb.Checked ) {
				return;
			}
			mFunc = (OutputFunc)rb.Tag;
			switch( mFunc ) {
				case OutputFunc.None:
					mCurrentProgram = new SequenceNull();
					break;
				case OutputFunc.Static:
					mCurrentProgram = new SequenceStatic( mEngine.Devices, () => cColourSelected.BackColor );
					break;
				case OutputFunc.ChannelTest:
					mCurrentProgram = new SequenceChannelTest( mEngine.Devices, clr => cColourSelected.BackColor = clr );
					break;
				case OutputFunc.Christmas:
					mCurrentProgram = new SequenceChristmas( mEngine.Devices );
					break;
				case OutputFunc.Rainbow:
					mCurrentProgram = new SequenceRainbow( mEngine.Devices, 94 / 3, 8 );
					break;
				case OutputFunc.WindowTest:
					mCurrentProgram = new SequenceTestWindow( mEngine.Devices.Find( "GlimSwarm-103" ) );
					break;
				case OutputFunc.PartyGame:
				case OutputFunc.PartyNoGame:
					mParty.EnableGame = ( OutputFunc.PartyGame == mFunc );
					mCurrentProgram = mParty;
					break;
				case OutputFunc.Custom:
					mCurrentProgram = mCustomProgram;
					break;
			}
			mCurrentProgram.Luminance = UILuminanceMultiplier;
			mCurrentProgram.Saturation = UISaturationMultiplier;
			// program changed..
			mEngine.Sequence = mCurrentProgram;
		}

		class GlimDeviceNull : IGlimDevice {
			public GlimDeviceNull() {
				IPEndPoint = new IPEndPoint( IPAddress.Loopback, NetworkServer.DefaultPort );
				HardwareType = HardwareType.GlimV2;
				Uptime = TimeSpan.MaxValue;
				CPU = 1.0f;
				dBm = 0;
				RSSI = WifiRSSI.Excellent;
			}
			public string Hostname => String.Empty;
			public IPEndPoint IPEndPoint { get; }
			public HardwareType HardwareType { get; }
			public TimeSpan Uptime { get; }
			public float CPU { get; }
			public int dBm { get; }
			public WifiRSSI RSSI { get; }
		}

		private void XPartyDebugShotClick( object sender, EventArgs e ) {
			var nd = new GlimDeviceNull();
			mCurrentProgram.ButtonStateChanged( nd, ButtonStatus.Down );
			mCurrentProgram.ButtonStateChanged( nd, ButtonStatus.Up );
		}
	}
}
