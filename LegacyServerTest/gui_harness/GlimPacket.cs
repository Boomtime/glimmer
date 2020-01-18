namespace ShadowCreatures.Glimmer {
    using System;
    using System.Collections.Generic;
	using System.Drawing;

	/// <summary>basic implementation of a colour vector packet</summary>
	class GlimPacket : IGlimPacket {
		readonly IGlimDevice mDevice;
		ColorReal[] mData;

		public GlimPacket( IGlimDevice device ) {
			mDevice = device;
			mData = new ColorReal[device.PixelCount];
		}

		public IGlimDevice Device {
			get { return mDevice; }
		}

		/// <summary>uses src-alpha to blend dst with 1 minus src-alpha</summary>
		/// <param name="dst">destination (alpha is ignored)</param>
		/// <param name="src">source colour with alpha</param>
		/// <returns>blend</returns>
		static Color Blend( Color dst, Color src ) {
			if( Byte.MaxValue == src.A ) {
				return src;
			}
			if( 0 == src.A ) {
				return dst;
			}
			double sa = (double)src.A / Byte.MaxValue;
			double omsa = 1.0 - sa;

			return Color.FromArgb( (int)( dst.R * omsa + src.R * sa ),
				(int)( dst.G * omsa + src.G * sa ), (int)( dst.B * omsa + src.B * sa ) );
		}

		/// <summary>set pixel preserving blend using src-alpha</summary>
		/// <param name="pixel"></param>
		/// <param name="src"></param>
		public void SetPixel( int pixel, Color src ) {
			mData[pixel] = Blend( this[pixel], src );
		}

		public IEnumerable<Color> Read() {
			foreach( var c in mData ) {
				yield return null != c ? c.ToColor() : Color.Black;
			}
		}

		public Color this[int pixel] {
			get {
				if( null == mData[pixel] )
					mData[pixel] = new ColorReal( Color.Black );
				return mData[pixel];
			}
		}
	}
}
