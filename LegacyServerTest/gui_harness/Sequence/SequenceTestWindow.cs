﻿namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
    using System.Drawing;

    class SequenceTestWindow : SequenceDefault {
		readonly FxScale mFxPerimeter = new FxScale(
			new FxRainbow { HueCyclePixelLength = 94 / 3, HueSecondsPerCycle = 8 }
			//new FxCandyCane()
		);
		readonly IGlimPixelMap mMapPerimeter;
		readonly IGlimPixelMap mEdgeLeft;
		readonly IGlimPixelMap mEdgeRight;
		IFx mFxCometLeft = null;
		IFx mFxCometRight = null;

		public SequenceTestWindow( GlimDevice window ) {
			mMapPerimeter = new GlimPixelMap.Factory { { window, 0, 94 } }.Compile();
			mEdgeLeft = new GlimPixelMap.Factory { { window, 55, -48 } }.Compile();
			mEdgeRight = new GlimPixelMap.Factory { { window, 54, 40 }, { window, 0, 8 } }.Compile();
			window.SetButtonColour( Color.Black, new ColorReal( Color.White ) { Luminance = 0.2 }, 1000, Color.White );
		}

		public override double Luminance {
			set { mFxPerimeter.Luminance = value; }
		}

		public override double Saturation {
			set { mFxPerimeter.Saturation = value; }
		}

		public override void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
			if( ButtonStatus.Up == btn ) {
				mFxCometLeft = new FxComet( mEdgeLeft.PixelCount + 10 ) { BaseColor = Color.White, TailPixelLength = 10, SpeedPixelsPerSecond = 40 };
				mFxCometRight = new FxComet( mEdgeRight.PixelCount + 10 ) { BaseColor = Color.White, TailPixelLength = 10, SpeedPixelsPerSecond = 40 };
			}
		}

		public override void Execute() {
			var ctx = MakeCurrentContext();
			mMapPerimeter.Write( mFxPerimeter.Execute( ctx ) );
			if( null != mFxCometLeft ) {
				mEdgeLeft.Write( mFxCometLeft.Execute( ctx ) );
				if( !mFxCometLeft.IsRunning ) {
					mFxCometLeft = null;
				}
			}
			if( null != mFxCometRight ) {
				mEdgeRight.Write( mFxCometRight.Execute( ctx ) );
				if( !mFxCometRight.IsRunning ) {
					mFxCometRight = null;
				}
			}
		}
	}
}