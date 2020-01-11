#include "network_client.h"
#include "glim-config.h"
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
namespace network { namespace client {

using glim::colour_t;
using glim::rgb_t;
using namespace shadowcreatures::arduino;

#define TIME_BETWEEN_ATTEMPTS_MS  10000

namespace MSG { typedef enum { 
	RGB = 1,
	PING = 3,
	PONG = 4,
	BTNS = 5,
	BTNC = 6,
} message; }

namespace HARDWARE { typedef enum {
	SERVER = 1,
	GLIM_V2 = 2,
	GLIM_V3 = 3,
	GLIM_V4 = 4,
} type; }

enum {
	UDP_PORT = 1998,
};

namespace local {

static WiFiUDP socket;
static bool is_connected;
static uint next_attempt;
static IPAddress server_ip;
static uint16_t server_port;
static net_status_connected_cb* cb_status;
static msg_rgb_cb* cb_rgb;
static msg_btnc_cb* cb_btnc;

} // namespace local


static bool read_rgb( rgb_t& rgb ) {
	if( 3 > local::socket.available() ) {
		return false;
	}
	rgb.r = local::socket.read();
	rgb.g = local::socket.read();
	rgb.b = local::socket.read();
	return true;
}

static bool read_short( short& s ) {
	short rs; // in standard C a short on the arg stack is passed as int
	if( 2 > local::socket.available() ) {
		return false;
	}
	s = ( local::socket.read() << 8 );
	s |= local::socket.read();
	return true;
}

static void write_integer( int to_write ) {
#	define WRITE_BYTE(b) local::socket.write( uint8_t( ( to_write >> b ) & 0xff ) )
	WRITE_BYTE( 24 );
	WRITE_BYTE( 16 );
	WRITE_BYTE(  8 );
	WRITE_BYTE(  0 );
#	undef WRITE_BYTE
}

static void transmit_ident( MSG::message msg_type, IPAddress remote_ip, uint16_t remote_port ) {
	uint b, f;
	String hostname = WiFi.hostname();
	// <ping/pong> <hw-type> <hostname> <uptime>
	// ping/pong are byte
	// hw-type is byte
	// hostname is a pascal string
	// uptime is a big-endian int32
	local::socket.beginPacket( remote_ip, remote_port );
	local::socket.write( uint8_t( msg_type ) );
	local::socket.write( uint8_t( HARDWARE::GLIM_V4 ) );
	local::socket.write( uint8_t( hostname.length() ) );
	local::socket.write( (uint8_t*)( hostname.c_str() ), hostname.length() );
	write_integer( glim::uptime() );
	kernel::cpu::sample( b, f );
	local::socket.write( uint8_t( 255 * b / ( b + f ) ) );
	local::socket.write( local::is_connected ? uint8_t( 255 + WiFi.RSSI() ) : 255 );
	local::socket.endPacket();
}

static void process_msg_PING( void ) {
	if( HARDWARE::SERVER == HARDWARE::type( local::socket.read() ) ) {
		local::server_ip = local::socket.remoteIP();
		local::server_port = local::socket.remotePort();
		transmit_ident( MSG::PONG, local::server_ip, local::server_port );
	}
}

static void process_msg_PONG( void ) {
	if( HARDWARE::SERVER == HARDWARE::type( local::socket.read() ) ) {
		local::server_ip = local::socket.remoteIP();
		local::server_port = local::socket.remotePort();
	}
}

static void process_msg_BTNC( void ) {
	rgb_t min, max, held;
	short period;
	// <rgb> <rgb> <short> <rgb>
	if( read_rgb( min ) && read_rgb( max ) && read_short( period ) && read_rgb( held ) && local::cb_btnc ) {
		local::cb_btnc( min, max, period, held );
	}
}

// return true if a packet was processed
static bool check_process_msg( void ) {
	// if there's data available, read a packet
	int packetSize = local::socket.parsePacket();
	if( 0 >= packetSize ) {
		return false;
	}
	uint8_t msg = local::socket.read();
	switch( msg ) {
	case MSG::RGB:
		if( local::cb_rgb ) {
			local::cb_rgb( local::socket );
		}
		break;
	case MSG::PING:
		process_msg_PING();
		break;
	case MSG::PONG:
		process_msg_PONG();
		break;
	case MSG::BTNC:
		process_msg_BTNC();
		break;
	default:
		DEBUG_SPRINT( "net::check_process_msg(): mystery [%i] from [%s:%i]", msg, local::socket.remoteIP().toString().c_str(), local::socket.remotePort() );
		break;
	}
	return true;
}

static void wifi_loop( void ) {
	static bool was_connected = false;
	wl_status_t now_status = WiFi.status();
#ifdef DEBUG_BUILD
	static wl_status_t last_status = WL_NO_SHIELD;
	if( last_status != now_status ) {
		DEBUG_SPRINT( "wifi status changed from [%i] to [%i]", last_status, now_status );
		last_status = now_status;
	}
#endif
	switch( now_status ) {
	case WL_CONNECTED:
		if( !was_connected ) {
			DEBUG_SPRINT( "wifi_loop(): wifi connected, strength [%i]", WiFi.RSSI() );
			DEBUG_SPRINT( "ip [%s] subnet [%s]", WiFi.localIP().toString().c_str(), WiFi.subnetMask().toString().c_str() );
			DEBUG_SPRINT( "gateway [%s] dns [%s]", WiFi.gatewayIP().toString().c_str(), WiFi.dnsIP().toString().c_str() );
			DEBUG_SPRINT( "ap-mac [%s]", WiFi.BSSIDstr().c_str() );
			was_connected = true;
			if( local::cb_status ) {
				local::cb_status( true );
			}
		}
		break;
	default:
		if( was_connected ) {
			DEBUG_PRINT( "wifi DISconnected" );
			was_connected = false;
			local::next_attempt = millis() + TIME_BETWEEN_ATTEMPTS_MS;
			if( local::cb_status ) {
				local::cb_status( false );
			}
		}
		else if( local::next_attempt < millis() ) {
			DEBUG_PRINT( "attempting connection" );
			local::next_attempt = millis() + TIME_BETWEEN_ATTEMPTS_MS;
			WiFi.begin( config::key::wifi_ssid, config::key::wifi_psk );
		}
		break;
	}
}

KERNEL_LOOP_DEFINE( network_client ) {
	DEBUG_TRACE( network::client::loop );
	wifi_loop();
	if( WiFi.isConnected() ) {
		if( !local::is_connected ) {
			DEBUG_PRINT( "wifi has connected" );
			local::socket.begin( UDP_PORT );
			local::is_connected = true;
			transmit_ident( MSG::PING, IPAddress( 255, 255, 255, 255 ), UDP_PORT );
		}
		if( check_process_msg() ) {
			// we read a packet so it'd be nice to return sooner to keep processing
			return 0;
		}
	}
	else {
		local::is_connected = false;
	}

	return 20;
}


#ifdef DEBUG_BUILD
static const char* encTypeToString( int encType ) {
	switch( encType ) {
	case ENC_TYPE_WEP:
		return "WEP";
	case ENC_TYPE_TKIP:
		return "WPA";
	case ENC_TYPE_CCMP:
		return "WPA2";
	case ENC_TYPE_NONE:
		return "None";
	case ENC_TYPE_AUTO:
		return "Auto";
	}
	return "Unknown";
}
static void DEBUG_SCAN_NETWORKS( void ) {
	DEBUG_ENTER( DEBUG_SCAN_NETWORKS );
	int numSsid = WiFi.scanNetworks();
	DEBUG_SPRINT( "numSsid [%i]", numSsid );
	for( int k = 0 ; k < numSsid ; k++ ) {
		DEBUG_SPRINT( "[%02i] %s @ %idbm %s", k, WiFi.SSID(k).c_str(), WiFi.RSSI(k), encTypeToString(WiFi.encryptionType(k)) );
	}
}
#else
#define DEBUG_SCAN_NETWORKS()
#endif


/*****************************************************************/

void initialize( void ) {
	DEBUG_ENTER( network::client::initialize );
	DEBUG_SCAN_NETWORKS();

	String device_name = config::key::device_name;

	DEBUG_SPRINT( "deviceName [%s] wifiSSID [%s]", device_name.c_str(), config::key::wifi_ssid.c_str() );

	local::is_connected = false;
	local::next_attempt = 0;
	local::cb_status = 0;
	local::cb_rgb = 0;
	local::cb_btnc = 0;

	WiFi.hostname( device_name );
	WiFi.mode( WIFI_STA );
	WiFi.setAutoReconnect( false ); // all of these are broken
	WiFi.setAutoConnect( false );   // if these are ever set you can never change your mind or arduino will core-dump
	WiFi.persistent( false );

	DEBUG_PRINT( "config and hostname set" );
	DEBUG_SPRINT( "mac [%s]", WiFi.macAddress().c_str() );

	KERNEL_LOOP_ATTACH( network_client, 20 );
}

void on_msg_rgb( msg_rgb_cb* cb ) {
	local::cb_rgb = cb;
}

void on_net_status_changed( net_status_connected_cb* cb ) {
	local::cb_status = cb;
}

void on_msg_btnc( msg_btnc_cb* cb ) {
	local::cb_btnc = cb;
}

void send_button_state( BUTTON::state s ) {
	// <btns> <btn-state>
	local::socket.beginPacket( local::server_ip, local::server_port );
	local::socket.write( uint8_t( MSG::BTNS ) );
	local::socket.write( uint8_t( s ) );
	local::socket.endPacket();
}

bool is_connected( int* dbm ) {
	if( dbm && local::is_connected ) {
		*dbm = WiFi.RSSI();
	}
	return local::is_connected;
}


} } // namespace network::client
