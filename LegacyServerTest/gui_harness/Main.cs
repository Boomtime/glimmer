namespace ShadowCreatures.Glimmer {
	using System;
	using System.Windows.Forms;
	using System.Diagnostics;
	using System.Threading;
	using ShadowCreatures.Glimmer.Json;
	using System.Collections.Generic;
    using System.Drawing;
    using System.ComponentModel;

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
			Halloween2020,
			Christmas2020,
			Custom,
		}

		readonly Engine mEngine;
		readonly System.Windows.Forms.Timer mAutoHuntTimer = new System.Windows.Forms.Timer();
		OutputFunc mFunc = OutputFunc.None;
		Dictionary<OutputFunc, ISequence> mSequenceList = new Dictionary<OutputFunc, ISequence>();
		volatile ISequence mCurrentProgram = new SequenceNull();

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

			FormClosing += XMainFormClosing;
		}

		void AddFunctionRadio( string text, OutputFunc func, ISequence sequence = null ) {
			var r = new RadioButton {
				Text = text,
				Tag = func,
				Checked = ( 0 == cFuncFlow.Controls.Count ),
				AutoSize = true
			};
			r.CheckedChanged += XFuncCheckChanged;
			cFuncFlow.Controls.Add( r );
			mSequenceList.Add( func, sequence );
		}

		void XAutoHuntCheckedChanged( object sender, EventArgs e ) {
			if( ( sender as CheckBox ).Checked ) {
				mEngine.PingTriggerNextFrame();
				mAutoHuntTimer.Start();
			}
			else {
				mAutoHuntTimer.Stop();
			}
		}

		void XMainLoad( object sender, EventArgs e ) {
			mEngine.Devices.FindOrCreate( "GlimSwarm-101" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-102" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-103" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-104" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-105" );
			mEngine.Devices.FindOrCreate( "GlimSwarm-106" );

			AddFunctionRadio( "none", OutputFunc.None, new SequenceNull() );
			AddFunctionRadio( "static", OutputFunc.Static, new SequenceStatic() );
			AddFunctionRadio( "rainbow cycle", OutputFunc.Rainbow, new SequenceRainbow() );
			AddFunctionRadio( "channel test", OutputFunc.ChannelTest, new SequenceChannelTest() );
			//AddFunctionRadio( "party game", OutputFunc.PartyGame, new SequenceParty { EnableGame = true } );
			//AddFunctionRadio( "party no game", OutputFunc.PartyNoGame, new SequenceParty { EnableGame = false } );
			AddFunctionRadio( "christmas", OutputFunc.Christmas, new SequenceChristmas() );
			AddFunctionRadio( "window test", OutputFunc.WindowTest, new SequenceTestWindow() );
			AddFunctionRadio( "halloween 2020", OutputFunc.Halloween2020, new SequenceHalloween2020() );
			AddFunctionRadio( "christmas 2020", OutputFunc.Christmas2020, new SequenceChristmas2020() );
			AddFunctionRadio( "custom...", OutputFunc.Custom, new SequenceNull() );

			cBtnLoad.Click += XBtnLoad_Click;
			cBtnSave.Click += XBtnSave_Click;

			mEngine.Start();
		}

		void XMainFormClosing( object sender, FormClosingEventArgs e ) {
			mEngine.Stop();
		}

		void XBtnSave_Click( object sender, EventArgs e ) {
			// @todo
		}

		void XBtnLoad_Click( object sender, EventArgs e ) {
			var dlg = new OpenFileDialog();
			if( DialogResult.OK == dlg.ShowDialog() ) {
				try {
					LogAppend( "SequenceJson.Load: ", dlg.FileName );
					using( var file = dlg.OpenFile() ) {
						mSequenceList[OutputFunc.Custom] = SequenceJson.Load( file );
						if( OutputFunc.Custom == mFunc ) {
							StartCurrentSequence();
						}
					}
				}
				catch( Exception ex ) {
					var msg = string.Format( "Error occurred loading JSON. The message was:\r\n{0}", ex.Message );
					if( ex is JsonException ) {
						msg += string.Format( "\r\n\r\nAt:\r\n{0}", ( ex as JsonException ).JsonPath.FullPath );
					}
					LogAppend( Color.White, Color.DarkRed, "Error loading JSON: ", null, msg );
					MessageBox.Show( msg, "Error loading JSON", MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
			}
		}

		void XDeviceAdded( object sender, DeviceAddedEventArgs e ) {
			UISafeCall( () => {
				LogAppend( "XDeviceAdded: ", e.Device.HostName );
				var gp = new GlimPanel { Device = e.Device };
				gp.Click += ( s, e2 ) => SelectGlimPanel( s as GlimPanel );
				cDevices.Controls.Add( gp );
			} );
		}

		void SelectGlimPanel( GlimPanel gp ) {
			foreach( GlimPanel od in cDevices.Controls ) {
				od.SelectedAppearance = false;
			}
			if( cDetailsGrid is INotifyPropertyChanged ) {
				( cDetailsGrid as INotifyPropertyChanged ).PropertyChanged -= GlimDatGridPropertyChanged;
			}
			var p = gp.GetDataGridObject();
			cDetailsGrid.SelectedObject = p;
			p.PropertyChanged += GlimDatGridPropertyChanged;
			gp.SelectedAppearance = true;
		}

		void GlimDatGridPropertyChanged( object sender, PropertyChangedEventArgs e ) {
			UISafeCall( cDetailsGrid.Refresh );
		}

		void XHistogramChanged( object sender, EngineHistorgramUpdatedEventArgs e ) {
			UISafeCall( () => {
				cFrameLatency.Image = e.Bitmap;
				foreach( GlimPanel d in cDevices.Controls ) {
					d.UpdateStats();
				}
			} );
		}

		void UISafeCall( Action func ) {
			if( InvokeRequired ) {
				try {
					ThreadPool.QueueUserWorkItem( f => { try { Invoke( f as Action ); } catch { } }, func );
				}
				catch( Exception ex ) {
					Debug.WriteLine( "*** EXCEPTION in UISafeCall:" );
					Debug.WriteLine( ex.ToString() );
				}
			}
			else {
				func();
			}
		}

		void LogAppend( params object[] seq ) {
			ctlEventLog.AppendLine( seq );
		}

		void StartCurrentSequence() {
			mCurrentProgram = mSequenceList[mFunc];
			LogAppend( "StartCurrentSequence(): ", FontStyle.Bold, mFunc.ToString() );

			mEngine.LoadSequence( mCurrentProgram );
			// rebuild control panel
			ctlSequenceControlsPanel.SuspendLayout();
			ctlSequenceControlsPanel.Controls.Clear();
			foreach( var ctlv in mCurrentProgram.Controls ) {
				ctlSequenceControlsPanel.Controls.Add( ctlv.Value.MakeUIControl( ctlv.Key ) );
			}
			ctlSequenceControlsPanel.ResumeLayout();
		}

		void XFuncCheckChanged( object sender, EventArgs e ) {
			var rb = sender as RadioButton;
			if( rb.Checked ) {
				mFunc = (OutputFunc)rb.Tag;
				StartCurrentSequence();
			}
		}
	}
}
