namespace ShadowCreatures.Glimmer.Json {
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

	class JsonException : ExceptionMessage {
		public JsonException( JsonPath path, string fmt, params string[] args )
			: base( fmt, args ) {
			JsonPath = path;
		}

		public JsonPath JsonPath { get; }
	}

	/// <summary>required JSON key does jnot occur when it should</summary>
	class JsonKeyMissingException : JsonException {
		public JsonKeyMissingException( JsonPath path, string key )
			: base( path, "Required property '{0}' is missing.", key ) {
		}
	}

	/// <summary>the value of key is incorrect</summary>
	class JsonKeyWrongTypeException : JsonException {
		public JsonKeyWrongTypeException( JsonPath keyPath, string typeExpected )
			: base( keyPath, "Property '{0}' should be of type '{1}'.", keyPath.Name, typeExpected ) {
		}
		public JsonKeyWrongTypeException( JsonPath keyPath, string typeExpected, string typeExpected2 )
			: base( keyPath, "Property '{0}' should be of type '{1}' or '{2}'.", keyPath.Name, typeExpected, typeExpected2 ) {
		}
	}

	/// <summary>key is unexpected in this location</summary>
	class JsonKeyInvalidException : JsonException {
		public JsonKeyInvalidException( JsonPath path, string reason )
			: base( path, "Property '{0}' is not allowed at this location: {1}", path.Name, reason ) {
		}
	}

	/// <summary>value has invalid data</summary>
	class JsonValueInvalidException : JsonException {
		public JsonValueInvalidException( JsonValue v, string reason )
			: base( v, "Property '{0}' has invalid value '{1}': {2}.", v.Name, v.ToString(), reason ) {
		}
	}

	class JsonPath {
		internal JsonPath( string name, JsonElement self, JsonPath parent ) {
			Name = name;
			Parent = parent;
			Self = self;
		}

		public string Name { get; }

		public JsonPath Parent { get; }

		protected JsonElement Self { get; }

		protected virtual string FormatKeyPath( string basePath, string key ) {
			return string.Format( "{0}.{1}", basePath, key );
		}

		protected string MakeKeyPath( string key ) {
			if( null == Parent ) {
				return key;
			}
			// this would be stupid for deep paths but it only gets used for exceptions, so.. meh
			return FormatKeyPath( Parent.MakeKeyPath( Name ), key );
		}

		public string FullPath {
			get { return null == Parent ? Name : Parent.MakeKeyPath( Name ); }
		}
	}

	class JsonValue : JsonPath {

		internal JsonValue( string name, JsonElement self, JsonPath parent )
			: base( name, self, parent ) {
		}

		public JsonValueKind ValueKind {
			get { return Self.ValueKind; }
		}

		JsonElement AssertType( JsonValueKind type ) {
			if( Self.ValueKind != type ) {
				throw new JsonKeyWrongTypeException( this, type.ToString() );
			}
			return Self;
		}

		public string AsString() {
			return AssertType( JsonValueKind.String ).GetString();
		}

		public double AsNumber() {
			return AssertType( JsonValueKind.Number ).GetDouble();
		}

		public bool AsBoolean() {
			switch( ValueKind ) {
				case JsonValueKind.True:
					return true;
				case JsonValueKind.False:
					return false;
			}
			throw new JsonKeyWrongTypeException( this, "True or False" );
		}

		public JsonObject AsObject() {
			return new JsonObject( Name, AssertType( JsonValueKind.Object ), Parent );
		}

		public JsonArray AsArray() {
			return new JsonArray( Name, AssertType( JsonValueKind.Array ), Parent );
		}

		public override string ToString() {
			return Self.ToString();
		}
	}

	class JsonObject : JsonPath, IReadOnlyDictionary<string, JsonValue> {
		readonly Dictionary<string, JsonValue> mElements = new Dictionary<string, JsonValue>();

		internal JsonObject( string name, JsonElement self, JsonPath parent )
			: base( name, self, parent ) {
			foreach( var e in self.EnumerateObject() ) {
				mElements.Add( e.Name, new JsonValue( e.Name, e.Value, this ) );
			}
		}

		public IEnumerable<string> Keys => mElements.Keys;

		public IEnumerable<JsonValue> Values => mElements.Values;

		public int Count => mElements.Count;

		public bool TryGetValue( string key, out JsonValue value ) => mElements.TryGetValue( key, out value );

		public JsonValue this[string key] {
			get {
				if( TryGetValue( key, out JsonValue v ) ) {
					return v;
				}
				throw new JsonKeyMissingException( this, key );
			}
		}

		public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator() => mElements.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool ContainsKey( string key ) => mElements.ContainsKey( key );

		/// <summary>locates an optional property, returns null if the key does not exist</summary>
		/// <param name="key">name of the property to extract</param>
		/// <param name="allowNullInJson">true (default) to permit JSON to explicitly specify null for the value, false to throw if null is given in JSON</param>
		/// <returns>property value as a string</returns>
		/// <exception cref="JsonExceptionOptionalKeyWrongType">if property is given but is not a string, or property is given as null but allowNull is false</exception>
		public string GetOptionalPropertyAsStringOrNull( string key, bool allowNullInJson = true ) {
			if( !TryGetValue( key, out JsonValue v ) ) {
				return null;
			}
			switch( v.ValueKind ) {
				case JsonValueKind.String:
					return v.AsString();
				case JsonValueKind.Null:
					if( allowNullInJson ) {
						return null;
					}
					break;
			}
			throw new JsonKeyWrongTypeException( v, "String" );
		}

		public static JsonObject Load( Stream file ) {
			var jroot = JsonDocument.Parse( file, new JsonDocumentOptions {
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip
			} ).RootElement;
			return new JsonObject( string.Empty, jroot, null );
		}
	}

	class JsonArray : JsonPath, IReadOnlyList<JsonValue> {
		readonly List<JsonValue> mElements = new List<JsonValue>();

		internal JsonArray( string name, JsonElement self, JsonPath parent )
			: base( name, self, parent ) {
			int index = 0;
			foreach( var e in self.EnumerateArray() ) {
				mElements.Add( new JsonValue( index.ToString(), e, this ) );
				index++;
			}
		}

		protected override string FormatKeyPath( string basePath, string key ) {
			return string.Format( "{0}[{1}]", basePath, key );
		}

		public int Count {
			get { return mElements.Count; }
		}

		public JsonValue this[int index] {
			get { return mElements[index]; }
		}

		public IEnumerator<JsonValue> GetEnumerator() {
			return mElements.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return mElements.GetEnumerator();
		}
	}
}
