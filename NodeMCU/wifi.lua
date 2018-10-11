print( ">> wifi.lua" )
-------
-- WiFi

local wifi_print = function( msg )
	print( "wifi: "..msg )
end

local SYSLED_CONNECTING = string.char( 0, 0, 0x80 )
local SYSLED_CONNECTED = string.char( 0, 0x80, 0 )
local SYSLED_ESTABLISHED = string.char( 0, 0, 0 )
local SYSLED_DISCONNECTED = string.char( 0x80, 0, 0 )


local wifi_connect_event = function( args )
	wifi_print( args.SSID.." connected" )
	rgb_system_set( SYSLED_CONNECTED )
end

local wifi_disconnect_event = function( args )
	wifi_print( args.SSID.." disconnected" )
	rgb_system_set( SYSLED_DISCONNECTED )
	net_on_disconnect()
end

local wifi_got_ip_event = function( args )
	wifi_print( "ip address "..args.IP )
	rgb_system_set( SYSLED_ESTABLISHED )
	net_on_connect()
end

-- Register WiFi Station event callbacks
wifi.eventmon.register( wifi.eventmon.STA_CONNECTED, wifi_connect_event )
wifi.eventmon.register( wifi.eventmon.STA_GOT_IP, wifi_got_ip_event )
wifi.eventmon.register( wifi.eventmon.STA_DISCONNECTED, wifi_disconnect_event )

wifi_print( "connecting to "..cfg.wifi.ssid )
wifi.setmode( wifi.STATION )
wifi.sta.sethostname( cfg.hostname )
wifi.sta.config( cfg.wifi )

rgb_system_set( SYSLED_CONNECTING )
