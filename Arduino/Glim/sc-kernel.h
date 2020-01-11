#include <Arduino.h>
namespace shadowcreatures { namespace arduino { namespace kernel {

// call in main setup
void setup( void );

// call in main loop
void loop( void );

typedef uint cb_t( void );

struct s {
	const char* name;
	cb_t* cb;
	uint next_systime;
	struct s* next_process;
};

void attach( struct s&, uint );

void detach( struct s& );

// non-returning function, use to implement assertions
void halt( void );

// arduino reboots
void reboot( void );

namespace cpu {
	void sample( uint& ms_busy, uint& ms_free );
} // namespace cpu



#define KERNEL_LOOP_DECLARE(name,cb) 		static struct shadowcreatures::arduino::kernel::s name = { #name, cb, 0, NULL }
#define KERNEL_LOOP_DEFINE(prefix)			static uint prefix##_cb( void ); KERNEL_LOOP_DECLARE(prefix##_loop,prefix##_cb); static uint prefix##_cb( void )
#define KERNEL_LOOP_ATTACH(prefix,delay) 	shadowcreatures::arduino::kernel::attach( prefix##_loop, delay )
#define KERNEL_LOOP_DETACH(prefix)			shadowcreatures::arduino::kernel::detach( prefix##_loop )


} } } // namespace shadowcreatures::arduino::kernel
