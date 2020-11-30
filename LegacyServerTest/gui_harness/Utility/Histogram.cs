namespace ShadowCreatures.Glimmer.Utility {
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using System.Drawing;

    class HistogramData<T> : IEnumerable<T> {
		readonly T[] data;
		int index;
		int count;

		public HistogramData( int size ) {
			data = new T[size];
			index = 0;
			count = 0;
		}

		public void Push( T v ) {
			data[index] = v;
			if( count < data.Length ) {
				count++;
			}
			index++;
			if( index >= data.Length ) {
				index = 0;
			}
		}

		public IEnumerator<T> GetEnumerator() {
			if( 0 == count ) {
				yield break;
			}
			int j = index - 1;
			while( j >= 0 ) {
				yield return data[j];
				j--;
			}
			j = count - 1;
			while( j >= index ) {
				yield return data[j];
				j--;	
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	class HistogramGraphics : IDisposable {
		int mHeight = 10;
		Bitmap mFrame = null;
		Color mColourCanvas = Color.Transparent;
		Color mColourFill = Color.LightGreen;
		Color mColourLine = Color.DarkGreen;

		public void Dispose() {
			DisposeFrame();
		}

		public int Height {
			get { return mHeight; }
			set { mHeight = value; DisposeFrame(); }
		}

		public Color ColourCanvas {
			get { return mColourCanvas; }
			set { mColourCanvas = value; DisposeFrame(); }
		}

		public Color ColourFill {
			get { return mColourFill; }
			set { mColourFill = value; DisposeFrame(); }
		}

		public Color ColourLine {
			get { return mColourLine; }
			set { mColourLine = value; DisposeFrame(); }
		}

		void DisposeFrame() {
			if( null != mFrame ) {
				mFrame.Dispose();
				mFrame = null;
			}
		}

		void AssertFrame() {
			if( null == mFrame ) {
				mFrame = new Bitmap( 1, mHeight * 2 - 1 );
				using( var gfx = Graphics.FromImage( mFrame ) ) {
					gfx.Clear( ColourCanvas );
					using( Pen line = new Pen( mColourFill, 1 ) ) {
						gfx.DrawLine( line, 0, mHeight, 0, mHeight * 2 - 1 );
					}
				}
				mFrame.SetPixel( 0, mHeight - 1, mColourLine );
			}
		}

		public void DrawLine( Graphics gfx, int x_pos, int y_mark ) {
			AssertFrame();
			gfx.DrawImage( mFrame, x_pos, 0, new Rectangle { X = 0, Y = y_mark, Width = 1, Height = mHeight }, GraphicsUnit.Pixel );
		}
	}

	class Histogram {
		[Flags]
		public enum SampleStyle : int {
			Regular = 0,
			Break = ( 1 << 0 ),
			NoSignal = ( 1 << 1 ),
			AlternateColours = NoSignal
		}

		struct Sample {
			public double Value;
			public SampleStyle Style;
		}

		readonly HistogramData<Sample> data;
		readonly Size size;
		Bitmap prev;
		int newSampleCount;

		public Histogram( Size sz ) {
			size = sz;
			data = new HistogramData<Sample>( sz.Width );
			GraphicsRegular = new HistogramGraphics { Height = sz.Height };
			GraphicsAlternate = new HistogramGraphics { Height = sz.Height, ColourFill = Color.LightGray, ColourLine = Color.Gray };
			prev = new Bitmap( size.Width, size.Height );
			using( var gfx = Graphics.FromImage( prev ) ) {
				gfx.Clear( GraphicsRegular.ColourCanvas );
			}
			newSampleCount = 0;
		}

		public HistogramGraphics GraphicsRegular { get; set; }

		public HistogramGraphics GraphicsAlternate { get; set; }

		public Color ColourWarning { get; set; } = Color.Red;

		public void PushSample( double v, SampleStyle style = SampleStyle.Regular ) {
			data.Push( new Sample { Value = v, Style = style } );
			newSampleCount++;
		}

		public Bitmap GenerateBitmap() {
			if( 0 == newSampleCount ) {
				return prev;
			}
			HistogramGraphics hgfx;
			int x = size.Width - 1;
			double pt;
			var bmp = new Bitmap( size.Width, size.Height );
			using( var gfx = Graphics.FromImage( bmp ) ) {
				gfx.DrawImage( prev, 0, 0, new Rectangle { X = newSampleCount, Y = 0, Width = size.Width - newSampleCount, Height = size.Height }, GraphicsUnit.Pixel );
				foreach( var pin in data ) {
					pt = Math.Max( 0, pin.Value );
					hgfx = pin.Style.HasFlag( SampleStyle.AlternateColours ) ? GraphicsAlternate : GraphicsRegular;
					hgfx.DrawLine( gfx, x, (int)( pt * (double)size.Height ) );
					if( pin.Style.HasFlag( SampleStyle.Break ) ) {
						using( var pen = new Pen( ColourWarning, 1 ) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot } ) {
							gfx.DrawLine( pen, x, 0, x, size.Height - 1 );
						}
					}
					if( 0 == x ) {
						break;
					}
					newSampleCount--;
					if( 0 == newSampleCount ) {
						break;
					}
					x--;
				}
			}
			prev = bmp;
			return bmp;
		}
	}
}
