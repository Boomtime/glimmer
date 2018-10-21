namespace ShadowCreatures.Glimmer {
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

		public ColorReal this[int pixel] {
			get {
				if( null == mData[pixel] )
					mData[pixel] = new ColorReal( Color.Black );
				return mData[pixel];
			}
			set { mData[pixel] = new ColorReal( value ); }
		}

		public IEnumerator<ColorReal> GetEnumerator() {
			foreach( var c in mData ) {
				yield return null != c ? c : new ColorReal( Color.Black );
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return mData.GetEnumerator();
		}
	}
}
