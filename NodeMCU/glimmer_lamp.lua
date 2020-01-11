
local ls = {
	BTN_PIN = 5,
	GLIMMER_MS = 40,
	BTN_HOLD_REPEAT_MS = 100,
	BTN_HOLD_MAX_MS = 10 * 1000,
	BTN_BOUNCE_EPSILON_MS = 2,
	timer = tmr.create(),
	rgb_minima = nil,
	rgb_maxima = nil,
	rgb_onheld = nil,
	period_ms = 0,
	btn_held_state = 0,
	btn_next_when = 0,
	btn_trigger_waiting = false,
	cb_button_state = nil,
	cb_button_colour = nil
}

local glimmer_btn_check_state -- = function() defined below

local glimmer_sawtooth_scalar = function( min, max )
	local period_us = 1000 * ls.period_ms
	local range = max - min
	-- using modulo sys time so rollover will cause a tear in the pattern every 35 minutes
	local bright = 2 * ( tmr.now() % period_us ) * range / period_us
	if bright > range then
		-- falling edge of sawtooth
		bright = 2 * range - bright
	end
	return bright + min
end

local glimmer_cb_lamp = function()
	-- default is "held" brightness
	local bright = ls.rgb_onheld
	if 0 == ls.btn_held_state then
		-- button not held, so glimmer
		bright = string.char( 
			glimmer_sawtooth_scalar( ls.rgb_minima:byte( 1 ), ls.rgb_maxima:byte( 1 ) ),
			glimmer_sawtooth_scalar( ls.rgb_minima:byte( 2 ), ls.rgb_maxima:byte( 2 ) ),
			glimmer_sawtooth_scalar( ls.rgb_minima:byte( 3 ), ls.rgb_maxima:byte( 3 ) )	
		)
	end
	ls.cb_button_colour( bright )
	glimmer_btn_check_state()
end

local glimmer_resume = function()
	glimmer_cb_lamp()
	if ls.period_ms > 0 then
		ls.timer:register( ls.GLIMMER_MS, tmr.ALARM_AUTO, glimmer_cb_lamp )
		ls.timer:start()
	end
end

-- local, but predeclared now
glimmer_btn_check_state = function()
	if 0 == gpio.read( ls.BTN_PIN ) then
		if 0 == ls.btn_held_state then
			-- button was released, and now held
			ls.btn_held_state = 1
			glimmer_cb_lamp()
			ls.cb_button_state( "down" )
			ls.timer:register( ls.BTN_HOLD_REPEAT_MS, tmr.ALARM_AUTO, function()
				ls.btn_held_state = ls.btn_held_state + 1
				ls.cb_button_state( "held" )
				glimmer_btn_check_state()
			end )
			ls.timer:start()
		end
	elseif 0 < ls.btn_held_state then
		-- button was held, and now released
		ls.btn_held_state = 0
		ls.cb_button_state( "up" )
		ls.timer:unregister()
		glimmer_resume()
	end
	ls.btn_trigger_waiting = false
end

return {
	initialize = function( on_button_state_changed, on_button_colour_changed )
		ls.cb_button_state = on_button_state_changed
		ls.cb_button_colour = on_button_colour_changed
	end,

	clear = function( rgb_value )
		ls.timer:unregister()
		if nil == rgb_value then
			rgb_value = string.char( 0, 0, 0 )
		end
		ls.cb_button_colour( rgb_value )
		gpio.mode( ls.BTN_PIN, gpio.INPUT, gpio.PULLUP )
	end,

	set = function( rgb_minima, rgb_maxima, period_ms, rgb_onheld )
		ls.rgb_minima = rgb_minima
		ls.rgb_maxima = rgb_maxima
		ls.rgb_onheld = rgb_onheld
		ls.period_ms = period_ms
		gpio.mode( ls.BTN_PIN, gpio.INT, gpio.PULLUP )
		gpio.trig( ls.BTN_PIN, "both", function( level, when )
			if false == ls.btn_trigger_waiting then
				ls.btn_trigger_waiting = true
				node.task.post( node.task.MEDIUM_PRIORITY, glimmer_btn_check_state )
			end
		end )
		glimmer_resume();
	end,
}
