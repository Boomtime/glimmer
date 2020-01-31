#include "sc-kernel.h"
#include "sc-debug.h"
namespace shadowcreatures { namespace arduino { namespace kernel {

const uint MAXIMUM_FRAME_DELAY_MS = 50;

namespace cpu {
	static struct {
		uint last_mark;
		uint samples;
		uint user_us;
		uint free_us;
		uint system_us;
	} local = { 0, 0, 0, 0, 0 };

	// returns the number of microseconds since this function was last called
	static uint mark_micros( void ) {
		uint now = micros();
		uint ret;
		// deal with rollover
		if( now < local.last_mark ) {
			DEBUG_PRINT( "cpu::mark_micros(): rollover" );
			ret = ( ~0 - local.last_mark ) + now;
		}
		else {
			ret = now - local.last_mark;
		}
		local.last_mark = now;
		return ret;
	}

} // namespace cpu

static struct s* head = NULL;

// execute the loops that have expired, returns the delay desired until next loop
static uint execute( void ) {
	DEBUG_TRACE( kernel::execute );
	uint now = millis();
	uint earliest = now + MAXIMUM_FRAME_DELAY_MS;
	struct s** cur = &head;
	struct s* ref;
	int delay;
	while( ref = *cur ) {
		now = millis(); // @todo: fix 49 day bug
		if( ref->next_systime <= now ) {
			//DEBUG_SPRINT( "executing cb [%s]", ref->name );
			ref->next_systime = now + ref->cb();
		}
		if( earliest > ref->next_systime ) {
			earliest = ref->next_systime;
		}
		cur = &(ref->next_process);
	}
	if( earliest < now ) {
		return 0;
	}
	return earliest - now;
}

void attach( struct s& proc, uint delay ) {
	DEBUG_ENTER( kernel::attach );
	proc.next_systime = millis() + delay;
	struct s* next = head;
	while( next ) {
		if( next == &proc ) {
			DEBUG_PRINT( "already attached" );
			return;
		}
		next = next->next_process;
	}
	DEBUG_PRINT( "attaching at the head" );
	proc.next_process = head;
	head = &proc;
}

void detach( struct s& proc ) {
	DEBUG_ENTER( kernel::detach );
	struct s** cur = &head;
	while( *cur ) {
		if( *cur == &proc ) {
			DEBUG_PRINT( "detaching" );
			*cur = (*cur)->next_process;
			break;
		}
		cur = &((*cur)->next_process);
	}
}

void setup( void ) {
	// nothing to do
}

void loop( void ) {
	DEBUG_TRACE( kernel::loop );

	cpu::local.system_us += cpu::mark_micros();
	cpu::local.samples ++;

	uint delay_ms = execute();

	if( cpu::local.samples > 0x80 ) {
		cpu::local.samples >>= 1;
		cpu::local.user_us >>= 1;
		cpu::local.free_us >>= 1;
		cpu::local.system_us >>= 1;
	}

	cpu::local.user_us += cpu::mark_micros();

	if( delay_ms ) {
		delay( delay_ms );
	}

	cpu::local.free_us += cpu::mark_micros();
}

// non-returning function, use to implement assertions
void halt( void ) {
	DEBUG_ENTER( kernel::halt );
	DEBUG_PRINT_STACKTRACE();
	noInterrupts();
	while( true ) {
		delay( 1 );
	}
}

// arduino reboots
void reboot( void ) {
	DEBUG_ENTER( kernel::reboot );
	DEBUG_PRINT_STACKTRACE();
	// pfft, arduino has no built-in way to reboot.. well.. except for being C of course
	//*((uint*)1) = 0;
	// ESP8266 gives a helper
	ESP.restart();
}

namespace cpu {
stats_t sample( uint base_scale ) {
	stats_t ret = { 0, 0, 0 };
	uint total_us = local.user_us + local.free_us + local.system_us;
	if( 0 < total_us ) {
		ret.user = ( local.user_us * base_scale ) / total_us;
		ret.free = ( local.free_us * base_scale ) / total_us;
		ret.system = ( local.system_us * base_scale ) / total_us;
	}
	return ret;
}
} // namespace cpu



} } } // namespace shadowcreatures::arduino::kernel
