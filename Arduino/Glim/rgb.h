#ifndef RGB_H
#define RGB_H
#include <Arduino.h>
#include "glim.ino.h"
namespace rgb {

// initialize RGB subsystem, call from Arduino setup
void initialize( void );

// main line of RGB(W) lamps, reads all available() bytes from the Stream
void write( Stream& src );

struct lamp_api_s {
	// set the lamp RGB value solid (cancels glimmer and fader)
	virtual void set_colour( const glim::rgb_t& ) = 0;

	// glimmer between last set_colour and bound on period ms (cancels fader)
	virtual void set_glimmer( const glim::rgb_t& bound, uint period_ms ) = 0;

	// fade out last set_colour over duration ms (cancels glimmer)
	virtual void set_fader( uint duration_ms ) = 0;
};

namespace lamp {
	extern struct lamp_api_s& system;
	extern struct lamp_api_s& button;
}

} // namespace rgb
#endif // RGB_H
