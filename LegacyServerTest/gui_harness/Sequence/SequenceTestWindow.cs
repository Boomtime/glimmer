namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
    using System;
	using System.Collections.Generic;
	using System.Drawing;

    class SequenceTestWindow : SequenceMinimum {
		static readonly TimeSpan CometTimeMax = TimeSpan.FromSeconds( 5 );

		readonly FxScale mFxPerimeter = new FxScale(
			new FxRainbow { HueCyclePixelLength = 94 / 3, HueSecondsPerCycle = 8 }
			//new FxCandyCane()
		);
		readonly WindowDevice mWindow;
		readonly IGlimPixelMap mMapPerimeter;
		readonly IGlimPixelMap mEdgeLeft;
		readonly IGlimPixelMap mEdgeRight;
		FxComet mFxCometLeft = null;
		FxComet mFxCometRight = null;

		class WindowDevice : SequenceDeviceBasic {
			public TimeSpan CometTimeStart = TimeSpan.FromSeconds( -10 );

			public WindowDevice() : base( "Window", "GlimSwarm-103", 94 ) {
			}

			public override void OnButtonStateChanged( ButtonStatus btn ) {
				if( ButtonStatus.Up == btn ) {
					CometTimeStart = TimeSpan.Zero;
				}
			}
		}

		public SequenceTestWindow() {
			mWindow = new WindowDevice();
			mMapPerimeter = new GlimPixelMap.Factory { { mWindow, 0, 94 } }.Compile();
			mEdgeLeft = new GlimPixelMap.Factory { { mWindow, 55, -48 } }.Compile();
			mEdgeRight = new GlimPixelMap.Factory { { mWindow, 54, 40 }, { mWindow, 0, 8 } }.Compile();
			mFxCometLeft = new FxComet { 
				PixelCount = mEdgeLeft.PixelCount + 10,
				BaseColor = Color.White,
				TailPixelLength = 10,
				SpeedPixelsPerSecond = 40
			};
			mFxCometRight = new FxComet {
				PixelCount = mEdgeRight.PixelCount + 10,
				BaseColor = Color.White,
				TailPixelLength = 10,
				SpeedPixelsPerSecond = 40
			};
			AddLuminanceControl( v => mFxPerimeter.Luminance = v );
			AddSaturationControl( v => mFxPerimeter.Saturation = v );
		}

		public override IEnumerable<IDeviceBinding> Devices {
			get { yield return mWindow; }
		}

		public override void SetCurrentTime( TimeSpan elapsed ) {
			base.SetCurrentTime( elapsed );
			if( TimeSpan.Zero == elapsed ) {
				mWindow.ButtonColour = new ButtonColour( Color.Black, new ColorReal( Color.White ) { Luminance = 0.2 }, 1000, Color.White );
			}
		}

		public override void FrameExecute() {
			var ctx = MakeCurrentContext();
			mMapPerimeter.Write( mFxPerimeter.Execute( ctx ) );
			if( TimeSpan.Zero == mWindow.CometTimeStart ) {
				// comets reset
				mWindow.CometTimeStart = CurrentTime;
			}
			var comettime = CurrentTime - mWindow.CometTimeStart;
			if( comettime < CometTimeMax ) {
				// comets animating
				var cctx = new FxContextSimple( comettime );
				mEdgeLeft.Write( mFxCometLeft.Execute( cctx ) );
				mEdgeRight.Write( mFxCometRight.Execute( cctx ) );
			}
		}
	}
}
