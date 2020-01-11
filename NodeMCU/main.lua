function protect( tbl )
	return setmetatable( {}, { 
		__index = tbl,
		__newindex = function( t, k, v )
			error( "attempting to change constant "..tostring( k ), 2 )
		end
	} )
end

local dofile_measure = function( filename )
	local startheap = node.heap()
	local ret = dofile( filename )
	print( ">> "..filename.." @ "..startheap.." used "..(startheap-node.heap()) )
	return ret
end

local glimmer = dofile_measure( "glimmer_lamp.lua" )
local net = dofile_measure( "network.lua" )
local rgb = dofile_measure( "rgb.lua" )

glimmer.initialize( net.send_button_state, rgb.switch_set )

local cb_net_state_changed = function( state )
	local sysled = {
		["connecting"] 	= string.char( 0, 0, 0x80 ),
		["connected"] 	= string.char( 0, 0x80, 0 ),
		["established"] = string.char( 0, 0, 0 ),
		["disconnected"] = string.char( 0x80, 0, 0 )
	}
	rgb.system_set( sysled[state] );
end

print( ">> net.initialize @ "..node.heap() )
net.initialize( cb_net_state_changed, rgb.set, function( t ) glimmer.set( t.min, t.max, t.period, t.onheld ) end )

print( " << net.initialize @ "..node.heap() )
