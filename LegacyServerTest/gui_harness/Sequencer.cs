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

	public interface ISequence {
		void Execute();

		void ButtonStateChanged( IGlimDevice src, ButtonStatus btn );

		/// <summary>set current sequence time, when set it must not go backwards!</summary>
		TimeSpan CurrentTime { get; set; }

		IControlDictionary Controls { get; }
	}

	class SequenceControlDictionary : Dictionary<string, ControlVariable>, IControlDictionary {
	}

	abstract class SequenceDefault : ISequence {
		public ControlVariableRatio Luminance {
			get => Controls["Luminance"] as ControlVariableRatio;
		}
		public ControlVariableRatio Saturation {
			get => Controls["Saturation"] as ControlVariableRatio;
		}

		public virtual void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
		}

		public abstract void Execute();

		public TimeSpan CurrentTime { get; set; }

		protected IFxContext MakeCurrentContext() {
			return new FxContextSimple( CurrentTime );
		}

		public virtual IControlDictionary Controls { get; } = new SequenceControlDictionary {
			{ "Luminance", new ControlVariableRatio { Value = 1.0 } },
			{ "Saturation", new ControlVariableRatio { Value = 1.0 } }
		};
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

		public void AddControl( string name, ControlVariable ctl ) {
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

		public TimeSpan CurrentTime { get; set; }

		public void ButtonStateChanged( IGlimDevice src, ButtonStatus btn ) {
		}

		public void Execute() {
			var ctx = new FxContextSimple( CurrentTime );
			mSequences.ForEach( s => s.Execute( ctx ) );
		}

		public IControlDictionary Controls => mControls;
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

		class GlimAliasDictionary : Dictionary<string, GlimDevice> {
			public GlimDevice FromJsonAlias( JsonValue alias ) {
				if( TryGetValue( alias.AsString(), out GlimDevice d ) ) {
					return d;
				}
				throw new JsonValueInvalidException( alias, "Unknown device name" );
			}
		}

		readonly GlimManager Manager;
		readonly SequenceConstructible Program;
		readonly GlimAliasDictionary Devices;

		SequenceJson( GlimManager mgr ) {
			Manager = mgr;
			Program = new SequenceConstructible();
			Devices = new GlimAliasDictionary();
		}

		GlimDevice JsonParseDevice( JsonObject jdevice, out string name ) {
			name = jdevice["Name"].AsString();
			var hostname = jdevice["Hostname"].AsString();
			var pixelcount = jdevice["PixelCount"].AsNumber();
			Debug.Print( "got device [{0}] hostname [{1}] pixelcount [{2}]", name, hostname, pixelcount );
			var d = Manager.FindOrCreate( hostname );
			d.PixelCount = (int)pixelcount; // @todo: do we care about silently ignoring decimals in the JSON?
			return d;
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
				if( !Program.Controls.TryGetValue( vctl.AsString(), out ControlVariable ctl ) ) {
					throw new JsonEffectParameterControlReferenceUnknown( vctl );
				}
				// grab the current value
				ca.SetObjectProperty( fx, pi, ctl.Value, false );
				// set up for change triggers
				ctl.ValueChanged += ( s, a ) => ca.SetObjectProperty( fx, pi, a.Value, false );
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
			ControlVariable ctl;
			if( !Enum.TryParse<ControlType>( typestr, out ControlType type ) ) {
				throw new JsonValueInvalidException( jctl["Type"], "Type must be one of: " + string.Join( ",", Enum.GetNames( typeof( ControlType ) ) ) );
			}
			switch( type ) {
				case ControlType.Ratio:
					ctl = new ControlVariableRatio();
					if( jctl.TryGetValue( "Default", out JsonValue jctlvalue ) ) {
						ctl.Value = jctlvalue.AsNumber();
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
					ctl = new ControlVariableInteger( min, max, step );
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

		ISequence Load( Stream file ) {
			var jroot = JsonObject.Load( file );
			foreach( var jdev in jroot["Devices"].AsArray() ) {
				var glim = JsonParseDevice( jdev.AsObject(), out string name );
				Devices.Add( name, glim );
			}
			// controls are optional but must be an array of object if present
			if( jroot.TryGetValue( "Controls", out JsonValue jctlarray ) ) {
				foreach( var jctl in jctlarray.AsArray() ) {
					AddJsonControl( jctl.AsObject() );
				}
			}
			foreach( var jseq in jroot["Sequences"].AsArray() ) {
				AddJsonSequence( jseq.AsObject() );
			}
			return Program;
		}

		public static ISequence Load( GlimManager mgr, Stream jsonFile ) {
			return new SequenceJson( mgr ).Load( jsonFile );
		}
	}
}
