/***************************************************
 *
 */
#include "glim.ino.h"
#include "rgb.h"
#include "network_client.h"
#include "network_server.h"
//#include "glim-config.h"
#include <ESP8266WiFi.h>

using namespace shadowcreatures::arduino;
namespace net = network::client;

namespace glim {
/**************************************************/

namespace PIN { enum {
	DEADMAN 		= NODEPIN::D7,
	REED 			= NODEPIN::D6,
	BUTTON 			= NODEPIN::D5,
	SERIAL_ENABLE 	= NODEPIN::D1,
	RGB_TX 			= LED_BUILTIN 		// the builtin LED is ganged with UART1 TX
}; }

namespace LAMP {
	static const rgb_t DARK = 		{ 0, 0, 0 };
	static const rgb_t FATAL = 		{ 255, 20, 20 };
	static const rgb_t ON_CONNECTED_SPARK = { 40, 255, 40 };
	static const rgb_t ON_DISCONNECTED_MIN = { 20, 0, 0 };
	static const rgb_t ON_DISCONNECTED_MAX = { 80, 0, 0 };
	static const rgb_t CONFIG_MIN = { 10, 0, 0 };
	static const rgb_t CONFIG_MAX = { 0, 0, 150 };
	static const rgb_t BOOT_MIN = 	{ 0, 20, 20 };
	static const rgb_t BOOT_MAX = 	{ 0, 0, 50 };
} // namespace LAMP

const rgb_t rgb_t::off;


/**************************************************/
namespace local {

static struct button_s {
	bool enabled;
	rgb_t min;
	rgb_t max;
	uint period;
	rgb_t held;
	int down_tick_count;
	net::BUTTON::state state; // @todo: remove?
} button = { false };

#define SYSTEM_LAMP_GLIMMER_PERIOD 2000
#define SYSTEM_LAMP_SPARK_PERIOD 2500
#define BUTTON_LAMP_SPARK_PERIOD 1000

static volatile bool button_interrupt_flag = false;


} // namespace local
/**************************************************/

static bool is_deadman( void ) {
	static int deadman = -1;
	if( -1 == deadman ) {
		pinMode( PIN::DEADMAN, INPUT_PULLUP );
		delay( 1 ); // ensure deadman debounce -- need to be certain on this one
		deadman = digitalRead( PIN::DEADMAN );
		if( deadman > 0 ) {
			Serial.println( "" );
			Serial.println( "*** deadman" );
			Serial.println( "" );
		}
	}
	return deadman > 0;
}

#define deadman_check() if( glim::is_deadman() ) return



/**************************************************/

#ifdef DEBUG_BUILD
KERNEL_LOOP_DEFINE( debug_metrics_print ) {
	DEBUG_TRACE( debug_metrics_print );
	static uint target_time = 0;
	static struct { uint recv; uint sent; } nettraffic = { 0, 0 };
	kernel::cpu::stats_t cpu;
	bool connected;
	int strength = -200;
	connected = net::is_connected( &strength );
	cpu = kernel::cpu::sample( 1000 ); // default sample precision is percentage
	if( 0 == cpu.user && 0 == cpu.free && 0 == cpu.system ) {
		return 100; // skip until we get a valid sample
	}
	const char* wifi = "no wifi";
	if( connected ) {
		wifi = va( "wifi @ %i dbm, %i tx/s, %i rx/s", strength, ( net::counters::sent() - nettraffic.sent ) / 5, ( net::counters::recv() - nettraffic.recv ) / 5 );
		nettraffic.sent = net::counters::sent();
		nettraffic.recv = net::counters::recv();
	}
	DEBUG_SPRINT( ">>> cpu.user: %i.%i%%, cpu.system: %i.%i%%, heap: %i bytes, %s",
		cpu.user / 10, cpu.user % 10, cpu.system / 10, cpu.system % 10, ESP.getFreeHeap(), wifi );
	do {
		target_time += 5000;
	} while( target_time <= millis() );
	return target_time - millis();
}
#define DEBUG_METRICS_LOOP_ATTACH() KERNEL_LOOP_ATTACH( debug_metrics_print, 500 )
#else
#define DEBUG_METRICS_LOOP_ATTACH()
#endif


/**************************************************/

static void set_system_lamp_glimmer( const rgb_t& min, const rgb_t& max ) {
	DEBUG_TRACE( set_system_lamp_glimmer );
	rgb::lamp::system.set_colour( max );
	rgb::lamp::system.set_glimmer( min, SYSTEM_LAMP_GLIMMER_PERIOD );
}

static void set_system_lamp_spark( const rgb_t& max ) {
	DEBUG_TRACE( set_system_lamp_spark );
	rgb::lamp::system.set_colour( max );
	rgb::lamp::system.set_fader( SYSTEM_LAMP_SPARK_PERIOD );
}

static void commit_button_lamp_glimmer( void ) {
	DEBUG_TRACE( commit_button_lamp_glimmer );
	rgb::lamp::button.set_colour( local::button.min );
	rgb::lamp::button.set_glimmer( local::button.max, local::button.period );
}

static void commit_button_lamp_spark( void ) {
	DEBUG_TRACE( commit_button_lamp_spark );
	rgb::lamp::button.set_colour( local::button.held );
	rgb::lamp::button.set_fader( BUTTON_LAMP_SPARK_PERIOD );
}


/**************************************************/

KERNEL_LOOP_DEFINE( button_glimmer ) {
	if( !local::button.enabled ) {
		return 250;
	}
	// deal with events that occurred while we were sleeping (and debounce)
	bool was_button_interrupt = local::button_interrupt_flag && 0 == local::button.down_tick_count;
	#ifdef DEBUG_BUILD
	if( local::button_interrupt_flag && !was_button_interrupt ) {
		DEBUG_PRINT( "button_glimmer(): (de)bounced" );
	}
	#endif
	local::button_interrupt_flag = false;
	if( was_button_interrupt ) {
		net::send_button_state( net::BUTTON::DOWN );
		commit_button_lamp_spark();
		local::button.down_tick_count = 1;
	}
	// read current state and send off those conditions
	if( 0 == digitalRead( PIN::BUTTON ) ) {
		// down... and was it last time too?
		if( 10 <= local::button.down_tick_count ) {
			net::send_button_state( net::BUTTON::HELD );
			local::button.down_tick_count = 1;
		}
		else {
			local::button.down_tick_count ++;
		}
	}
	else if( 0 < local::button.down_tick_count ) {
		// up..
		local::button.down_tick_count = 0;
		net::send_button_state( net::BUTTON::UP );
		commit_button_lamp_glimmer();
	}
	return 10;
}

static void ICACHE_RAM_ATTR button_interrupt( void ) {
	// called when the button is initially depressed
	// no interrupt for released or held.. poll for those
	local::button_interrupt_flag = true;
}

static void net_button_glimmer_recv( rgb_t min, rgb_t max, short period, rgb_t held ) {
	local::button.min = min;
	local::button.max = max;
	local::button.period = period;
	local::button.held = held;
	if( ( min.r || min.g || min.b || max.r || max.g || max.b ) && period > 0 ) {
		if( !local::button.enabled ) {
			local::button.enabled = true;
			local::button.down_tick_count = 0;
			commit_button_lamp_glimmer();
			attachInterrupt( digitalPinToInterrupt( PIN::BUTTON ), button_interrupt, FALLING );
		}
	}
	else if( local::button.enabled ) {
		local::button.enabled = false;
		rgb::lamp::button.set_colour( rgb_t::off );
		detachInterrupt( digitalPinToInterrupt( PIN::BUTTON ) );
	}
}

static void net_status_changed( bool connected ) {
	if( connected ) {
		set_system_lamp_spark( LAMP::ON_CONNECTED_SPARK );
	}
	else {
		set_system_lamp_glimmer( LAMP::ON_DISCONNECTED_MIN, LAMP::ON_DISCONNECTED_MAX );
	}
}


} // namespace glim
/**************************************************/
using namespace glim;

// main setup
void setup() {
	Serial.begin( 115200 );
	Serial.println( "" );
	Serial.println( "*" );
	Serial.println( "*  ShadowCreatures GlimSwarm v4.2 (" __DATE__ ")" );
	Serial.println( "*" );
	Serial.println( "" );
	deadman_check();

	DEBUG_ENTER( setup );
	DEBUG_SPRINT( "heap [%i]", ESP.getFreeHeap() );

	pinMode( PIN::BUTTON, INPUT_PULLUP );
	pinMode( PIN::REED, INPUT_PULLUP );

#	ifdef DEBUG_BUILD
	int endian_test = 1;
	DEBUG_SPRINT( "architecture is %s", ((uint8_t*)&endian_test)[0] ? "LITTLE endian" : "BIG endian" );
#	endif

	kernel::setup();

	DEBUG_METRICS_LOOP_ATTACH();

	rgb::initialize();

	KERNEL_LOOP_ATTACH( button_glimmer, 50 );

	if( 0 == digitalRead( PIN::REED ) ) {
		// configuration lighthouse mode
		set_system_lamp_glimmer( LAMP::CONFIG_MIN, LAMP::CONFIG_MAX );
		config::load(); // blind load config, ignore result
		network::server::initialize();
	}
	else if( config::load() ) {
		// normal operating mode
		set_system_lamp_glimmer( LAMP::BOOT_MIN, LAMP::BOOT_MAX );
		net::initialize();
		net::on_msg_rgb( rgb::write );
		net::on_net_status_changed( net_status_changed );
		net::on_msg_btnc( net_button_glimmer_recv );
	}
	else {
		// failure, halt
		DEBUG_PRINT( "failed to load config" );
		set_system_lamp_glimmer( LAMP::DARK, LAMP::FATAL );
	}
}

// main loop
void loop() {
	deadman_check();
	DEBUG_TRACE( loop );
	kernel::loop();
}
