# BluConfig
## Description
Provides simple loading and saving config information.
## How to Use
Just reference `BluConfig.dll` in your project and add the namespace `using BluConfig;`<br/><br/>

Because the `ConfigHandler.Load()` will automatically put the information into `MyConfig`, just assign its field values to whatever variables you need changed.
For `ConfigHandler.Save()`, make sure to assign values to the fields in `MyConfig` before you call `ConfigHandler.Save()`.<br/><br/>

Config fields are no longer marked with attributes, as `ConfigHandler` just auto-detects for the supported types: `int`, `float`, `double`, `bool`, and/or `string`.<br/><br/>

Also,
- `ConfigHandler.Setup()` and `ConfigHandler.Load()` should be called at the start of your program.
- `ConfigHandler.Save()` should be called when your program is closing.
```csharp
using System.Windows.Forms;
using BluConfig;

namespace MyProgram
{
    public partial class MyForm : Form
    {
        [Config]
        private static class MyConfig
        {
            public static int width;
            public static int height;
            public static string text;
            public static bool topmost;
        }
        
        public MyForm()
        {
            InitializeComponent();

            ConfigHandler.Setup();
            ConfigHandler.Load();
            
            FormClosing += (_, __) => ConfigHandler.Save();
        }
    }
}
```

## Download
[BluConfig.dll](https://github.com/Lexz-08/BluConfig/releases/latest/download/BluConfig.dll)
