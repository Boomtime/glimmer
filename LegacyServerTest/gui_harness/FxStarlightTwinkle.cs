namespace ShadowCreatures.Glimmer {
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Security.Cryptography;

	class FxStarlightTwinkle : FxBase {
		readonly double[] SinePeriods = { 5.9, 7.1, 12.7 }; // primes to maximise time to repeat
		readonly uint SeedStripe = 0x7f123abc;
		HashAlgorithm mSeedGen = MD5.Create();

		public FxStarlightTwinkle( GlimPixelMap map ) : base( map ) {
			BaseColor = Color.White;
			LuminanceMinima = 0.00;
			LuminanceMaxima = 0.40;
			SpeedFactor = 1.0;
		}

		public ColorReal BaseColor { get; set; }
		public double LuminanceMinima { get; set; }
		public double LuminanceMaxima { get; set; }
		public double SpeedFactor { get; set; }

		/// <summary>a sort-of hash that is 0-1</summary>
		double GetPixelHash( int pixel ) {
			var res = mSeedGen.ComputeHash( BitConverter.GetBytes( SeedStripe ^ (uint)pixel ) );
			return (double)BitConverter.ToUInt32( res, 0 ) / (double)UInt32.MaxValue;
		}

		public override void Execute( IFxContext ctx ) {
			double currentSeed;
			double lum;
			int pixelCount = 0;

			foreach( var p in PixelMap ) {
				currentSeed = ctx.TimeNow.TotalSeconds + ( GetPixelHash( pixelCount ) * 13 );
				pixelCount++;
				lum = 0;
				foreach( var period in SinePeriods ) {
					double c = period / SpeedFactor;
					lum += Math.Sin( 2 * Math.PI * ( currentSeed % c ) / c );
				}
				// lum is now -1 to 1, changing.. slowly?
				lum = ( lum + 1.0 ) / 2.0;
				lum *= ( LuminanceMaxima - LuminanceMinima );
				lum += LuminanceMinima;

				p.CopyFrom( BaseColor );
				p.Luminance = lum;
			}
		}
	}
}
