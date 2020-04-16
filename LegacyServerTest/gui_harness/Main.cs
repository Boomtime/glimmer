namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Utility;
	using System;
	using System.Net;
	using System.Windows.Forms;
	using System.Diagnostics;
    using System.Threading;
	using System.Text.Json;

    public partial class Main : Form {
		const int AutoHuntInterval = 1000;
		static readonly TimeSpan LongSleepThreshold = TimeSpan.FromMilliseconds( 15 );
		static readonly TimeSpan GraphPinInterval = TimeSpan.FromMilliseconds( 1000 );

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

		readonly NetworkServer mNetwork = new NetworkServer();
		readonly GlimManager mDevices = new GlimManager();
		readonly System.Windows.Forms.Timer mAutoHuntTimer = new System.Windows.Forms.Timer();
		OutputFunc mFunc = OutputFunc.None;
		volatile ISequence mCurrentProgram = new SequenceNull();

		Thread mWorkerThread;
		TimeSpan mFrameTimeInterval;
		bool mWorkerPingTrigger = false;
		readonly Histogram mWorkerFrameHistogram;
		readonly Action<GlimDevice> mWorkerOnPingReplyRecv;


		/******************************************************************/

		// party stuff
		SequenceParty mParty;

		ISequence mCustomProgram;

		/*******************************************************************************/

		public Main() {
			InitializeComponent();

			mFrameTimeInterval = TimeSpan.FromMilliseconds( 1000.0 / (double)cUpdatesPerSecond.Value );
			cUpdatesPerSecond.ValueChanged += ( s, e ) => mFrameTimeInterval = TimeSpan.FromMilliseconds( 1000.0 / (double)cUpdatesPerSecond.Value );

			mAutoHuntTimer.Interval = AutoHuntInterval;
			mAutoHuntTimer.Tick += ( s, e ) => mWorkerPingTrigger = true;
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

			mWorkerFrameHistogram = new Histogram( cFrameLatency.Size );
			mWorkerOnPingReplyRecv = XPingReplyReceived;
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
				mWorkerPingTrigger = true;
				mAutoHuntTimer.Start();
			}
			else {
				mAutoHuntTimer.Stop();
			}
		}

		void XHuntClick( object sender, EventArgs e ) {
			mWorkerPingTrigger = true;
		}

		void XMainLoad( object sender, EventArgs e ) {
			mDevices.FindOrCreate( "GlimSwarm-101" );
			mDevices.FindOrCreate( "GlimSwarm-102" );
			mDevices.FindOrCreate( "GlimSwarm-103" );
			mDevices.FindOrCreate( "GlimSwarm-104" );
			mDevices.FindOrCreate( "GlimSwarm-105" );
			mDevices.FindOrCreate( "GlimSwarm-106" );

			mParty = new SequenceParty( mDevices );

			mCustomProgram = new SequenceNull();
			cBtnLoad.Click += XBtnLoad_Click;
			cBtnSave.Click += XBtnSave_Click;

			mWorkerThread = new Thread( WorkerThreadMain );
			mWorkerThread.Start();

			cFrameLatency.Image = mWorkerFrameHistogram.GenerateBitmap();
		}

		void XBtnSave_Click( object sender, EventArgs e ) {
		}

		void XBtnLoad_Click( object sender, EventArgs e ) {
			var dlg = new OpenFileDialog();
			if( DialogResult.OK == dlg.ShowDialog() ) {
				dlg.OpenFile();
				// load doc
			}
		}

		void XPingReplyReceived( GlimDevice d ) {
			if( 0 == d.BootCount ) {
				UISafeCall( () => { cDevices.Controls.Add( new GlimPanel { Device = d } ); } );
			}
		}

		void WorkerProcessTriggers() {
			if( mWorkerPingTrigger ) {
				mNetwork.Send( new IPEndPoint( IPAddress.Broadcast, NetworkServer.DefaultPort ), new NetworkMessagePing() );
				mWorkerPingTrigger = false;
			}
		}

		void WorkerProcessButtonStatus( NetworkButtonStatusEventArgs e ) {
			GlimDevice g;
			lock( mDevices ) {
				g = mDevices.Find( e.SourceAddress );
			}
			if( null == g ) {
				Debug.WriteLine( "mystery button press received from {0}: Button.{1}", e.SourceAddress.Address, e.ButtonStatus );
				return;
			}
			mCurrentProgram.ButtonStateChanged( g, e.ButtonStatus );
		}

		void WorkerProcessPingPong( NetworkPingEventArgs e ) {
			switch( e.HardwareType ) {
				case HardwareType.Server:
					Debug.WriteLine( "ping/pong of server type (probably us)" );
					break;
				case HardwareType.GlimV2:
				case HardwareType.GlimV3:
				case HardwareType.GlimV4:
					Debug.WriteLine( string.Format( "ping/pong from {0} ({1}) cpu {2}%, wifi strength {3} ({4}dbm)",
						e.Hostname, e.HardwareType.ToString(), (int)( e.CPU * 100 ), e.RSSI.ToString(), e.dBm ) );
					GlimDevice g;
					lock( mDevices ) {
						g = mDevices.FindOrCreate( e.Hostname );
					}
					mWorkerOnPingReplyRecv( g );
					g.UpdateFromNetworkData( e );
					// reply if appropriate
					if( NetworkMessageType.Ping == e.MessageType ) {
						g.SendPingReply();
					}
					g.AssertButtonColour();
					break;
				default:
					Debug.WriteLine( "unknown type ping" );
					break;
			}
		}

		void WorkerProcessNetworkTraffic() {
			NetworkEventArgs msg;
			while( null != ( msg = mNetwork.Receive() ) ) {
				switch( msg.MessageType ) {
					case NetworkMessageType.Ping:
					case NetworkMessageType.Pong:
						WorkerProcessPingPong( msg as NetworkPingEventArgs );
						break;
					case NetworkMessageType.ButtonStatus:
						WorkerProcessButtonStatus( msg as NetworkButtonStatusEventArgs );
						break;
				}
			}
		}

		void WorkerThreadMain() {
			ISequence seq;
			Stopwatch progTime = new Stopwatch();
			TimeSpan lastGraphPin = TimeSpan.Zero;
			TimeSpan nextGraphPin = GraphPinInterval;
			Stopwatch freeTime = new Stopwatch();
			NetworkUdpFrame netframe;
			while( !IsDisposed ) {
				// ### check incoming network traffic
				WorkerProcessNetworkTraffic();

				// ### check triggers
				WorkerProcessTriggers();

				// ### prepare next frame with preset time target
				seq = mCurrentProgram;
				seq.Execute();
				netframe = new NetworkUdpFrame();
				lock( mDevices ) {
					foreach( var g in mDevices.AllSeenDevices() ) {
						netframe.AddRange( g.MarshalNetworkPackets() );
					}
				}

				if( TimeSpan.Zero == seq.CurrentTime ) {
					// ### special condition for first frame
					mNetwork.Send( netframe );
					progTime.Restart();
					seq.CurrentTime += mFrameTimeInterval;
					lastGraphPin = TimeSpan.Zero;
					nextGraphPin = GraphPinInterval;
				}
				else {
					// ### record some frame statistics..
					if( progTime.Elapsed >= nextGraphPin ) {
						// maintain graph pins...
						//Debug.WriteLine( "spanTime.ElapsedMilliseconds [{0}] freeTime.ElapsedMilliseconds [{1}]", spanTime.ElapsedMilliseconds, freeTime.ElapsedMilliseconds );
						//Debug.WriteLine( "seq.CurrentTime {0}, progTime.Elapsed {1}", seq.CurrentTime, progTime.Elapsed );
						double markPeriod = ( progTime.Elapsed - lastGraphPin ).TotalMilliseconds;
						double ratioUsed = 1.0 - ( (double)freeTime.ElapsedMilliseconds / markPeriod );
						lock( mWorkerFrameHistogram ) {
							mWorkerFrameHistogram.PushSample( ratioUsed );
						}
						lastGraphPin = progTime.Elapsed;
						nextGraphPin = lastGraphPin + GraphPinInterval;
						freeTime.Reset();
						UISafeCall( UIRebuildGraphs );
					}

					// ### spin until frame target
					freeTime.Start();
					while( seq.CurrentTime - progTime.Elapsed > LongSleepThreshold ) {
						Thread.Sleep( 1 );
					}
					while( seq.CurrentTime > progTime.Elapsed ) {
						Thread.Sleep( 0 );
					}
					freeTime.Stop();

					// ### send all assembled frame data
					mNetwork.Send( netframe );
				}

				// ### set next frame target
				seq.CurrentTime += mFrameTimeInterval;
				if( seq.CurrentTime < progTime.Elapsed ) {
					// @todo: can't keep up! need to log a warning?
					// match speed to max...
					seq.CurrentTime = progTime.Elapsed;
				}
			}
		}

		void XColourPickClick( object sender, EventArgs e ) {
			using( var dlg = new ColorDialog { Color = cColourSelected.BackColor } ) {
				if( DialogResult.OK == dlg.ShowDialog( this ) ) {
					cColourSelected.BackColor = dlg.Color;
				}
			}
		}

		void UIRebuildGraphs() {
			lock( mWorkerFrameHistogram ) {
				cFrameLatency.Image = mWorkerFrameHistogram.GenerateBitmap();
			}
			foreach( GlimPanel d in cDevices.Controls ) {
				d.UpdateStats();
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
			// program changed.. clear all buttons
			lock( mDevices ) {
				foreach( var g in mDevices.AllSeenDevices() ) {
					g.ClearButtonColour();
				}
				switch( mFunc ) {
					case OutputFunc.None:
						mCurrentProgram = new SequenceNull();
						break;
					case OutputFunc.Static:
						mCurrentProgram = new SequenceStatic( mDevices, () => cColourSelected.BackColor );
						break;
					case OutputFunc.ChannelTest:
						mCurrentProgram = new SequenceChannelTest( mDevices, clr => cColourSelected.BackColor = clr );
						break;
					case OutputFunc.Christmas:
						mCurrentProgram = new SequenceChristmas( mDevices );
						break;
					case OutputFunc.Rainbow:
						mCurrentProgram = new SequenceRainbow( mDevices, 94 / 3, 8 );
						break;
					case OutputFunc.WindowTest:
						mCurrentProgram = new SequenceTestWindow( mDevices.Find( "GlimSwarm-103" ) );
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
			}
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
			public string Hostname => null;
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
