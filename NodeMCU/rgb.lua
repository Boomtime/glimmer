print( ">> rgb.lua" )

-- glimswarm v3
-- ws2812 (single wire protocol) dual outputs
-- note the system lamps need to be set once or rgb_set won't function correctly

local rgb_print = function( msg )
	print( "rgb: "..msg )
end

local rgb = {
	DRIVER_PIN = 1,
	WS2812_MODE = ws2812.MODE_SINGLE,
	system_lamp = string.char( 0, 0, 0 ),
	switch_lamp = string.char( 0, 0, 0 ),
}

gpio.mode( rgb.DRIVER_PIN, gpio.OUTPUT )
gpio.write( rgb.DRIVER_PIN, gpio.HIGH )
ws2812.init( rgb.WS2812_MODE )

-- either vector can be nil
rgb_set = function( vector1, vector2 )
	ws2812.write( vector1, vector2 )
end

local rgb_update_system_vector = function()
	--rgb_print( "rgb_update_system_vector" )
	gpio.write( rgb.DRIVER_PIN, gpio.HIGH )
	tmr.delay( 20 )
	ws2812.write( rgb.system_lamp .. rgb.switch_lamp, nil )
	tmr.delay( 10 )
	gpio.write( rgb.DRIVER_PIN, gpio.LOW )
end

rgb_system_set = function( rgb_vector )
	if 3 == rgb_vector:len() then
		rgb.system_lamp = rgb_vector
		node.task.post( node.task.LOW_PRIORITY, rgb_update_system_vector )
	else
		rgb_print( "rgb_system_set(): incorrect string length" )
	end
end

rgb_switch_set = function( rgb_vector )
	if 3 == rgb_vector:len() then
		rgb.switch_lamp = rgb_vector
		node.task.post( node.task.LOW_PRIORITY, rgb_update_system_vector )
	else
		rgb_print( "rgb_switch_set(): incorrect string length" )
	end
end
