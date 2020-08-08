namespace ShadowCreatures.Glimmer {
	using System;

	class ExceptionMessage : Exception {
		public ExceptionMessage( string msg ) : base( msg ) {
		}
		public ExceptionMessage( string fmt, params object[] args ) : base( string.Format( fmt, args ) ) {
		}
	}
}
