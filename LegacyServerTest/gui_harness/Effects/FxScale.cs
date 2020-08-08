namespace ShadowCreatures.Glimmer.Effects {
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	enum FxScaleOperator {
		Multiply,
		Add,
		Ceiling,
		Floor,
		Subtract
	}

	class FxScale : IFxPipe {

		IFx mSrc;

		public FxScale() {
			mSrc = new FxSolid { Colour = Color.White };
		}

		public FxScale( IFx src ) : this() {
			mSrc = src;
		}

		[ConfigurableRatio]
		public double Luminance { get; set; } = 1.0;

		[ConfigurableRatio]
		public double Saturation { get; set; } = 1.0;

		[Configurable]
		public FxScaleOperator Operator { get; set; } = FxScaleOperator.Multiply;

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

		IEnumerable<Color> MethodSubtract( IFxContext ctx ) {
			foreach( ColorReal pix in mSrc.Execute( ctx ) ) {
				pix.Luminance -= Luminance;
				pix.Saturation -= Saturation;
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

		IEnumerable<Color> Empty() {
			yield break;
		}

		public IEnumerable<Color> Execute( IFxContext ctx ) {
			if( null == mSrc ) {
				return Empty();
			}
			switch( Operator ) {
				case FxScaleOperator.Add:
					return MethodAdd( ctx );
				case FxScaleOperator.Ceiling:
					return MethodCeiling( ctx );
				case FxScaleOperator.Floor:
					return MethodFloor( ctx );
				case FxScaleOperator.Subtract:
					return MethodSubtract( ctx );
			}
			return MethodMultiply( ctx );
		}

		public void AddSource( IFx src ) {
			mSrc = src;
		}
	}
}
