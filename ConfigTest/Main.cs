using System.Windows.Forms;

using BluConfig;

namespace ConfigTest
{
	public partial class Main : Form
	{
		[Config("first")]
		private static class Cfg
		{
			public static int Number;
			public static string Text;
			public static bool Boolean;
		}

		[Config("second")]
		private static class Cfg2
		{
			public static int Number;
			public static string Text;
			public static bool Boolean;
		}

		public Main()
		{
			InitializeComponent();

			ConfigHandler.Setup();
			ConfigHandler.Load();

			cfg_Number.Value = Cfg.Number;
			cfg_Text.Text = Cfg.Text;
			cfg_Boolean.Checked = Cfg.Boolean;
			cfg_Number2.Value = Cfg2.Number;
			cfg_Text2.Text = Cfg2.Text;
			cfg_Boolean2.Checked = Cfg2.Boolean;

			cfg_Save.Click += (_, __) =>
			{
				Cfg.Number = (int)cfg_Number.Value;
				Cfg.Text = cfg_Text.Text;
				Cfg.Boolean = cfg_Boolean.Checked;
				Cfg2.Number = (int)cfg_Number2.Value;
				Cfg2.Text = cfg_Text2.Text;
				Cfg2.Boolean = cfg_Boolean2.Checked;

				ConfigHandler.Save();
			};
		}
	}
}
