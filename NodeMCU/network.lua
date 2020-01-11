local net_print = function( msg )
	print( "network: "..msg )
end

local const = protect( {
	LISTEN_PORT = 1998,
	MSG_PING = 3,
	MSG_PONG = 4,
	MSG_BTNC = 6,
	HW_SERVER = 1,
	HW_GLIM_V2 = 2,
	HW_GLIM_V3 = 3,
} )

local self = {
	socket = nil,
	server_ip = nil,
	server_port = nil,
	cb_net_state = nil,
	cb_rgb_vector = nil,
	cb_btn_glimmer = nil,
}

local net_pack_integer = function( intr )
	local net_pull_byte = function( intr )
		local b = intr % 0x100
		return b, ( intr - b ) / 0x100
	end
	local b1, b2, b3, b4
	b1, intr = net_pull_byte( intr )
	b2, intr = net_pull_byte( intr )
	b3, intr = net_pull_byte( intr )
	b4, intr = net_pull_byte( intr )
	return string.char( b4, b3, b2, b1 )
end

local net_send_ident_message = function( msg, ip, port )
	-- sending ping or pong with our type and identity
	self.socket:send( port, ip, string.char( msg, const.HW_GLIM_V3, cfg.hostname:len() )..cfg.hostname..net_pack_integer( tmr.time() ) )
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

local net_data_handler = {
	-- netmsg: 1=rgb1
	[1] = function( args )
		self.cb_rgb_vector( args.payload:sub( 2 ) )
	end,
	[const.MSG_PING] = function( args )
		-- only reply to servers, glims may send announcements that we don't care about (and we'll see our own broadcasts)
		if const.HW_SERVER == args.payload:byte( 2 ) then
			if self.server_ip ~= args.ip then
				net_print( "server ping request from "..net_extract_pascal_string( args.payload, 3 ).." ("..args.ip..":"..args.port..")" )
			end
			net_send_ident_message( const.MSG_PONG, args.ip, args.port )
			self.server_ip = args.ip
			self.server_port = args.port
		end
	end,
	[const.MSG_PONG] = function( args )
		net_print( "net_recv_pong from "..net_extract_pascal_string( args.payload, 3 ).." ("..args.ip..":"..args.port..")" )
		-- probably a server responding to a ping broadcast, no use for it here
		self.server_ip = args.ip
		self.server_port = args.port
	end,
	-- netmsg: 6=btnc
	[6] = function( args )
		if 12 ~= args.payload:len() then
			net_print( "net_recv_btnc: malformed payload of "..args.payload:len().." bytes" )
			return
		end
		--net_print( "net_recv_btnc" )
		local net_extract_rgb = function( payload, index )
			return string.char( payload:byte( index ), payload:byte( index + 1 ), payload:byte( index + 2 ) )
		end
		local min = net_extract_rgb( args.payload, 2 )
		local max = net_extract_rgb( args.payload, 5 )
		local period = net_extract_short( args.payload, 8 )
		local onheld = net_extract_rgb( args.payload, 10 )
		self.cb_btn_glimmer( { min = min, max = max, period = period, onheld = onheld } )
	end,
}

return {
	initialize = function( on_state_changed, on_recv_rgb, on_recv_btnc )
		net_print( "net.initialize" )

		self.cb_net_state = on_state_changed
		self.cb_rgb_vector = on_recv_rgb
		self.cb_btn_glimmer = on_recv_btnc

		-- Register WiFi Station event callbacks
		wifi.eventmon.register( wifi.eventmon.STA_CONNECTED, function( args )
			net_print( args.SSID.." connected" )
			self.cb_net_state( "connected" )
		end )

		wifi.eventmon.register( wifi.eventmon.STA_GOT_IP, function( args )
			net_print( "ip address "..args.IP )
			self.cb_net_state( "established" )
			self.socket = net.createUDPSocket()
			self.socket:listen( const.LISTEN_PORT )
			self.socket:on( "receive", function( sck, data, port, ip )
				local f = net_data_handler[data:byte( 1 )]
				if f == nil then
					net_print( "unknown directive "..data:byte(1).." from "..ip..":"..port )
				else
					f( { payload = data, ip = ip, port = port } )
				end
			end )
			net_send_ident_message( const.MSG_PING, "255.255.255.255", const.LISTEN_PORT )
		end )

		wifi.eventmon.register( wifi.eventmon.STA_DISCONNECTED, function( args )
			net_print( args.SSID.." disconnected: "..args.reason )
			self.cb_net_state( "disconnected" )
			if self.socket then
				self.socket:close()
				self.socket = nil
			end
		end )

		net_print( "connecting to "..cfg.wifi.ssid )
		wifi.setmode( wifi.STATION )
		wifi.sta.sethostname( cfg.hostname )
		wifi.sta.config( cfg.wifi )

		self.cb_net_state( "connecting" )
	end,

	send_button_state = function( state )
		-- called by button trigger receiver
		if self.socket and self.server_ip and self.server_port then
			local states = {
				["down"] = 1,
				["held"] = 2,
				["up"] = 3
			}
			-- netmsg: 5=btns
			self.socket:send( self.server_port, self.server_ip, string.char( 5, states[state] ) )
		end
	end
}
