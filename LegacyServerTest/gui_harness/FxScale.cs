using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ShadowCreatures.Glimmer {

	class FxScale : IFx {

		readonly IFx mSrc;

		public FxScale( IFx src ) {
			mSrc = src;
			Luminance = 1.0;
			Saturation = 1.0;
			Method = Function.Multiply;
		}

		public enum Function {
			Multiply,
			Add,
			Ceiling,
			Floor
		}

		[ConfigurableDouble( Maximum = 1.0, Minimum = 0.0 )]
		public double Luminance { get; set; }

		[ConfigurableDouble( Maximum = 1.0, Minimum = 0.0 )]
		public double Saturation { get; set; }

		[Configurable]
		public Function Method { get; set; }

		public bool IsRunning => mSrc.IsRunning;

		IEnumerable<Color> MethodMultiply( IFxContext ctx ) {
			foreach( ColorReal pix in mSrc.Execute( ctx ) ) {
				pix.Luminance *= Luminance;
				pix.Saturation *= Saturation;
				yield return pix;
			}
		}

		IEnumerable<Color> MethodAdd( IFxContext ctx ) {
			foreach( ColorReal pix in mSrc.Execute( ctx ) ) {
				pix.Luminance += Luminance;
				pix.Saturation += Saturation;
				yield return pix;
			}
		}

		IEnumerable<Color> MethodCeiling( IFxContext ctx ) {
			foreach( ColorReal pix in mSrc.Execute( ctx ) ) {
				pix.Luminance = Math.Min( pix.Luminance, Luminance );
				pix.Saturation = Math.Min( pix.Saturation, Saturation );
				yield return pix;
			}
		}

		IEnumerable<Color> MethodFloor( IFxContext ctx ) {
			foreach( ColorReal pix in mSrc.Execute( ctx ) ) {
				pix.Luminance = Math.Max( pix.Luminance, Luminance );
				pix.Saturation = Math.Max( pix.Saturation, Saturation );
				yield return pix;
			}
		}

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			switch( Method ) {
				case Function.Add:
					return MethodAdd( ctx );
				case Function.Ceiling:
					return MethodCeiling( ctx );
				case Function.Floor:
					return MethodFloor( ctx );
			}
			return MethodMultiply( ctx );
		}

		//public void Initialize( int pixelCount ) {
		//	mSrc.Initialize( pixelCount );
		//}
	}
}
