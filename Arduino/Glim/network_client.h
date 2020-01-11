#include <Arduino.h>
#include "glim.ino.h"
namespace network { namespace client {

void initialize( void );

typedef void net_status_connected_cb( bool );
void on_net_status_changed( net_status_connected_cb* );

typedef void msg_rgb_cb( Stream& );
void on_msg_rgb( msg_rgb_cb* );

typedef void msg_btnc_cb( glim::rgb_t min, glim::rgb_t max, short period, glim::rgb_t held );
void on_msg_btnc( msg_btnc_cb* );

namespace BUTTON { typedef enum {
	DOWN = 1,
	HELD = 2,
	UP = 3
} state; }

void send_button_state( BUTTON::state );

bool is_connected( int* dbm = 0 );

} } // namespace network::client
