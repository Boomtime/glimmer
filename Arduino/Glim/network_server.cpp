#include "network_server.h"
#include "glim.ino.h"
#include "glim-config.h"
#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <ESP8266mDNS.h>
namespace network { namespace server {

static const char    AP_SSID[] =    "GlimSwarm";
static const uint8_t AP_IP[] =      { 192, 168, 0, 1 };
static const uint8_t AP_GATEWAY[] = { 192, 168, 0, 1 };
static const uint8_t AP_SUBNET[] = 	{ 255, 255, 255, 0 };

static const char HTML_FORM_CONTENT[] PROGMEM = 
	"<html><body><form method='post' action='/'>"
	"Device Name: <input type='text' name='device' value='%s' /><br />"
	"Wifi SSID: <input type='text' name='ssid' value='%s' /><br />"
	"Wifi pwd: <input type='text' name='pwd' value='' /><br />"
	"<input type='submit' value='Submit &amp; Reboot' /></form></body></html>";
static const int HTML_FORM_CONTENT_LEN = sizeof(HTML_FORM_CONTENT);

static ESP8266WebServer* http;

KERNEL_LOOP_DEFINE( network_server ) {
	http->handleClient();
	MDNS.update();
	return 20;
}

static void root_post( void ) {
	DEBUG_ENTER( network::server::root_post );
	DEBUG_SPRINT( "[%i] post arguments", http->args() );

	String device = http->arg( "device" );
	String ssid = http->arg( "ssid" );
	String pwd = http->arg( "pwd" );

	DEBUG_SPRINT( "device [%s] ssid [%s]", device.c_str(), ssid.c_str() );

	http->send( 200, "text/html", "<html><body><p>Got your stuff.. Rebooting now</p></body></html>" );
	//http->close(); @todo flush response here

	if( device && device.length() > 0 ) {
		config::key::device_name = device;
	}
	if( ssid && ssid.length() > 0 ) {
		config::key::wifi_ssid = ssid;
	}
	if( pwd && pwd.length() > 0 ) {
		config::key::wifi_psk = pwd;
	}
	config::save();
	shadowcreatures::arduino::kernel::reboot();
}

static void root_get( void ) {
	DEBUG_ENTER( network::server::root_get );
	String deviceName = config::key::device_name;
	String wifissid = config::key::wifi_ssid;
	char temp[HTML_FORM_CONTENT_LEN+200];
	snprintf_P( temp, sizeof(temp), HTML_FORM_CONTENT, deviceName.c_str(), wifissid.c_str() );
	temp[sizeof(temp)-1] = 0; // ANSI snprintf doesn't terminate the string if hit max
	http->send( 200, "text/html", temp );
}

void initialize( void ) {
	DEBUG_ENTER( network::server::initialize );

	String deviceName = config::key::device_name;
	String wifiSSID( AP_SSID );
	IPAddress ap_ip( AP_IP );
	IPAddress ap_gateway( AP_GATEWAY );
	IPAddress ap_subnet( AP_SUBNET );

	DEBUG_SPRINT( "deviceName [%s] wifiSSID [%s]", deviceName.c_str(), wifiSSID.c_str() );

	WiFi.hostname( deviceName ? deviceName : wifiSSID );
	//WiFi.mode( WIFI_AP );
	WiFi.softAPConfig( ap_ip, ap_gateway, ap_subnet );
	WiFi.softAP( wifiSSID );
	DEBUG_PRINT( "softAP started" );

	http = new ESP8266WebServer( 80 );
	http->on( "/", HTTP_GET, root_get );
	http->on( "/", HTTP_POST, root_post );
	http->begin();
	DEBUG_PRINT( "http service started" );

	// perhaps the client device will hear us if we shout...
	MDNS.begin( wifiSSID );
	MDNS.addService( "http", "tcp", 80 );
	DEBUG_PRINT( "mDNS advertising" );

	KERNEL_LOOP_ATTACH( network_server, 10 );
}

} } // namespace network::server
