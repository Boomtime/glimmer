namespace ThreadPrecisionTest {
	using System;
    using System.Diagnostics;
    using System.Threading;

    class Program {
		static void Main( string[] args ) {
			Console.WriteLine( "(any key to stop)" );
#if USE_DATETIME
			DateTime start = DateTime.Now;
			DateTime target = start;
			while( !Console.KeyAvailable ) {
				target = target.AddMilliseconds( 25 ); // new target time
				while( target - DateTime.Now > TimeSpan.FromMilliseconds( 20 ) ) {
					Thread.Sleep( 1 );
					Debug.WriteLine( "sleep long time" );
				}
				while( DateTime.Now < target ) {
					Thread.Sleep( 0 );
				}
				Console.WriteLine( string.Format( "{0:G} (error: {1} ms", DateTime.Now - start, ( DateTime.Now - target ).TotalMilliseconds ) );
			}
#else
			Stopwatch sw = new Stopwatch();
			TimeSpan target = TimeSpan.Zero;
			sw.Start();
			while( !Console.KeyAvailable ) {
				target = target.Add( TimeSpan.FromMilliseconds( 25 ) );
				while( target - sw.Elapsed > TimeSpan.FromMilliseconds( 20 ) ) {
					Thread.Sleep( 1 );
					Debug.WriteLine( "sleep long time" );
				}
				while( target > sw.Elapsed ) {
					Thread.Sleep( 0 );
				}
				Console.WriteLine( string.Format( "{0:G} (error: {1} ms", sw.Elapsed, ( sw.Elapsed - target ).TotalMilliseconds ) );
			}
#endif
		}
	}
}
