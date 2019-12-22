print( ">> network.lua" )

local net_print = function( msg )
	print( "network: "..msg )
end

local glim = {
	PORT = 1998,
	ANNOUNCE = true,

	MSG_RGB1 = 1,
	MSG_RGB2 = 2,
	MSG_PING = 3,
	MSG_PONG = 4,
	MSG_BTNS = 5,
	MSG_BTNC = 6,

	HW_SERVER = 1,
	HW_GLIM_V2 = 2,
	HW_GLIM_V3 = 3,

	socket = nil,

	server_ip = nil,
	server_port = nil
}

local net_pull_byte = function( intr )
	local b = intr % 0x100
	return b, ( intr - b ) / 0x100
end

local net_pack_integer = function( intr )
	local b1, b2, b3, b4
	b1, intr = net_pull_byte( intr )
	b2, intr = net_pull_byte( intr )
	b3, intr = net_pull_byte( intr )
	b4, intr = net_pull_byte( intr )
	return string.char( b4, b3, b2, b1 )
end

local net_send_ident_message = function( msg, ip, port )
	-- sending ping or pong with our type and identity
	glim.socket:send( port, ip, string.char( msg, glim.HW_GLIM_V3, cfg.hostname:len() )..cfg.hostname..net_pack_integer( tmr.time() ) )
end

local net_recv_rgb1 = function( args )
	--net_print( "net_recv_rgb1" )
	rgb_set( args.payload:sub( 2 ), nil )
end

local net_recv_rgb2 = function( args )
	net_print( "net_recv_rgb2" )
	rgb_set( nil, args.payload:sub( 2 ) )
end

local net_extract_pascal_string = function( payload, index )
	local count = payload:byte( index )
	local start = index + 1
	return payload:sub( start, start + count )
end

local net_extract_short = function( payload, index )
	local r = 0
	r = r + ( payload:byte( index ) * 0x100 )
	r = r + payload:byte( index + 1 )
	return r
end

local net_extract_int = function( payload, index )
	local r = 0
	r = r + ( payload:byte( index ) * 0x1000000 )
	r = r + ( payload:byte( index + 1 ) * 0x10000 )
	r = r + ( payload:byte( index + 2 ) * 0x100 )
	r = r + payload:byte( index + 3 )
	return r
end

local net_extract_rgb = function( payload, index )
	return string.char( payload:byte( index ), payload:byte( index + 1 ), payload:byte( index + 2 ) )
end

local net_recv_ping = function( args )
	net_print( "net_recv_ping" )
	-- only reply to servers, glims may send announcements that we don't care about (and we'll see our own broadcasts)
	if glim.HW_SERVER == args.payload:byte( 2 ) then
		net_print( "server ping request from "..net_extract_pascal_string( args.payload, 3 ).." ("..args.ip..":"..args.port..")" )
		net_send_ident_message( glim.MSG_PONG, args.ip, args.port )
		glim.server_ip = args.ip
		glim.server_port = args.port
	end
end

local net_recv_pong = function( args )
	net_print( "net_recv_pong from "..net_extract_pascal_string( args.payload, 3 ).." ("..args.ip..":"..args.port..")" )
	-- probably a server responding to a ping broadcast, no use for it here
	glim.server_ip = args.ip
	glim.server_port = args.port
end

local net_recv_btnc = function( args )
	if 12 ~= args.payload:len() then
		net_print( "net_recv_btnc: malformed payload of "..args.payload:len().." bytes" )
		return
	end

	net_print( "net_recv_btnc" )
	local min = net_extract_rgb( args.payload, 2 )
	local max = net_extract_rgb( args.payload, 5 )
	local period = net_extract_short( args.payload, 8 )
	local onheld = net_extract_rgb( args.payload, 10 )
	glimmer_set( min, max, period, onheld )
end

local net_data_handler = {
	[1] = net_recv_rgb1,
	[2] = net_recv_rgb2,
	[3] = net_recv_ping,
	[4] = net_recv_pong,
	[6] = net_recv_btnc,
}

local net_data_rcv = function( sck, data, port, ip )
	--net_print( "data received from "..ip..":"..port )

	local f = net_data_handler[data:byte( 1 )]

	if f == nil then
		net_print( "unknown directive from "..ip..":"..port )
	else
		f( { payload = data, ip = ip, port = port } )
	end
end


net_on_connect = function()
	-- called by the wifi module
	net_print( "connected" )

	glim.socket = net.createUDPSocket()
	glim.socket:listen( glim.PORT )
	glim.socket:on( "receive", net_data_rcv )

	if glim.ANNOUNCE then
		net_print( "announcing" )
		net_send_ident_message( glim.MSG_PING, "255.255.255.255", glim.PORT )
	end
end

net_on_disconnect = function()
	-- called by the wifi module
	net_print( "disconnected" )

	if nil ~= glim.socket then
		glim.socket:close()
		glim.socket = nil
	end
end

net_send_button_state = function( state )
	-- called by button trigger receiver
	if nil ~= glim.socket and nil ~= glim.server_ip and glim.server_port ~= nil then
		if "down" == state then
			state = 1
		elseif "held" == state then
			state = 2
		elseif "up" == state then
			state = 3
		end
		glim.socket:send( glim.server_port, glim.server_ip, string.char( glim.MSG_BTNS, state ) )
	end
end
