#include "rgb.h"
namespace rgb {

#define DATA_LOOP_BUFFER	60
#define MAX_FADER_TIME_MS	60000

using namespace shadowcreatures::arduino;
using glim::colour_t;
using glim::rgb_t;

namespace PIN { enum {
	RGB_ENABLE = NODEPIN::D1, 	// serial output selector pin on glim v3.4 (NodeMCU pin D1)
	RGB_TX = LED_BUILTIN 		// the builtin LED is ganged with UART1 TX
}; }


/********************************************************/
namespace lamp {

static bool needs_update = true;
static uint last_update_time = 0;

struct lamp_impl_s : lamp_api_s {
	rgb_t value;// current
	rgb_t max;  // glimmer max (and fade start)
	rgb_t min;	// glimmer min
	uint period;// glimmer period (and fade duration)
	uint fade_time_target;

	virtual void set_colour( const glim::rgb_t& );
	virtual void set_glimmer( const glim::rgb_t& bound, uint period_ms );
	virtual void set_fader( uint duration_ms );

	// runs programs, sets global "needs_update" if value changed
	void update( void );
};

namespace local {
	static struct lamp_impl_s system;
	static struct lamp_impl_s button;
}

struct lamp_api_s& system = local::system;
struct lamp_api_s& button = local::button;


static int sawtooth_by_range( uint period_ms, int range ) {
	uint phase = millis() % period_ms;
	uint half_period = period_ms >> 1;
	return phase >= half_period ? range * int( period_ms - phase ) / int( half_period ) : range * int( phase ) / int( half_period );
}

void lamp_impl_s::set_colour( const glim::rgb_t& c ) {
	this->period = 0;
	this->value = c;
	needs_update = true;
}

void lamp_impl_s::set_glimmer( const glim::rgb_t& bound, uint period_ms ) {
	this->period = period_ms;
	this->fade_time_target = 0;
	this->max = this->value;
	this->min = bound;
	this->update();
}

void lamp_impl_s::set_fader( uint duration_ms ) {
	if( duration_ms > MAX_FADER_TIME_MS ) { // sanity
		duration_ms = MAX_FADER_TIME_MS;
	}
	this->period = duration_ms;
	this->fade_time_target = millis() + duration_ms;
	this->max = this->value;
	if( this->fade_time_target < millis() ) {
		// pfft, it rolled over, give up
		this->set_colour( rgb_t::off );
	}
	DEBUG_SPRINT( "rgb::lamp::set_fader(): duration_ms [%i] fade_time_target [%i]", duration_ms, this->fade_time_target );
}

void lamp_impl_s::update( void ) {
	DEBUG_TRACE( rgb::lamp::update );
	if( 0 != this->period ) {
		// animating...
		if( 0 == this->fade_time_target ) {
			// glimmer
			int saw = 1 + sawtooth_by_range( this->period, 1000 );
#			undef CH
#			define CH(x) uint8_t( int(this->min.x) + ( int(this->max.x) - int(this->min.x) ) * saw / 1000 )
			this->value = rgb_t( CH(r), CH(g), CH(b) );
		}
		else {
			// fader
			uint current = millis();
			if( current >= this->fade_time_target ) {
				DEBUG_SPRINT( "current [%i] fade_time_target [%i]", current, this->fade_time_target );
				this->period = 0;
				this->value = rgb_t::off;
			}
			else {
				// calculate faded values
#				undef CH
#				define CH(x) uint8_t( uint(this->max.x) * current / this->period )
				current = this->fade_time_target - current;
				this->value = rgb_t( CH(r), CH(g), CH(b) );
				//DEBUG_SPRINT( "lamp::update(): fading, current [%i] period [%i] rgb [%i,%i,%i]", current, this->period, this->value.r, this->value.g, this->value.b );
			}
		}
		needs_update = true;
	}
}

} // namespace lamp
/********************************************************/

static void WS2811_writeBytes( const colour_t* array, uint count ) {
	static const uint8_t q[4] = { 0b00110111, 0b00000111, 0b00110100, 0b00000100 };
	uint8_t buf[4]; //  		     11101110    10001110    11101000    10001000
	uint8_t one;    //               00010001    01110001    00010111    01110111
	while( count > 0 ) {
		one = *array++;
		buf[0] = q[(one >> 6) & 3];
		buf[1] = q[(one >> 4) & 3];
		buf[2] = q[(one >> 2) & 3];
		buf[3] = q[(one >> 0) & 3];
		Serial1.write( buf, sizeof(buf) );
		count--;
	}
}

static void WS2811_writeSystemColours( void ) {
	colour_t clrs[6];
	clrs[0] = lamp::local::system.value.r;
	clrs[1] = lamp::local::system.value.g;
	clrs[2] = lamp::local::system.value.b;
	clrs[3] = lamp::local::button.value.r;
	clrs[4] = lamp::local::button.value.g;
	clrs[5] = lamp::local::button.value.b;
	WS2811_writeBytes( clrs, sizeof(clrs) );
}

static bool can_update_system( void ) {
	uint now = millis();
	// more than 10ms.. or the clock wrapped around
	return( now > lamp::last_update_time + 10 || now < lamp::last_update_time );
}

KERNEL_LOOP_DEFINE( rgb ) {
	lamp::local::system.update();
	lamp::local::button.update();
	if( lamp::needs_update && can_update_system() ) {
		//DEBUG_PRINT( "updating system lamp" );
		// need to clear the line to switch outputs
		Serial1.flush();
		digitalWrite( PIN::RGB_ENABLE, HIGH );
		delay( 1 );
		WS2811_writeSystemColours();
		Serial1.flush();
		digitalWrite( PIN::RGB_ENABLE, LOW );
		lamp::needs_update = false;
		lamp::last_update_time = millis();
	}

	return 20;
}


/********************************************************/

void initialize( void ) {
	DEBUG_ENTER( rgb::initialize );

	// why is there a check on the UART number for the "invert" flag..?!
	// -> https://github.com/esp8266/Arduino/blob/6c2ab2508734e88b06e0a7633445998044fd151c/cores/esp8266/uart.cpp#L660
	// hacking around that ... and doing what the hardware is capable of
	int config = SERIAL_6N1 | BIT(UCDTRI) | BIT(UCRTSI) | BIT(UCTXI) | BIT(UCDSRI) | BIT(UCCTSI) | BIT(UCRXI);

	// yikes.. ok.. 3.2MBit/s, 6 data, no parity, 1 stop, inverted output
	// Serial1.begin( 3200000, SERIAL_6N1, SERIAL_TX_ONLY, -1, true );
	DEBUG_PRINT( "Serial1 init" );
	Serial1.begin( 3200000, SerialConfig(config), SERIAL_TX_ONLY );

	// enable the output driver chip in system/button mode
	pinMode( PIN::RGB_ENABLE, OUTPUT );
	digitalWrite( PIN::RGB_ENABLE, HIGH );

	KERNEL_LOOP_ATTACH( rgb, 50 );
}

// main line of RGB(W) lamps, reads all available() bytes from the Stream
void write( Stream& src ) {
	colour_t d[DATA_LOOP_BUFFER];
	for( uint n = src.available() ; n > 0 ; n = src.available() ) {
		n = src.readBytes( d, n > sizeof(d) ? sizeof(d) : n );
		WS2811_writeBytes( d, n );
	}
}


} // namespace rgb
