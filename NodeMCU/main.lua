print( ">> main.lua" )

dofile( "rgb.lua" )
dofile( "network.lua" )
dofile( "wifi.lua" )


local PIN_SYSMODE = 5

local on_button_pressed = function()

end

gpio.mode( PIN_SYSMODE, gpio.INT, gpio.PULLUP )
gpio.trig( PIN_SYSMODE, "low", on_button_pressed )
