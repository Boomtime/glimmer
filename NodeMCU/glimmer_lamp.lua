print( ">> glimmer_lamp.lua" )

local dbg_print = function( msg )
	print( "glimmer_lamp: "..msg )
end

local ls = {
	BTN_PIN = 5,
	GLIMMER_MS = 40,
	BTN_HOLD_REPEAT_MS = 100,
	BTN_HOLD_MAX_MS = 10 * 1000,
	BTN_BOUNCE_EPSILON_MS = 2,
	timer = tmr.create(),
	rgb_minima = 0,
	rgb_maxima = 0,
	rgb_onheld = 0,
	period_ms = 0,
	btn_held_state = 0,
	btn_next_when = 0,
	btn_trigger_waiting = false
}

local glimmer_btn_check_state -- = function()

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
	glimmer_btn_check_state()
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
	--if ls.btn_held_state * ls.BTN_HOLD_REPEAT_MS < ls.BTN_HOLD_MAX_MS then
		net_send_button_state( "held" )
		glimmer_btn_check_state()
	--else
	--	dbg_print( "glimmer_cb_btn_held: cat on keyboard" )
	--	glimmer_btn_trigger_up();
	--end
end

local glimmer_btn_trigger_down = function()
	if 0 == ls.btn_held_state then
		-- button was released, and now held
		ls.btn_held_state = 1
		glimmer_cb_lamp()
		net_send_button_state( "down" )
		ls.timer:register( ls.BTN_HOLD_REPEAT_MS, tmr.ALARM_AUTO, glimmer_cb_btn_held )
		ls.timer:start()
	else
		--[[
		-- rising edge trigger might not have reached high enough to count as a HIGH level yet, re-sample it
		if 0 ~= gpio.read( ls.BTN_PIN ) then
			dbg_print( "glimmer_btn_trigger_down converting to up due to pin state" )
			glimmer_btn_trigger_up()
		end
		]]
	end
end

-- local, but predeclared now
glimmer_btn_check_state = function()
	if 0 == gpio.read( ls.BTN_PIN ) then
		glimmer_btn_trigger_down()
	else
		glimmer_btn_trigger_up()
	end
	ls.btn_trigger_waiting = false
end

local glimmer_on_button_changed = function( level, when )
	dbg_print( "glimmer_on_button_changed: level "..level.." when "..when )

	if false == ls.btn_trigger_waiting then
		ls.btn_trigger_waiting = true
		node.task.post( node.task.MEDIUM_PRIORITY, glimmer_btn_check_state )
	end
--[[
	if when < ls.btn_next_when then
		dbg_print( "... de-bouncing" )
		return
	end
	ls.btn_next_when = when + ( ls.BTN_BOUNCE_EPSILON_MS * 1000 )
	if gpio.HIGH == level then
		-- button is released
		glimmer_btn_trigger_up()
	else
		-- button is pressed
		glimmer_btn_trigger_down()
	end
]]
end

glimmer_clear = function( rgb_value )
	dbg_print( "glimmer_clear" )
	ls.timer:unregister()
	if nil ~= rgb_value then
		rgb_value = string.char( 0, 0, 0 )
	end
	rgb_switch_set( rgb_value )
	gpio.mode( ls.BTN_PIN, gpio.INPUT, gpio.PULLUP )
end

glimmer_set = function( rgb_minima, rgb_maxima, period_ms, rgb_onheld )
	dbg_print( "glimmer_set" )
	ls.rgb_minima = rgb_minima
	ls.rgb_maxima = rgb_maxima
	ls.rgb_onheld = rgb_onheld
	ls.period_ms = period_ms
	gpio.mode( ls.BTN_PIN, gpio.INT, gpio.PULLUP )
	gpio.trig( ls.BTN_PIN, "both", glimmer_on_button_changed )
	glimmer_resume();
end
