namespace ShadowCreatures.Glimmer {
	using ShadowCreatures.Glimmer.Effects;
	using System;
    using System.Collections.Generic;
    using System.Drawing;

    class SequenceParty : SequenceMinimum {
		const double ButtonTimeEpsilonSeconds = 0.2;

		/// <summary>internal descriptor of a device</summary>
		class GlimDescriptor : SequenceDeviceBasic {
			readonly IGlimPixelMap mPixelMap;
			FxComet mFxComet;

			public GlimDescriptor( string displayName, string hostName, int pixelCount, Color partyColor )
				: base( displayName, hostName, pixelCount ) {
				PartyColor = partyColor;
				mFxComet = null;
				mPixelMap = new GlimPixelMap.Factory { { this, 0, 100 } }.Compile();
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
		readonly SequenceDeviceBasic mGlimStars;
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

		public SequenceParty() {
			Luminance = AddLuminanceControl( x => { } );
			Saturation = AddSaturationControl( x => { } );

			// comets!
			mGlimRedGun = new GlimDescriptor( "Red", "GlimSwarm-103", 150, Color.Red );
			mGlimBlueGun = new GlimDescriptor( "Blue", "GlimSwarm-104", 100, Color.Blue );
			mGlimStars = new SequenceDeviceBasic( "Stars", "GlimSwarm-102", 150 );
			mFxBarrel = new FxComet { BaseColor = Color.FromArgb( 0xff, 0, 0xff ), PixelCount = 50 };
			mFxCannonTwinkle = new FxStarlightTwinkle {
				BaseColor = Color.FromArgb( 0xff, 0, 0xff ),
				SpeedFactor = 15.0,
				LuminanceMinima = 0.2,
				LuminanceMaxima = 0.8
			};

			mPixelMapStars = new GlimPixelMap.Factory { mGlimStars }.Compile();
			mPixelMapBarrel = new GlimPixelMap.Factory { { mGlimRedGun, 100, 50 } }.Compile();
			mPixelMapPerimeter = new GlimPixelMap.Factory { { mGlimRedGun, 0, 100 }, { mGlimBlueGun, 100, -100 } }.Compile();

			mFxPerimeterRainbow = new FxScale( new FxRainbow() );
			mFxStarlight = new FxScale( new FxStarlightTwinkle { BaseColor = Color.Yellow } ) { Saturation = 0.3 };
		}

		IEnumerable<Color> BarrelColour() {
			while( true ) {
				yield return Color.Black;
			}
		}

		public ControlVariableRatio Luminance { get; }

		public ControlVariableRatio Saturation { get; }


		public override void FrameExecute() {
			var ctx = MakeCurrentContext();
			mFxPerimeterRainbow.Luminance = PerimeterLuminanceMultiplier;
			mFxPerimeterRainbow.Saturation = Saturation.Value;
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

		public override IEnumerable<IDeviceBinding> Devices {
			get {
				yield return mGlimRedGun;
				yield return mGlimBlueGun;
				yield return mGlimStars;
			}
		}

		public override void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
			GlimDescriptor d;
			if( src.HostName == mGlimRedGun.HostName ) {
				d = mGlimRedGun;
			}
			else if( src.HostName == mGlimBlueGun.HostName ) {
				d = mGlimBlueGun;
			}
			else {
				if( null == src.HostName ) {
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
						return Luminance.Value;
				}
				// take 2 seconds to return to full glow
				return Luminance.Value * ( ( CurrentTime - mGameCoolDownStart ).TotalSeconds - 1 ) / 2;
			}
		}

		void RecalculateButtonGlimmer( GlimDescriptor d ) {
			ColorReal min;
			ColorReal max;
			ColorReal onHeld;
			if( ( EnableGame && !d.CometIsRunning && GameState.Null == mGameState ) ) {
				min = d.PartyColor;
				min.Luminance = 0.1;
				max = d.PartyColor;
				max.Luminance = 0.3;
				onHeld = d.PartyColor;
				onHeld.Luminance = 0.55;
				d.ButtonColour = new ButtonColour( min, max, 1024, onHeld );
			}
			else {
				d.ButtonColour = ButtonColour.Off;
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
