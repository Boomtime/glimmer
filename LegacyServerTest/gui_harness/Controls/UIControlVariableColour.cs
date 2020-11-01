using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShadowCreatures.Glimmer.Controls {
	public partial class UIControlVariableColour : UserControl {

		IControlVariable<Color> mVariable;

		public UIControlVariableColour() {
			InitializeComponent();
			ctlValue.Click += CtlValue_Click;
		}

		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
				ctlName.Text = value;
			}
		}

		public IControlVariable<Color> Variable {
			set {
				mVariable = value;
				mVariable.ValueChanged += ( s, e ) => Value = mVariable.Value;
				Value = mVariable.Value;
			}
		}

		private void CtlValue_Click( object sender, EventArgs e ) {
			using( var dlg = new ColorDialog { Color = Value } ) {
				if( DialogResult.OK == dlg.ShowDialog( this ) ) {
					Value = mVariable.Value = dlg.Color;
				}
			}
		}

		void UpdateColourDescription() {
			ctlEdit.Text = string.Format( "#{0:X2}{1:X2}{2:X2}", Value.R, Value.G, Value.B );
		}

		Color Value {
			get {
				return ctlValue.BackColor;
			}
			set {
				ctlValue.BackColor = value;
				if( ctlEdit.InvokeRequired ) {
					ctlEdit.BeginInvoke( new Action( UpdateColourDescription ) );
				}
				else {
					UpdateColourDescription();
				}
			}
		}
	}
}
