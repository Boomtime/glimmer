using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShadowCreatures.Glimmer.Controls {
	public partial class LabelledTrackBar : UserControl {
		bool mEnableEvents = true;

		public LabelledTrackBar() {
			InitializeComponent();
			ctlTrackBar.ValueChanged += CtlTrackBar_ValueChanged;
		}

		public override string Text {
			get => base.Text;
			set {
				base.Text = value;
				ctlName.Text = value;
			}
		}

		public double Maximum { get; set; } = 1.0;

		public double Minimum { get; set; } = 0.0;

		public double Range { get => Maximum - Minimum; }

		public double Value {
			get {
				return Minimum + ( (double)ctlTrackBar.Value / (double)ctlTrackBar.Maximum ) * Range;
			}
			set {
				if( value < Minimum ) {
					value = Minimum;
				}
				if( value > Maximum ) {
					value = Maximum;
				}
				try {
					mEnableEvents = false;
					ctlTrackBar.Value = (int)( ctlTrackBar.Maximum * ( value - Minimum ) / Range );
				}
				finally {
					mEnableEvents = true;
				}
			}
		}

		public event EventHandler<EventArgs> ValueChanged;

		void CtlTrackBar_ValueChanged( object sender, EventArgs e ) {
			if( mEnableEvents ) {
				ValueChanged?.Invoke( this, EventArgs.Empty );
			}
			ctlEdit.Text = string.Format( "{0:0.00}", Value );
		}
	}
}
