namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
	using System;
    using System.Collections.Generic;
    using System.Drawing;

    class SequenceParty : SequenceDefault {
		const double ButtonTimeEpsilonSeconds = 0.2;

		/// <summary>internal descriptor of a device</summary>
		class GlimDescriptor {
			readonly IGlimPixelMap mPixelMap;
			FxComet mFxComet;

			public GlimDescriptor( GlimDevice device, Color partyColor ) {
				Device = device;
				PartyColor = partyColor;
				mFxComet = null;
				mPixelMap = new GlimPixelMap.Factory { { device, 0, 100 } }.Compile();
			}

			public void WriteExecute( IFxContext ctx ) {
				if( null != mFxComet && mFxComet.IsRunning ) {
					mPixelMap.Write( mFxComet.Execute( ctx ) );
				}
			}

			public bool CometIsRunning {
				get { return mFxComet == null ? false : mFxComet.IsRunning; }
			}

			public void StartComet( int pixelCount ) {
				mFxComet = new FxComet { BaseColor = PartyColor, PixelCount = pixelCount };
			}

			public readonly GlimDevice Device;
			public readonly Color PartyColor;
			public TimeSpan? ButtonDownTimestamp { get; set; }
		}

		enum GameState {
			Null,
			SynchronizedShotsFired,
			BarrelShotFired,
			CoolDown,
		}

		readonly GlimDescriptor mGlimRedGun;
		readonly GlimDescriptor mGlimBlueGun;
		readonly IGlimPixelMap mPixelMapStars;
		readonly IGlimPixelMap mPixelMapBarrel;
		readonly IGlimPixelMap mPixelMapPerimeter;
		readonly FxScale mFxPerimeterRainbow;
		readonly FxScale mFxStarlight;
		readonly IFx mFxCannonTwinkle;
		FxComet mFxBarrel;
		TimeSpan? mButtonUpSynchronizeTimestamp;
		GameState mGameState = GameState.Null;
		TimeSpan mGameCoolDownStart;

		public SequenceParty( GlimManager mgr ) {
			var deviceRed = mgr.Find( "GlimSwarm-103" );
			var deviceBlue = mgr.Find( "GlimSwarm-104" );
			var deviceStars = mgr.Find( "GlimSwarm-102" );

			// comets!
			mGlimRedGun = new GlimDescriptor( deviceRed, Color.Red );
			mGlimBlueGun = new GlimDescriptor( deviceBlue, Color.Blue );
			mFxBarrel = new FxComet { BaseColor = Color.FromArgb( 0xff, 0, 0xff ), PixelCount = 50 };
			mFxCannonTwinkle = new FxStarlightTwinkle {
				BaseColor = Color.FromArgb( 0xff, 0, 0xff ),
				SpeedFactor = 15.0,
				LuminanceMinima = 0.2,
				LuminanceMaxima = 0.8
			};

			mPixelMapStars = new GlimPixelMap.Factory { deviceStars }.Compile();
			mPixelMapBarrel = new GlimPixelMap.Factory { { deviceRed, 100, 50 } }.Compile();
			mPixelMapPerimeter = new GlimPixelMap.Factory { { deviceRed, 0, 100 }, { deviceBlue, 100, -100 } }.Compile();

			mFxPerimeterRainbow = new FxScale( new FxRainbow() );
			mFxStarlight = new FxScale( new FxStarlightTwinkle { BaseColor = Color.Yellow } ) { Saturation = 0.3 };
		}

		IEnumerable<Color> BarrelColour() {
			while( true ) {
				yield return Color.Black;
			}
		}

		public override void Execute() {
			var ctx = MakeCurrentContext();
			mFxPerimeterRainbow.Luminance = PerimeterLuminanceMultiplier;
			mFxPerimeterRainbow.Saturation = Saturation;
			mPixelMapPerimeter.Write( mFxPerimeterRainbow.Execute( ctx ) );
			mFxStarlight.Luminance = LuminanceStarlightMultiplier;
			mPixelMapStars.Write( mFxStarlight.Execute( ctx ) );

			mGlimRedGun.WriteExecute( ctx );
			mGlimBlueGun.WriteExecute( ctx );
			if( null != mFxBarrel && mFxBarrel.IsRunning ) {
				mPixelMapBarrel.Write( mFxBarrel.Execute( ctx ) );
			}
			switch( mGameState ) {
				case GameState.Null:
				case GameState.SynchronizedShotsFired:
					return;
			}
			double lumscale = 0.0;
			if( GameState.CoolDown == mGameState ) {
				mPixelMapBarrel.Write( BarrelColour() );
				mPixelMapStars.Write( mFxCannonTwinkle.Execute( ctx ) );
				// take 2 seconds to return to full glow
				lumscale = ( ( CurrentTime - mGameCoolDownStart ).TotalSeconds - 1 ) / 2;
			}
			if( lumscale < 1 ) {
				// barrel shot or cooling down
			}
			else {
				mGameState = GameState.Null;
				RecalculateButtonGlimmer( mGlimBlueGun );
				RecalculateButtonGlimmer( mGlimRedGun );
			}
			switch( mGameState ) {
				case GameState.BarrelShotFired:
					if( !mFxBarrel.IsRunning ) {
						mGameState = GameState.CoolDown;
						mGameCoolDownStart = CurrentTime;
						mFxBarrel = null;
					}
					break;
				case GameState.SynchronizedShotsFired:
					if( !mGlimBlueGun.CometIsRunning || !mGlimRedGun.CometIsRunning ) {
						mGameState = GameState.BarrelShotFired;
						mFxBarrel = new FxComet { BaseColor = Color.FromArgb( 0xff, 0, 0xff ), PixelCount = 50 };
					}
					break;
			}
		}

		public override void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
			GlimDescriptor d;
			if( src.Hostname == mGlimRedGun.Device.Hostname ) {
				d = mGlimRedGun;
			}
			else if( src.Hostname == mGlimBlueGun.Device.Hostname ) {
				d = mGlimBlueGun;
			}
			else {
				if( null == src.Hostname ) {
					DebugClick();
				}
				// only responding to known descriptors
				return;
			}
			if( ButtonStatus.Up == btn ) {
				if( EnableGame && d.ButtonDownTimestamp.HasValue ) {
					int tlimit = Math.Min( 100, (int)( ( CurrentTime - d.ButtonDownTimestamp.Value ).TotalSeconds * 20 ) );
					d.StartComet( tlimit );
					RecalculateButtonGlimmer( d );
					if( mButtonUpSynchronizeTimestamp.HasValue ) {
						// compare current time to Up sync time to see if it's game on
						double diffsec = ( CurrentTime - mButtonUpSynchronizeTimestamp.Value ).TotalSeconds;
						if( ButtonTimeEpsilonSeconds > diffsec && 100 == tlimit ) {
							mGameState = GameState.SynchronizedShotsFired;
							//PrintLine( "mGameState -> " + mGameState.ToString() );
						}
						else {
							mButtonUpSynchronizeTimestamp = null;
						}
					}
					else if( mGlimBlueGun.ButtonDownTimestamp.HasValue && mGlimRedGun.ButtonDownTimestamp.HasValue ) {
						// both buttons were down.. check for epilson sync
						double diffsec = ( mGlimRedGun.ButtonDownTimestamp.Value - mGlimBlueGun.ButtonDownTimestamp.Value ).TotalSeconds;
						if( ButtonTimeEpsilonSeconds > Math.Abs( diffsec ) ) {
							mButtonUpSynchronizeTimestamp = CurrentTime;
							//PrintLine( "down push was synchronized" );
						}
					}
				}
				d.ButtonDownTimestamp = null;
			}
			else if( !d.ButtonDownTimestamp.HasValue ) {
				d.ButtonDownTimestamp = CurrentTime;
			}
		}

		public bool EnableGame { set; private get; }

		double LuminanceStarlightMultiplier { get { return 0.6; } }
		double PerimeterLuminanceMultiplier {
			get {
				switch( mGameState ) {
					case GameState.Null:
					case GameState.SynchronizedShotsFired:
						return Luminance;
				}
				// take 2 seconds to return to full glow
				return Luminance * ( ( CurrentTime - mGameCoolDownStart ).TotalSeconds - 1 ) / 2;
			}
		}

		void RecalculateButtonGlimmer( GlimDescriptor d ) {
			ColorReal min;
			ColorReal max;
			ColorReal onHeld;
			if( null != d.Device.IPEndPoint ) {
				if( ( EnableGame && !d.CometIsRunning && GameState.Null == mGameState ) ) {
					min = d.PartyColor;
					min.Luminance = 0.1;
					max = d.PartyColor;
					max.Luminance = 0.3;
					onHeld = d.PartyColor;
					onHeld.Luminance = 0.55;
					d.Device.SetButtonColour( min, max, 1024, onHeld );
				}
				else {
					d.Device.ClearButtonColour();
				}
			}
		}

		void DebugClick() {
			if( GameState.Null == mGameState ) {
				mGameState = GameState.SynchronizedShotsFired;
				mGlimRedGun.StartComet( 100 );
				RecalculateButtonGlimmer( mGlimRedGun );
				mGlimBlueGun.StartComet( 100 );
				RecalculateButtonGlimmer( mGlimBlueGun );
			}
		}
	}

}
