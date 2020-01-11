#include "sc-kernel.h"
#include "sc-debug.h"
namespace shadowcreatures { namespace arduino { namespace kernel {

const uint MAXIMUM_FRAME_DELAY_MS = 50;

namespace cpu {
	static struct {
		uint last_mark;
		uint samples;
		uint busy_ms;
		uint free_ms;
	} local = { 0, 0, 0, 0 };
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
		now = millis();
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
	//cpu::local.last_mark = millis();
}

void loop( void ) {
	DEBUG_TRACE( kernel::loop );

	uint delay_ms = execute();

	// record some CPU stats
	cpu::local.busy_ms += millis() - cpu::local.last_mark;
	cpu::local.free_ms += delay_ms;
	cpu::local.samples ++;

	if( cpu::local.samples > 0x80 ) {
		cpu::local.samples >>= 1;
		cpu::local.busy_ms >>= 1;
		cpu::local.free_ms >>= 1;
	}

	if( delay_ms ) {
		delay( delay_ms );
	}

	// everything from here until the end of the next loop() is counted as "busy"...
	// @todo, could record the missing time as "steal" instead
	cpu::local.last_mark = millis();
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
void sample( uint& ms_busy, uint& ms_free ) {
	ms_busy = local.busy_ms;
	ms_free = local.free_ms;
}
} // namespace cpu



} } } // namespace shadowcreatures::arduino::kernel
