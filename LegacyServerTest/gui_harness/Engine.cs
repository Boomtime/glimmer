namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Utility;
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Threading;
	using System.Drawing;

	class EngineHistorgramUpdatedEventArgs : EventArgs {
		public EngineHistorgramUpdatedEventArgs( Bitmap bmp ) {
			Bitmap = bmp;
		}
		public Bitmap Bitmap { get; }
	}

	class Engine : IDisposable {
		static readonly TimeSpan GraphPinInterval = TimeSpan.FromMilliseconds( 1000 );
		static readonly TimeSpan LongSleepThreshold = TimeSpan.FromMilliseconds( 15 );

		readonly Thread mWorkerThread;
		bool mPingTriggerNextFrame = false;
		volatile ISequence mCurrentProgram = new SequenceNull();
		Histogram mWorkerFrameHistogram = null;
		bool isRunning = true;

		public Engine() {
			Devices = new GlimManager();
			Network = new NetworkServer();
			FrameTimeInterval = TimeSpan.FromMilliseconds( 40 );
			mWorkerThread = new Thread( WorkerThreadMain );
			mWorkerThread.Start();
		}

		public void Start() {
			if( !isRunning ) {
				isRunning = true;
				mWorkerThread.Start();
			}
		}

		public void Stop() {
			if( isRunning ) {
				isRunning = false;
				mWorkerThread.Join();
			}
		}

		public GlimManager Devices { get; }

		NetworkServer Network { get; }

		public TimeSpan FrameTimeInterval { get; set; }

		public Size HistogramSize {
			set { mWorkerFrameHistogram = new Histogram( value ); }
		}

		public event EventHandler<EngineHistorgramUpdatedEventArgs> HistogramChanged;

		public ISequence Sequence {
			get {
				return mCurrentProgram;
			}
			set {
				// mCurrentProgram is volatile
				mCurrentProgram = value;
				Devices.ForEachSeenDevice( g => g.ClearButtonColour() );
			}
		}

		public void PingTriggerNextFrame() {
			mPingTriggerNextFrame = true;
		}

		void WorkerProcessTriggers() {
			if( mPingTriggerNextFrame ) {
				mPingTriggerNextFrame = false;
				Network.Send( new IPEndPoint( IPAddress.Broadcast, NetworkServer.DefaultPort ), new NetworkMessagePing() );
			}
		}

		void WorkerProcessButtonStatus( NetworkButtonStatusEventArgs e ) {
			var g = Devices.Find( e.SourceAddress );
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
					var g = Devices.FindOrCreate( e.Hostname );
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
			while( null != ( msg = Network.Receive() ) ) {
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
			while( isRunning ) {
				// ### check incoming network traffic
				WorkerProcessNetworkTraffic();

				// ### check triggers
				WorkerProcessTriggers();

				// ### prepare next frame with preset time target
				seq = mCurrentProgram;
				seq.Execute();
				netframe = new NetworkUdpFrame();
				Devices.ForEachSeenDevice( g => netframe.AddRange( g.MarshalNetworkPackets() ) );

				if( TimeSpan.Zero == seq.CurrentTime ) {
					// ### special condition for first frame
					Network.Send( netframe );
					progTime.Restart();
					seq.CurrentTime += FrameTimeInterval;
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
							mWorkerFrameHistogram.PushSample( Math.Max( 0, ratioUsed ) );
						}
						lastGraphPin = progTime.Elapsed;
						nextGraphPin = lastGraphPin + GraphPinInterval;
						freeTime.Reset();
						HistogramChanged?.Invoke( this, new EngineHistorgramUpdatedEventArgs( mWorkerFrameHistogram.GenerateBitmap() ) );
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
					Network.Send( netframe );
				}

				// ### set next frame target
				seq.CurrentTime += FrameTimeInterval;
				if( seq.CurrentTime < progTime.Elapsed ) {
					// @todo: can't keep up! need to log a warning?
					// match speed to max...
					seq.CurrentTime = progTime.Elapsed;
				}
			}
		}

		#region IDisposable Support
		protected virtual void Dispose( bool disposing ) {
			Stop();
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose() {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose( true );
		}
		#endregion
	}
}
