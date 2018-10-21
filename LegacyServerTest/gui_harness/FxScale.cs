namespace ShadowCreatures.Glimmer {

	class FxScale : FxBase {

		public FxScale( IGlimPixelMap map ) : base( map ) {
		}

		public double LuminanceScale { get; set; }
		public double SaturationScale { get; set; }

		public override void Execute( IFxContext ctx ) {
			foreach( var pix in PixelMap ) {
				pix.Luminance *= LuminanceScale;
				pix.Saturation *= SaturationScale;
			}
		}
	}
}
