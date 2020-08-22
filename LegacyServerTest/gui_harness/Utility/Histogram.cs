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

	class Histogram {
		readonly HistogramData<double> data;
		readonly Size size;
		Bitmap prev;
		int newSampleCount;
		readonly Color colourCanvas = Color.Transparent;
		readonly Color colourFill = Color.LightGreen;
		readonly Color colourLine = Color.DarkGreen;
		readonly Color colourWarning = Color.Red;

		public Histogram( Size sz ) {
			size = sz;
			data = new HistogramData<double>( sz.Width );
			prev = new Bitmap( size.Width, size.Height );
			using( var gfx = Graphics.FromImage( prev ) ) {
				gfx.Clear( colourCanvas );
			}
			newSampleCount = 0;
		}

		public void PushSample( double v ) {
			data.Push( v );
			newSampleCount++;
		}

		public Bitmap GenerateBitmap() {
			if( 0 == newSampleCount ) {
				return prev;
			}
			int x = size.Width - 1;
			int y;
			double pt;
			var bmp = new Bitmap( size.Width, size.Height );
			using( var gfx = Graphics.FromImage( bmp ) ) {
				gfx.DrawImage( prev, 0, 0, new Rectangle { X = newSampleCount, Y = 0, Width = size.Width - newSampleCount, Height = size.Height }, GraphicsUnit.Pixel );
				using( var fill = new SolidBrush( colourFill ) ) {
					using( var canvas = new SolidBrush( colourCanvas ) ) {
						foreach( var pinv in data ) {
							pt = pinv;
							if( pt < 0 ) {
								pt = 0 - pt;
							}
							y = size.Height - (int)( pt * (double)size.Height );
							gfx.FillRectangle( canvas, x, 0, 1, y );
							gfx.FillRectangle( fill, x, y, 1, size.Height - y );
							bmp.SetPixel( x, Math.Min( y, size.Height - 1 ), colourLine );
							if( pinv < 0 ) {
								using( var pen = new Pen( colourWarning, 1 ) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot } ) {
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
				}
			}
			prev = bmp;
			return bmp;
		}
	}
}
