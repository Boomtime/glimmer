namespace ShadowCreatures.Glimmer {
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	class FxChristmasTwinkle : FxBase {

		public FxChristmasTwinkle( IGlimPixelMap map ) : base( map ) {
		}

		public double CyclesPerSecond = 0.4;
		public double StridePerPixel = 0.15;

		public Color BaseColour = Color.FromArgb( 0xff, 0xee, 0xdd );
		public double LumLow = 0.1;
		public double LumHigh = 0.52;
		public double Saturation = 0.55;

		/// <summary>write current frame data to pixel map</summary>
		public override void Execute( IFxContext ctx ) {

			var secondsPerCycle = ( 1.0 / CyclesPerSecond );
			var posBase = ( ctx.TimeNow.TotalSeconds % secondsPerCycle ) / secondsPerCycle * 2;
			var posShift = false;

			foreach( var pix in PixelMap ) {
				ColorReal clr = BaseColour;
				clr.Saturation = Saturation;
				var pos = posBase;
				if( posShift ) {
					pos = ( posBase + 1 ) % 2;
				}
				posShift = !posShift;
				if( pos > 1 ) {
					clr.Luminance = LumLow + ( ( 2 - pos ) * ( LumHigh - LumLow ) );
				}
				else {
					clr.Luminance = LumLow + ( pos * ( LumHigh - LumLow ) );
				}
				pix.CopyFrom( clr );

				posBase += StridePerPixel;
				if( posBase > 2 ) {
					posBase = posBase - 2;
				}
			}
		}
	}
}
