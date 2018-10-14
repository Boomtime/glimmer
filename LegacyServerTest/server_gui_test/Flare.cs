using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowCreatures.Luminosity;
using System.Drawing;

namespace server_gui_test
{
	class Flare
	{
		int m_pixel;
		ColorReal m_clr;
		bool m_asc;
		double m_step;

		public Flare( int p_pixel, ColorReal p_colour, double p_step )
		{
			m_pixel = p_pixel;
			m_clr = p_colour;
			m_clr.Luminance = 0;
			m_asc = true;
			m_step = p_step;
		}

		public void Step()
		{
			// Luminance clamps to unity internally
			if( m_asc )
				m_clr.Luminance += m_step;
			else
			if( m_clr.Luminance > 0 )
				m_clr.Luminance -= m_step;

			// rebound
			if( 0.6 <= m_clr.Luminance )
				m_asc = false;
		}

		/// <summary>current value</summary>
		public ColorReal Value
		{
			get { return m_clr; }
		}

		/// <summary>get the pixel number</summary>
		public int Pixel
		{
			get { return m_pixel; }
		}

		public void WriteToBuffer( byte[] p_buffer )
		{
			int ordinal = m_pixel * 3;
			Color clr = m_clr;

			p_buffer[ordinal + 0] = clr.R;
			p_buffer[ordinal + 1] = clr.G;
			p_buffer[ordinal + 2] = clr.B;
		}
	}
}
