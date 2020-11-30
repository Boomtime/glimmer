namespace ShadowCreatures.Glimmer {
	using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
	using ShadowCreatures.Glimmer.Json;
	using JsonValueKind = System.Text.Json.JsonValueKind;

	/// <summary>
	/// device bindings are a bridge between sequences that generate colour data and the devices that receive it
	/// binding objects are created and owned by a sequence
	/// the generation engine associates these instances to physical glim devices when executing the sequence
	/// </summary>
	public interface IDeviceBinding {

		/// <summary>a descriptive display name, used for human interface</summary>
		string DisplayName { get; }

		/// <summary>last known hostname of the device that got bound, engine sets this during binding</summary>
		string HostName { get; set; }

		/// <summary>get colour data for the current frame</summary>
		Color[] FrameBuffer { get; }

		/// <summary>get the current button control definition</summary>
		ButtonColour ButtonColour { get; }

		/// <summary>called by the controller if a button press message is received</summary>
		/// <param name="btn">button state message</param>
		void OnButtonStateChanged( ButtonStatus btn );
	}

	public interface ISequence {
		/// <summary>sets sequence time, all methods called subsequently are in the context of the given sequence time</summary>
		/// <param name="elapsed">time elapsed in the current sequence</param>
		void SetCurrentTime( TimeSpan elapsed );

		/// <summary>called by the controller at reset to provide the set of known discovered devices</summary>
		/// <param name="devices">enumeration of known devices</param>
		void SetDiscoveredDevices( IEnumerable<IGlimDevice> devices );

		/// <summary>called by the controller to generate a complete frame output based on current time</summary>
		void FrameExecute();

		/// <summary>@todo: delete this, use the binding interface instead</summary>
		/// <param name="src"></param>
		/// <param name="btn"></param>
		void ButtonStateChanged( IGlimDevice src, ButtonStatus btn );

		/// <summary>sequence publishes the controls to export</summary>
		IControlDictionary Controls { get; }

		/// <summary></summary>
		IEnumerable<IDeviceBinding> Devices { get; }
	}

	class SequenceControlDictionary : Dictionary<string, IControlVariable>, IControlDictionary {
	}

	class SequenceDeviceBasic : IDeviceBinding, IGlimPacket {
		public SequenceDeviceBasic( string displayName, string hostName, int pixelCount ) {
			DisplayName = displayName;
			HostName = hostName;
			PixelCount = pixelCount;
		}

		public string DisplayName { get; }

		public string HostName { get; set; }

		public Color[] FrameBuffer { get; private set; }

		public ButtonColour ButtonColour { get; set; }

		public int PixelCount { 
			get { return FrameBuffer.Length; }
			set { FrameBuffer = new Color[value]; }
		}

		public virtual void OnButtonStateChanged( ButtonStatus btn ) {
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

		public void SetPixel( int pixel, Color src ) {
			FrameBuffer[pixel] = Blend( FrameBuffer[pixel], src );
		}
	}

	abstract class SequenceMinimum : ISequence {
		public virtual void SetCurrentTime( TimeSpan elapsed ) {
			CurrentTime = elapsed;
		}

		public virtual void SetDiscoveredDevices( IEnumerable<IGlimDevice> devices ) {
		}

		public virtual void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
		}

		public abstract void FrameExecute();

		public TimeSpan CurrentTime { get; private set; }

		protected IFxContext MakeCurrentContext() {
			return new FxContextSimple( CurrentTime );
		}

		protected SequenceControlDictionary Controls { get; } = new SequenceControlDictionary();

		ControlVariableRatio AddStandardControl( string name, Action<double> cb, double dv ) {
			var ctl = new ControlVariableRatio { Value = dv };
			ctl.ValueChanged += ( s, e ) => cb( ctl.Value );
			Controls.Add( name, ctl );
			return ctl;
		}

		protected ControlVariableRatio AddLuminanceControl( Action<double> cb, double dv =1.0 ) {
			return AddStandardControl( "Luminance", cb, dv );
		}
		protected ControlVariableRatio AddSaturationControl( Action<double> cb, double dv = 1.0 ) {
			return AddStandardControl( "Saturation", cb, dv );
		}

		IControlDictionary ISequence.Controls => Controls;

		public abstract IEnumerable<IDeviceBinding> Devices { get; }
	}

	/// <summary>publishes all discovered devices and maps to a single contiguous PixelMap</summary>
	abstract class SequenceDiagnostic : SequenceMinimum {
		readonly ICollection<IDeviceBinding> mDevices = new List<IDeviceBinding>();
		IGlimPixelMap mPixelMap = null;

		public override void SetDiscoveredDevices( IEnumerable<IGlimDevice> devices ) {
			var gf = new GlimPixelMap.Factory();
			foreach( var d in devices ) {
				var sd = new SequenceDeviceBasic( d.HostName, d.HostName, 300 );
				mDevices.Add( sd );
				gf.Add( sd );
			}
			mPixelMap = gf.Compile();
		}

		protected IGlimPixelMap PixelMap => mPixelMap;

		public override IEnumerable<IDeviceBinding> Devices => mDevices;
	}

	class SequenceConstructible : ISequence {

		class FxSequence {
			readonly List<IFx> mFx = new List<IFx>();
			readonly IGlimPixelMap mPixelMap;

			public FxSequence( IGlimPixelMap map, IEnumerable<IFx> fx ) {
				mFx.AddRange( fx );
				mPixelMap = map;
			}

			public void Execute( IFxContext ctx ) {
				mFx.ForEach( fx => mPixelMap.Write( fx.Execute( ctx ) ) );
			}
		}

		readonly List<FxSequence> mSequences = new List<FxSequence>();
		readonly Dictionary<string, FxSequence> mNamedSequences = new Dictionary<string, FxSequence>();
		readonly SequenceControlDictionary mControls = new SequenceControlDictionary();
		readonly List<IDeviceBinding> mDevices = new List<IDeviceBinding>();

		public void SetDiscoveredDevices( IEnumerable<IGlimDevice> devices ) {
		}

		public IEnumerable<IDeviceBinding> Devices => mDevices;

		public IControlDictionary Controls => mControls;

		public void AddDevice( IDeviceBinding dev ) {
			mDevices.Add( dev );
		}

		public void AddControl( string name, IControlVariable ctl ) {
			if( mControls.ContainsKey( name ) ) {
				throw new ExceptionMessage( "Control name '{0}' occurs twice.", name );
			}
			mControls.Add( name, ctl );
		}

		public void AddSequence( string name, IGlimPixelMap map, IEnumerable<IFx> fx ) {
			var seq = new FxSequence( map, fx );
			mSequences.Add( seq );
			if( !string.IsNullOrEmpty( name ) ) {
				if( mNamedSequences.ContainsKey( name ) ) {
					throw new ExceptionMessage( "Sequence name '{0}' occurs twice.", name );
				}
				mNamedSequences.Add( name, seq );
			}
		}

		public void SetCurrentTime( TimeSpan elapsed ) {
			CurrentTime = elapsed;
		}

		public TimeSpan CurrentTime { get; private set; }

		public void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
		}

		public void FrameExecute() {
			var ctx = new FxContextSimple( CurrentTime );
			mSequences.ForEach( s => s.Execute( ctx ) );
		}
	}

	class SequenceJson {

		class EffectsDictionary : Dictionary<string, Type> {
		}

		static EffectsDictionary sFxTypes = null;

		static EffectsDictionary Effects {
			get {
				if( null == sFxTypes ) {
					sFxTypes = new EffectsDictionary();
					foreach( var t in Assembly.GetExecutingAssembly().GetTypes().Where( t => typeof( IFx ).IsAssignableFrom( t ) ) ) {
						sFxTypes.Add( t.Name, t );
					}
				}
				return sFxTypes;
			}
		}

		static IFx InstantiateEffectByName( string name ) {
			if( !Effects.TryGetValue( name, out Type t ) ) {
				throw new ExceptionMessage( "Named effect '{0}' could not be found.", name );
			}
			return Activator.CreateInstance( t ) as IFx;
		}

		class JsonEffectParameterNotSupportedException : JsonException {
			public JsonEffectParameterNotSupportedException( string fxclass, JsonPath p )
				: base( p, "Effect class '{0}' does not support '{1}'", fxclass, p.Name ) {
			}
		}

		class JsonEffectParameterUnknownException : JsonException {
			public JsonEffectParameterUnknownException( string fxclass, JsonPath p )
				: base( p, "Effect class '{0}' has no matching parameter '{1}'", fxclass, p.Name ) {
			}
		}

		class JsonEffectParameterNotConfigurableException : JsonException {
			public JsonEffectParameterNotConfigurableException( string fxclass, JsonPath p )
				: base( p, "Effect class '{0}' cannot configure parameter '{1}'", fxclass, p.Name ) {
			}
		}

		class JsonEffectParameterValueOutOfRangeException : JsonException {
			public JsonEffectParameterValueOutOfRangeException( string fxclass, JsonValue v, string msg )
				: base( v, "Effect class '{0}' parameter '{1}' value '{2}' is out of range. {3}", fxclass, v.Name, v.ToString(), msg ) {
			}
		}

		class JsonEffectParameterInvalidSyntax : JsonException {
			public JsonEffectParameterInvalidSyntax( JsonValue v ) :
				base( v, "Parameter '{1}' value has invalid syntax.", v.Name ) {
			}
		}

		class JsonEffectParameterControlReferenceUnknown : JsonException {
			public JsonEffectParameterControlReferenceUnknown( JsonValue v ) :
				base( v, "Control reference unknown: {0}", v.ToString() ) {
			}
		}

		class GlimAliasDictionary : Dictionary<string, SequenceDeviceBasic> {
			public SequenceDeviceBasic FromJsonAlias( JsonValue alias ) {
				if( TryGetValue( alias.AsString(), out SequenceDeviceBasic d ) ) {
					return d;
				}
				throw new JsonValueInvalidException( alias, "Unknown device name" );
			}
		}

		readonly SequenceConstructible Program = new SequenceConstructible();
		readonly GlimAliasDictionary Devices = new GlimAliasDictionary();

		void AddJsonDevice( JsonObject jdev ) {
			var name = jdev["Name"].AsString();
			var hostname = jdev["HostName"].AsString();
			var pixelcount = jdev["PixelCount"].AsNumber();
			Debug.Print( "got device [{0}] HostName [{1}] PixelCount [{2}]", name, hostname, pixelcount );
			var dev = new SequenceDeviceBasic( name, hostname, (int)pixelcount ); // @todo: do we care about silently ignoring decimals in the JSON?
			Devices.Add( name, dev );
			Program.AddDevice( dev );
		}

		void SetEffectProperty( IFx fx, string classname, string property, JsonValue jsonv ) {
			var pi = fx.GetType().GetRuntimeProperty( property );
			if( null == pi ) {
				throw new JsonEffectParameterUnknownException( classname, jsonv );
			}
			// check for configurable
			var ca = pi.GetCustomAttribute<ConfigurableAttribute>( true );
			if( null == ca ) {
				throw new JsonEffectParameterNotConfigurableException( classname, jsonv );
			}
			// a JSON object as the value is always some complex solution, probably a control
			if( JsonValueKind.Object == jsonv.ValueKind ) {
				var vctl = jsonv.AsObject()["Control"];
				if( !Program.Controls.TryGetValue( vctl.AsString(), out IControlVariable ctl ) ) {
					throw new JsonEffectParameterControlReferenceUnknown( vctl );
				}
				// grab the current value
				ca.SetObjectProperty( fx, pi, ctl.Value, false );
				// set up for change triggers
				ctl.ValueChanged += ( s, e ) => ca.SetObjectProperty( fx, pi, ctl.Value, false );
			}
			else {
				// the property determines JSON interpretation
				switch( pi.PropertyType.Name ) {
					//case "String": -- lolwut?
					//	break;
					case "Int32":
					case "Int64":
					case "Double":
					case "Single":
						// test limits
						try {
							ca.SetObjectProperty( fx, pi, jsonv.AsNumber(), true );
						}
						catch( Exception ex ) {
							throw new JsonEffectParameterValueOutOfRangeException( classname, jsonv, ex.Message );
						}
						break;
					case "Color":
						// @todo: colour property parsing
						break;
				}
			}
		}

		IFx JsonParseEffect( JsonObject jfx ) {
			var cn = jfx["Class"].AsString();
			var fxo = InstantiateEffectByName( cn );
			if( fxo is IFxPipe ) {
				// must have sources (can be empty though)
				foreach( var jsrc in jfx["Sources"].AsArray() ) {
					( fxo as IFxPipe ).AddSource( JsonParseEffect( jsrc.AsObject() ) );
				}
			}
			else {
				// must not have sources
				if( jfx.ContainsKey( "Sources" ) ) {
					throw new JsonEffectParameterNotSupportedException( cn, jfx["Sources"] );
				}
			}
			// read parameters
			if( jfx.TryGetValue( "Parameters", out JsonValue parms ) ) {
				// parameters, if present, must be an object
				foreach( var kv in parms.AsObject() ) {
					SetEffectProperty( fxo, cn, kv.Key, kv.Value );
				}
			}
			return fxo;
		}

		void AddJsonSequence( JsonObject jseq ) {
			var name = jseq.GetOptionalPropertyAsStringOrNull( "Name" );
			var pmf = new GlimPixelMap.Factory();
			// load devices and pixelmaps
			foreach( var jdev in jseq["Devices"].AsArray() ) {
				switch( jdev.ValueKind ) {
					case JsonValueKind.String:
						pmf.Add( Devices.FromJsonAlias( jdev ) );
						break;
					case JsonValueKind.Object:
						var jobj = jdev.AsObject();
						pmf.Add( Devices.FromJsonAlias( jobj["Device"] ), (int)jobj["PixelStart"].AsNumber(), (int)jobj["PixelCount"].AsNumber() );
						break;
					default:
						throw new JsonKeyWrongTypeException( jdev, "Object", "String" );
				}
			}
			// load effects
			var fx = new List<IFx>();
			foreach( var jfx in jseq["Effects"].AsArray() ) {
				fx.Add( JsonParseEffect( jfx.AsObject() ) );
			}
			Program.AddSequence( name, pmf.Compile(), fx );
		}

		void AddJsonControl( JsonObject jctl ) {
			var name = jctl["Name"].AsString();
			var typestr = jctl["Type"].AsString();
			IControlVariable ctl;
			if( !Enum.TryParse<ControlType>( typestr, out ControlType type ) ) {
				throw new JsonValueInvalidException( jctl["Type"], "Type must be one of: " + string.Join( ",", Enum.GetNames( typeof( ControlType ) ) ) );
			}
			switch( type ) {
				case ControlType.Ratio:
					if( jctl.TryGetValue( "Default", out JsonValue jctlvalue ) ) {
						ctl = new ControlVariableRatio { Value = jctlvalue.AsNumber() };
					}
					else {
						ctl = new ControlVariableRatio();
					}
					break;
				case ControlType.Integer:
					int min = int.MinValue;
					int max = int.MaxValue;
					int step = 1;
					if( jctl.TryGetValue( "Min", out JsonValue jmin ) ) {
						min = (int)jmin.AsNumber();
					}
					if( jctl.TryGetValue( "Max", out JsonValue jmax ) ) {
						max = (int)jmax.AsNumber();
					}
					if( jctl.TryGetValue( "Step", out JsonValue jstep ) ) {
						step = (int)jstep.AsNumber();
					}
					if( jctl.TryGetValue( "Default", out JsonValue jdef ) ) {
						ctl = new ControlVariableInteger( min, max, step ) { Value = (int)jdef.AsNumber() };
					}
					else {
						ctl = new ControlVariableInteger( min, max, step );
					}
					break;
				case ControlType.Double:
					double dmin = double.MinValue;
					double dmax = double.MaxValue;
					if( jctl.TryGetValue( "Min", out jmin ) ) {
						dmin = jmin.AsNumber();
					}
					if( jctl.TryGetValue( "Max", out jmax ) ) {
						dmax = jmax.AsNumber();
					}
					ctl = new ControlVariableDouble( dmin, dmax );
					break;
				case ControlType.Colour:
					// @todo: implement colour parsing
					ctl = new ControlVariableColour();
					throw new NotImplementedException( "Nopeity nope.. haven't implemented Colour variables" );
					//break;
				default:
					throw new ArgumentException( "Unknown ControlType value" );
			}
			Program.AddControl( name, ctl );
		}

		public static ISequence Load( Stream file ) {
			var sj = new SequenceJson();
			var jroot = JsonObject.Load( file );
			foreach( var jdev in jroot["Devices"].AsArray() ) {
				sj.AddJsonDevice( jdev.AsObject() );
			}
			// controls are optional but must be an array of object if present
			if( jroot.TryGetValue( "Controls", out JsonValue jctlarray ) ) {
				foreach( var jctl in jctlarray.AsArray() ) {
					sj.AddJsonControl( jctl.AsObject() );
				}
			}
			foreach( var jseq in jroot["Sequences"].AsArray() ) {
				sj.AddJsonSequence( jseq.AsObject() );
			}
			return sj.Program;
		}
	}
}
