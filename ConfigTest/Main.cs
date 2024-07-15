using System.Drawing;
using System.Windows.Forms;

namespace ConfigTest
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();

			console.GotFocus += (_, __) => label1.Focus();
		}

		private void log(object value, Color c)
		{
			string v = $"{value}\n";

			console.AppendText(v);
			console.SelectionStart = console.TextLength - v.Length;
			console.SelectionLength = v.Length;
			console.SelectionColor = c;
			console.SelectionStart = console.TextLength;
			console.SelectionLength = 0;
		}

		private void info(object value) => log(value, Color.LightGray);
		private void err(object value) => log(value, Color.OrangeRed);
		private void warn(object value) => log(value, Color.Goldenrod);
	}
}
