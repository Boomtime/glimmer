-- glimswarm v3
-- ws2812 (single wire protocol)

local const = protect( {
	DRIVER_PIN = 1,
} )

local self = {
	system_lamp = string.char( 0, 0, 0 ),
	switch_lamp = string.char( 0, 0, 0 ),
	update_in_progress = false
}

gpio.mode( const.DRIVER_PIN, gpio.OUTPUT )
gpio.write( const.DRIVER_PIN, gpio.LOW )
ws2812.init( ws2812.MODE_SINGLE )

local rgb_update_system_vector = function()
	gpio.write( const.DRIVER_PIN, gpio.HIGH )
	tmr.delay( 20 )
	ws2812.write( self.system_lamp .. self.switch_lamp, nil )
	tmr.delay( 10 )
	gpio.write( const.DRIVER_PIN, gpio.LOW )
	self.update_in_progress = false
end

local rgb_queue_update = function()
	if true ~= self.update_in_progress then
		self.update_in_progress = true
		node.task.post( node.task.LOW_PRIORITY, rgb_update_system_vector )
	end
end

return {
	set = function( vector1 )
		ws2812.write( vector1, nil )
	end,

	system_set = function( rgb_vector )
		assert( 3 == rgb_vector:len() )
		self.system_lamp = rgb_vector
		rgb_queue_update()
	end,

	switch_set = function( rgb_vector )
		assert( 3 == rgb_vector:len() )
		self.switch_lamp = rgb_vector
		rgb_queue_update()
	end
}
