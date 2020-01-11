#ifndef ZDEBUG_H
#define ZDEBUG_H
/***************************************************
definitions
***************************************************/

#define DEBUG_BUILD


/**************************************************/

#ifdef DEBUG_BUILD
char* va( const char* fmt, ... );
#endif

/**************************************************/

void DEBUG_TRACE( void* class_function );
void DEBUG_ENTER( void* class_function );
void DEBUG_PRINT( const char* msg );
void DEBUG_SPRINT( const char* fmt, ... );
void DEBUG_ASSERT( int if_not_then_print_sys_error );
void DEBUG_SLEEP( int milliseconds );
void DEBUG_BINARY( void*, unsigned int );
void DEBUG_PRINT_STACKTRACE( void );

#ifdef DEBUG_BUILD

	extern int debug_stack;

#	define DEBUG_SPRINT(fmt,...) \
		DEBUG_PRINT(va(fmt,__VA_ARGS__))

	void DEBUG_assert_win32( int if_not, const char* statement, const char* file, int line );
	void DEBUG_INITIALISE( int* p_StackReference );
#	define DEBUG_SLEEP( x ) delay( x )
	void DEBUG_kill_message( const char* kill_message );

#	define DEBUG_ASSERT_MSG( x, msg ) \
		if( !( x ) ) \
			DEBUG_kill_message(va( "Assertion failure: %s (%i) %s", DEBUG_FILE, DEBUG_LINE, msg ))

#	define DEBUG_ASSERT( x )	DEBUG_ASSERT_MSG( x, #x )

#	define DEBUG_ASSERT_WIN32( x ) \
		DEBUG_assert_win32( (x), #x, DEBUG_FILE, DEBUG_LINE )

	typedef struct debug_trace_func_desc_s {
		const char* mName;
		const char* mFile;
		int mLine;
	} debug_trace_func_desc_t;

	class DEBUG_trace {
	private:
		const debug_trace_func_desc_t* mFunc;
		DEBUG_trace* mPrev;
		bool mPrintTrace;
	public:
		DEBUG_trace( const debug_trace_func_desc_t&, bool printtrace = true );
		~DEBUG_trace();
		inline const DEBUG_trace* 	prev( void ) const 	{ return this->mPrev; }
		inline const char* 			name( void ) const 	{ return this->mFunc->mName; }
		inline const char*			file( void ) const 	{ return this->mFunc->mFile; }
		inline int 					line( void ) const 	{ return this->mFunc->mLine; }
		inline int 					stackdepth( void ) const { return debug_stack - (int)(&this->mPrintTrace); }
		inline bool 				printtrace( void ) const { return this->mPrintTrace; }
	};

#	define DEBUG_FUNC_DESC( f ) 	static const debug_trace_func_desc_t debug_func_desc = { #f, DEBUG_FILE, DEBUG_LINE }
#	define DEBUG_ENTER( funcname )	DEBUG_FUNC_DESC( funcname ); DEBUG_trace debug_trace_enter( debug_func_desc )
#	define DEBUG_TRACE( funcname )	DEBUG_FUNC_DESC( funcname ); DEBUG_trace debug_trace_enter( debug_func_desc, false )

#	define DEBUG_FILE				__FILE__
#	define DEBUG_LINE				__LINE__

#else /* DEBUG_BUILD */

#	define DEBUG_PRINT(x);
#	define DEBUG_SPRINT(x,...);
#	define DEBUG_BINARY(d,c);
#	define DEBUG_TRACE(x);
#	define DEBUG_ENTER(x);
#	define DEBUG_PRINT_STACKTRACE();
#	define DEBUG_ASSERT(x);
#	define DEBUG_SLEEP(x);

#endif /* !DEBUG_BUILD */

/* usage:
 *   DEBUG_ENTER( function_name );
 *   DEBUG_PRINT( "doing..." );
 *   DEBUG_SPRINT( "doing %i...", integer );
 *   DEBUG_EXIT();                                <-- superfluous in C++
 */

/**************************************************/
#endif /* Z_DEBUG_H */
