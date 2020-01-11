#include "sc-debug.h"
#ifdef DEBUG_BUILD
#include <Arduino.h>
#include <stdarg.h>

/***************************************************
va, builds strings from varargs
***************************************************/
char* va( const char* fmt, ... ) {
#	define VA_BUFFER_COUNT  	2
#	define VA_BUFFER_LENGTH 	0x100

	static char va_buffers[VA_BUFFER_COUNT][VA_BUFFER_LENGTH];
	static int va_count = 0;

	/* rotate the buffers */
	char* buf = va_buffers[va_count++ % VA_BUFFER_COUNT];
	va_list args;

	va_start( args, fmt );
	vsnprintf( buf, VA_BUFFER_LENGTH, fmt, args );
	buf[VA_BUFFER_LENGTH-1] = 0; /* vsnprintf merely stops at the limit, it does not terminate the string if reached */
	va_end( args );

	return buf;
}

/***************************************************
DEBUG_PRINT - function body for DEBUG_BUILD
***************************************************/

static int debug_print_depth = 0;
int debug_stack = 0;
static int debug_initialized = 0;
static DEBUG_trace* debug_trace_head = 0;

void DEBUG_INITIALISE( int* p_StackReference ) {
	if( !debug_stack || debug_stack < (int)p_StackReference ) {
		debug_stack = (int)p_StackReference;
	}
	if( !debug_initialized ) {
		debug_initialized = 1;
		if( !Serial ) {
			Serial.begin( 115200 );
			delay( 10 );
			Serial.println( "" );
		}
	}
}

static void DEBUG_PRINT_RAW( const char* msg ) {
	Serial.println( msg );
}

void DEBUG_kill_message( const char* kill_message ) {
	/* send the message to the persistent log (wherever that is) */
	DEBUG_PRINT( "---------------------------------------------------------------------" );
	DEBUG_PRINT( kill_message );
	DEBUG_PRINT( "---------------------------------------------------------------------" );
	DEBUG_PRINT_STACKTRACE();

	ESP.restart();
}

void DEBUG_PRINT( const char* msg ) {
	char buffer[0x180];
	char* pnt = buffer;
	int i;

	DEBUG_INITIALISE( &i );

	i = debug_print_depth;
	while( i-- > 0 ) {
		strcpy( pnt, "|  " );
		pnt += 3;
	}

	// append function name if the current trace didn't print a wrapper
	/* this turns out to be waaaay annoying
	if( debug_trace_head && !debug_trace_head->printtrace() ) {
		strcpy( pnt, debug_trace_head->name() );
		pnt += strlen( debug_trace_head->name() );
		strcpy( pnt, ": " );
		pnt += 2;
	}
	*/
	strcpy( pnt, msg );

	DEBUG_PRINT_RAW( buffer );
}

void DEBUG_BINARY( void* p_Data, unsigned long p_Length ) {
	unsigned long j;
	unsigned char* data = (unsigned char*)p_Data;
	char buffer[0x80];
	char* hex = buffer;
#ifdef DEBUG_BINARY_C_COMPATIBLE
	char* chr = buffer + 62;
#else
	char* chr = buffer + 32;
#endif

	if( !p_Data || 0 >= p_Length )
		return;

	memset( buffer, ' ', sizeof(buffer) );
	buffer[sizeof(buffer)-1] = 0;

	for( j=0 ; j<p_Length ; j++ ) {
#ifdef DEBUG_BINARY_C_COMPATIBLE
		sprintf( hex, "0x%02X, ", *data );
		hex[6] = ' ';
#else
		sprintf( hex, "%02X ", *data );
		hex[3] = ' ';
#endif

		switch( *data ) {
			case 0:
			case '\a':
			case '\b':
			case '\n':
			case '\r':
			case '\t':
				*chr = '.';
				break;
			default:
				*chr = *data;
				break;
		}

#ifdef DEBUG_BINARY_C_COMPATIBLE
		hex += 6;
#else
		hex += 3;
#endif
		chr ++;
		data++;

		if( 9 == (j%10) ) {
			*hex = ' ';
			*chr = 0;
			hex = buffer;
#ifdef DEBUG_BINARY_C_COMPATIBLE
			chr = buffer + 62;
#else
			chr = buffer + 32;
#endif
			DEBUG_PRINT( buffer );

			memset( buffer, ' ', sizeof(buffer) );
			buffer[sizeof(buffer)-1] = 0;
		}
	}

	*chr = 0;

	DEBUG_PRINT( buffer );
}

DEBUG_trace::DEBUG_trace( const debug_trace_func_desc_t& func, bool printtrace ) {
	DEBUG_INITIALISE( (int*)(&this->mPrintTrace) );
	this->mFunc = &func;
	this->mPrintTrace = printtrace;
	if( this->mPrintTrace ) {
		DEBUG_SPRINT( "/======================== %s =====[%6i]=====", this->name(), this->stackdepth() );
		::debug_print_depth++;
	}
	this->mPrev = debug_trace_head;
	debug_trace_head = this;
}
DEBUG_trace::~DEBUG_trace() {
	if( this->mPrintTrace ) {
		::debug_print_depth--;
		DEBUG_SPRINT( "\\------------------------ %s ------------------", this->name() );
	}
	debug_trace_head = this->mPrev;
	this->mPrev = 0;
}

void DEBUG_PRINT_STACKTRACE( void ) {
	const DEBUG_trace* t = debug_trace_head;
	int k = 0;
	while( t ) {
		DEBUG_SPRINT( "#%i %s(%i) - %s:%i", k, t->name(), t->stackdepth(), t->file(), t->line() );
		t = t->prev();
		k ++;
	}
}


#if 0
#undef DEBUG_TRACE
#undef DEBUG_ENTER
#undef DEBUG_ASSERT
#undef DEBUG_SLEEP

void DEBUG_TRACE( void* class_function ) { class_function=class_function; }
void DEBUG_ENTER( void* class_function ) { class_function=class_function; }
void DEBUG_ASSERT( int if_not_then_print_sys_error ) { if_not_then_print_sys_error=if_not_then_print_sys_error; }
void DEBUG_SLEEP( int milliseconds ) { milliseconds=milliseconds; }
#endif /* 0 */

#endif /* DEBUG_BUILD */



/**************************************************/
/* EOF */
