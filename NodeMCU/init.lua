print( ">> init.lua" )

-- deadman pin is zero on early versions.. until I read the spec for the ESP8266 where I discovered that should never have worked
local PIN_DEADMAN = 7

-- hardware info
--for k,v in pairs( node.info( "hw" ) ) do
--	print( k, v )
--end

function trim( s )
	-- from PiL2 20.4
	return (s:gsub("^%s*(.-)%s*$", "%1"))
end

-- config loading
if not file.open( "config", "r" ) then
	print( "*** config not found" )
	return
end
cfg = {}
cfg.hostname = trim( file.readline() )
print( "hostname: "..cfg.hostname )
cfg.wifi = {}
cfg.wifi.ssid = trim( file.readline() )
cfg.wifi.pwd = trim( file.readline() )
file.close()

-- check the deadman pin is pulled low (dead man switch)
gpio.mode( PIN_DEADMAN, gpio.INPUT, gpio.PULLUP )
if 0 ~= gpio.read( PIN_DEADMAN ) then
	print( "*** deadman" )
	return
end

-- execute main sequence
print( ">> main.lua @ "..node.heap() )
local main,err = loadfile( "main.lua" )
if main then
	print( "executing main sequence >>>" )
	main()
	print( "<<< finished main sequence" )
else
	print( "*** failed to load main.lua: "..err )
end
