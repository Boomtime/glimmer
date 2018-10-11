print( ">> glimmer_lamp.lua" )

local PIN = 5

gpio.mode( PIN, gpio.INPUT, gpio.PULLUP )


local glimmer_lamp = function()

	local PERIOD_US = 1000 * 1000
	local MIN = 0x20
	local MAX = 0x90
	local RANGE = MAX - MIN

	local bright = 0xb0

	if 1 == gpio.read( PIN ) then
		bright = 2 * ( tmr.now() % PERIOD_US ) * RANGE / PERIOD_US
		if bright > RANGE then
			bright = 2 * RANGE - bright
		end
		bright = bright + MIN
	end
	
	-- should be sawtooth in RANGE now
	--print( "bright: "..bright )

	if bright < 0x100 then
		rgb_system_set( string.char( 0, 0, bright ) )
	end
end

local glimmer_timer = tmr.create()
glimmer_timer:register( 25, tmr.ALARM_AUTO, glimmer_lamp )
glimmer_timer:start()
