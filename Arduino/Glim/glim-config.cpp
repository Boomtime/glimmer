#include "glim-config.h"
#include "glim.ino.h"
#include <FS.h>
namespace config {

namespace key {

static bool has_changed = false;

struct key_impl_s : key_s {
	String value;
	virtual operator String() const {
		return value;
	}
	virtual const String& operator = ( const String& v ) {
		DEBUG_PRINT( "config::key::key_s::operator =" );
		has_changed = true;
		value = v;
		return v;
	}
	virtual const char* c_str( void ) const {
		return this->value.c_str();
	}
	virtual int length() const {
		return this->value.length();
	}
};

namespace local {
	static struct key_impl_s device_name;
	static struct key_impl_s wifi_ssid;
	static struct key_impl_s wifi_psk;
}

struct key_s& device_name = local::device_name;
struct key_s& wifi_ssid = local::wifi_ssid;
struct key_s& wifi_psk = local::wifi_psk;

} // namespace key

static String fs_read_string( File f ) {
	char ret[0x80];
	int pos = 0;
	int c;
	while( -1 != ( c = f.read() ) ) {
		if( '\n' == c ) {
			break;
		}
		ret[pos++] = c;
	}
	ret[pos] = 0;
	return ret;
}

/**************************************************/

bool load( void ) {
	DEBUG_ENTER( config::load );

	if( !SPIFFS.begin() ) {
		DEBUG_PRINT( "SPIFFS.begin() failed" );
		return false;
	}

	FSInfo fs_info;
	SPIFFS.info( fs_info );

#	define DEBUG_FSINFO_PRINT(x) DEBUG_SPRINT( #x" [%i]", fs_info.x );
	DEBUG_FSINFO_PRINT( totalBytes );
	DEBUG_FSINFO_PRINT( usedBytes );
    DEBUG_FSINFO_PRINT( blockSize );
    DEBUG_FSINFO_PRINT( pageSize );
    DEBUG_FSINFO_PRINT( maxOpenFiles );
    DEBUG_FSINFO_PRINT( maxPathLength );

	{
		DEBUG_ENTER( root_directory );
		Dir root = SPIFFS.openDir( "/" );
		while( root.next() ) {
		    File f = root.openFile( "r" );
			String fn = root.fileName();
		    DEBUG_SPRINT( "%s [%i]", fn.c_str(), f.size() );
			f.close();
		}
	}

	File f = SPIFFS.open( "/config", "r" );
	if( !f ) {
		return false;
	}

	// got a config
	size_t len = f.size();
	DEBUG_SPRINT( "have a config of size [%i] bytes", len );

	key::device_name = fs_read_string( f );
	key::wifi_ssid = fs_read_string( f );
	key::wifi_psk = fs_read_string( f );
	key::has_changed = false;

	DEBUG_SPRINT( "DeviceName [%s]", key::device_name.c_str() );
	DEBUG_SPRINT( "WifiSSID [%s]", key::wifi_ssid.c_str() );
	DEBUG_SPRINT( "WifiPwd [len:%i]", key::wifi_psk.length() );

	f.close();
	return true;
}

bool save( bool force ) {
	DEBUG_ENTER( config::save );
	DEBUG_SPRINT( "force [%s] key::has_changed [%s]", force ? "true" : "false", key::has_changed ? "true" : "false" );
	if( key::has_changed || force ) {
		File f = SPIFFS.open( "/config", "w" );
		if( !f ) {
			DEBUG_PRINT( "failed to open /config for writing" );
			return false;
		}
		#define WK(k) f.write( key::k.c_str(), key::k.length() ); f.write( '\n' )
		WK( device_name );
		WK( wifi_ssid );
		WK( wifi_psk );
		f.close();
		DEBUG_PRINT( "apparently wrote all the things" );
		key::has_changed = false;
	}
	return true;
}


} // namespace config
