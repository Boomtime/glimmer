#ifndef GLIM_CONFIG_H
#define GLIM_CONFIG_H
#include <Arduino.h>
namespace config {

bool load( void );

bool save( bool force = false );

struct key_s {
	virtual operator String() const = 0;
	virtual const String& operator = ( const String& ) = 0;
};

namespace key {
	extern struct key_s& device_name;
	extern struct key_s& wifi_ssid;
	extern struct key_s& wifi_psk;
}

} // namespace config
#endif // GLIM_CONFIG_H
