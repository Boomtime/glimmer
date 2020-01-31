#ifndef GLIM_H
#define GLIM_H
#include <Arduino.h>
#include "sc-debug.h"
#include "sc-kernel.h"

namespace shadowcreatures { namespace arduino {

// these are the GPIO mappings for the NodeMCU and D1 mini
namespace NODEPIN { enum {
	D0 = 16,
	D1 = 5,
	D2 = 4,
	D3 = 0,
	D4 = 2,
	D5 = 14,
	D6 = 12,
	D7 = 13,
	D8 = 15,
}; }

} } // namespace shadowcreatures::arduino

namespace glim {

typedef uint8_t colour_t;

typedef struct rgb_s {
	colour_t r;
	colour_t g;
	colour_t b;
	inline rgb_s() : r(0), g(0), b(0) { }
	inline rgb_s( colour_t rc, colour_t gc, colour_t bc ) : r(rc), g(gc), b(bc) { }
	static const struct rgb_s off;
} rgb_t;

typedef struct rgbw_s : rgb_s {
	colour_t w;
} rgbw_t;


} // namespace glim
#endif // GLIM_H
