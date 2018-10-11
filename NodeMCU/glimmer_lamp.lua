print( ">> glimmer_lamp.lua" )

local ls = {
	PIN = 5,
	GLIMMER_MS = 40,
	BTN_HOLD_REPEAT_MS = 100,
	BTN_HOLD_MAX_MS = 10 * 1000,
	timer = tmr.create(),
	rgb_minima = 0,
	rgb_maxima = 0,
	rgb_onheld = 0,
	period_ms = 0,
	btn_held_state = 0
}

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

	rgb_switch_set( bright )
end

local glimmer_resume = function()
	glimmer_cb_lamp()
	if ls.period_ms > 0 then
		ls.timer:register( ls.GLIMMER_MS, tmr.ALARM_AUTO, glimmer_cb_lamp )
		ls.timer:start()
	end
end

local glimmer_btn_trigger_up = function()
	if 0 < ls.btn_held_state then
		-- button was held, and now released
		ls.btn_held_state = 0
		net_send_button_state( "up" )
		ls.timer:unregister()
		glimmer_resume()
	end
end

local glimmer_cb_btn_held = function()
	ls.btn_held_state = ls.btn_held_state + 1
	-- check if button has been held too long ("cat on keyboard" detect)
	if ls.btn_held_state * ls.BTN_HOLD_REPEAT_MS < ls.BTN_HOLD_MAX_MS then
		net_send_button_state( "held" )
	else
		glimmer_btn_trigger_up();
	end
end

local glimmer_btn_trigger_down = function()
	if 0 == ls.btn_held_state then
		-- button was released, and now held
		ls.btn_held_state = 1
		net_send_button_state( "down" )
		ls.timer:register( ls.BTN_HOLD_REPEAT_MS, tmr.ALARM_AUTO, glimmer_cb_btn_held )
		ls.timer:start()
	end
end

local glimmer_on_button_changed = function( level, when, eventcount )
	if gpio.HIGH == level then
		-- button is released
		glimmer_btn_trigger_up()
	else
		-- button is pressed
		glimmer_btn_trigger_down()
	end
end

glimmer_clear = function( rgb_value )
	ls.timer:unregister()
	if nil ~= rgb_value then
		rgb_value = string.char( 0, 0, 0 )
	end
	rgb_switch_set( rgb_value )
	gpio.mode( ls.PIN, gpio.INPUT, gpio.PULLUP )
end

glimmer_set = function( rgb_minima, rgb_maxima, period_ms, rgb_onheld )
	ls.rgb_minima = rgb_minima
	ls.rgb_maxima = rgb_maxima
	ls.rgb_onheld = rgb_onheld
	ls.period_ms = period_ms
	gpio.mode( ls.PIN, gpio.INT, gpio.PULLUP )
	gpio.trig( ls.PIN, "both", glimmer_on_button_changed )
	glimmer_resume();
end
