namespace gui_harness {
	using ShadowCreatures.Glimmer;
    using System.Collections.Generic;
    using System.Windows.Forms;
	class ProgramStatic : Main.ProgramDefault {
		readonly IGlimPixelMap mMap;
		readonly Control mClr;

		public ProgramStatic( Control disp, IGlimPixelMap map ) {
			mClr = disp;
			mMap = map;
		}

		public override void Execute() {
			mMap.Write( InfiniteColor( mClr.BackColor ) );
		}
	}
}
