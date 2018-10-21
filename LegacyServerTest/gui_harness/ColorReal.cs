namespace ShadowCreatures.Glimmer
{
	using System;
	using System.Drawing;

	/// <summary>convertible to/from Color, stores as HSL (+A) high-precision</summary>
	public class ColorReal
	{
		// these use Microsoft (Color) compatible ranges internally
		// except Hue which still uses 0-1 for ease of use, use Hue360 for Color compatible
		double m_hue = 0.0;
		double m_saturation = 1.0;
		double m_luminance = 1.0;
		double m_alpha = 1.0;

		/// <summary>Hue 0-360, Color.GetHue() compatible</summary>
		public double Hue360
		{
			get { return Hue * 360.0; }
			set { Hue = value / 360.0; }
		}

		/// <summary>Hue 0-1</summary>
		public double Hue
		{
			get { return m_hue; }
			set { m_hue = ClampToUnity( ( value + 1 ) - Math.Truncate( ( value + 1 ) ) ); }
		}

		/// <summary>Saturation 0-1, Color.GetSaturation() compatible</summary>
		public double Saturation
		{
			get { return m_saturation; }
			set { m_saturation = ClampToUnity( value ); }
		}

		/// <summary>Luminance 0-1, Color.GetBrightness() compatible</summary>
		public double Luminance
		{
			get { return m_luminance; }
			set { m_luminance = ClampToUnity( value ); }
		}

		/// <summary>Alpha 0-1, needs to be cast into Byte range for Color</summary>
		public double Alpha
		{
			get { return m_alpha; }
			set { m_alpha = ClampToUnity( value ); }
		}

		/// <summary>clamps values to 0-1</summary>
		double ClampToUnity( double p_value )
		{
			return Math.Min( Math.Max( p_value, 0.0 ), 1.0 );
		}

		/// <summary>converts the HSL interior colour to RGB values at high precision</summary>
		public void GetRGB( out double r, out double g, out double b )
		{
			if( 0 == Luminance )
			{
				r = g = b = 0.0;
				return;
			}

			if( 0 == Saturation )
			{
				r = g = b = Luminance;
				return;
			}

			var C = ( 1.0 - Math.Abs( 2.0 * Luminance - 1.0 ) ) * Saturation;
			var X = C * ( 1.0 - Math.Abs( ( Hue360 / 60.0 ) % 2.0 - 1.0 ) );
			var m = Luminance - C / 2.0;

			// spin the wheel! (6 segments of hue)
			switch( (int)( Hue * 6.0 ) )
			{
				case 0:
					r = C;
					g = X;
					b = 0;
					break;
				case 1:
					r = X;
					g = C;
					b = 0;
					break;
				case 2:
					r = 0;
					g = C;
					b = X;
					break;
				case 3:
					r = 0;
					g = X;
					b = C;
					break;
				case 4:
					r = X;
					g = 0;
					b = C;
					break;
				case 5:
					r = C;
					g = 0;
					b = X;
					break;
				default:
					// default to white for impossible Hue?
					r = g = b = 1.0;
					return;
			}

			r = ClampToUnity( r + m );
			g = ClampToUnity( g + m );
			b = ClampToUnity( b + m );
		}

		/// <summary>make two colors be equal</summary>
		public void CopyFrom( ColorReal rc ) {
			Hue = rc.Hue;
			Saturation = rc.Saturation;
			Luminance = rc.Luminance;
		}

		/// <summary>converts the HSL colour to RGB Color structure</summary>
		/// <returns>Microsoft Color</returns>
		public Color ToColor()
		{
			double r;
			double g;
			double b;

			GetRGB( out r, out g, out b );

			return Color.FromArgb( (int)( Alpha * byte.MaxValue ), (int)( r * 255 ), (int)( g * 255 ), (int)( b * 255 ) );
		}

		/// <summary>convert to Color on-the-fly</summary>
		public static implicit operator Color( ColorReal c )
		{
			return c.ToColor();
		}

		/// <summary>convert to ColorHSL on-the-fly</summary>
		public static implicit operator ColorReal( Color c )
		{
			return new ColorReal( c );
		}

		/// <summary>Color.White</summary>
		public ColorReal()
			: this( Color.White )
		{
		}

		/// <summary>init from Color</summary>
		public ColorReal( Color color )
			: this( color.GetHue(), color.GetSaturation(), color.GetBrightness() )
		{
		}
		
		/// <summary>direct initialize</summary>
		/// <param name="hue"></param>
		/// <param name="sat"></param>
		/// <param name="lum"></param>
		public ColorReal( double hue, double sat, double lum )
			: this( hue, sat, lum, 1.0 )
		{
		}

		/// <summary>direct initialize</summary>
		/// <param name="hue"></param>
		/// <param name="sat"></param>
		/// <param name="lum"></param>
		/// <param name="alpha"></param>
		public ColorReal( double hue, double sat, double lum, double alpha )
		{
			Hue360 = hue;
			Saturation = sat;
			Luminance = lum;
			Alpha = alpha;
		}

		/// <summary>printable</summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format( "H: {0:#0.##} S: {1:#0.##} L: {2:#0.##} A: {3:#0.##}", Hue360, Saturation, Luminance, Alpha );
		}

		/// <summary>printable</summary>
		/// <returns></returns>
		public string ToRGBString()
		{
			Color color = this;
			return string.Format( "R: {0:#0.##} G: {1:#0.##} B: {2:#0.##} A: {3:#0.##}", color.R, color.G, color.B, color.A );
		}

		/// <summary>give the RGB equivalent as 3 byte vector</summary>
		/// <returns></returns>
		public byte[] ToRGBArray()
		{
			Color color = this;
			return new byte[] { color.R, color.G, color.B };
		}

		/// <summary>give the RGB equivalent as 3 byte vector</summary>
		/// <returns></returns>
		public byte[] ToGBRArray() {
			Color color = this;
			return new byte[] { color.R, color.G, color.B };
		}
	}
}
