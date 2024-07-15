using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BluConfig
{
	/// <summary>
	/// Handles the setup, loading, and saving of configuration information in a class marked with the <see cref="ConfigAttribute"/>.
	/// </summary>
	public static class ConfigHandler
	{
		private static bool _init = false;
		private static Type _conf = null;
		private static Dictionary<string, FieldInfo> _configs = new Dictionary<string, FieldInfo>();

		private static readonly Type[] _cattrs = new Type[] { typeof(NumberAttribute), typeof(TextAttribute), typeof(BoolAttribute) };
		private static readonly Dictionary<string, bool> _bool = new Dictionary<string, bool> { { "false", false }, { "true", true } };
		private static readonly string _file = Environment.CurrentDirectory + "\\config";

		private static Type[] GetTypeAttr(Type[] Types, Type AttrType)
		{
			return Types.Where(t => t.IsDefined(AttrType)).ToArray();
		}

		/// <summary>
		/// Sets up the config fields for loading and saving and creates the config file if it does not already exist.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Setup()
		{
			Type[] types = Assembly.GetCallingAssembly().GetTypes();
			Type[] confs = GetTypeAttr(types, typeof(ConfigAttribute));

			if (confs.Length == 0) throw new InvalidOperationException("No config class was found.");
			if (confs.Length > 1) throw new InvalidOperationException("Only one config class can be marked.");
			_conf = confs[0];

			if (!(_conf.IsAbstract && _conf.IsSealed)) throw new InvalidOperationException("Config class must be static.");
			if (_conf.GetFields(BindingFlags.Public | BindingFlags.Static).Count() == 0)
				throw new InvalidOperationException("Config class does not contain any fields.");
			foreach (FieldInfo fl in _conf.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (fl.CustomAttributes.Count(ca => _cattrs.Contains(ca.AttributeType)) == 0)
					throw new InvalidOperationException($"{_conf.Name}.{fl.Name} is not marked as a config field.");
				else if (fl.CustomAttributes.Count(ca => _cattrs.Contains(ca.AttributeType)) > 1)
					throw new InvalidOperationException($"{_conf.Name}.{fl.Name} cannot have more than one config type.");

				_configs[fl.Name] = fl;
			}

			if (!File.Exists(_file)) File.WriteAllLines(_file, _configs.Select(kvp =>
			{
				Type attr = kvp.Value.CustomAttributes.First().AttributeType;
				string val = null;

				if (attr == _cattrs[0]) val = "0";
				else if (attr == _cattrs[1]) val = "";
				else if (attr == _cattrs[2]) val = "false";

				return $"{kvp.Key} = {val}";
			}));

			_init = true;
		}

		/// <summary>
		/// Loads config information from the config file into the config class.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Load()
		{
			if (!_init) throw new InvalidOperationException("Config must be initialized first. Call 'ConfigHandler.Setup()'");

			Dictionary<string, string> vals = File.ReadAllLines(_file)
				.ToDictionary(ln => ln.Substring(0, ln.IndexOf(' ')), ln => ln.Substring(ln.IndexOf('=') + 2));

			foreach (string conf in _configs.Keys)
			{
				if (_configs[conf].CustomAttributes.Count(ca => ca.AttributeType == _cattrs[0]) == 1)
				{
					if (_configs[conf].FieldType == typeof(int)) _configs[conf].SetValue(null, int.Parse(vals[conf]));
					else if (_configs[conf].FieldType == typeof(float)) _configs[conf].SetValue(null, float.Parse(vals[conf]));
					else if (_configs[conf].FieldType == typeof(double)) _configs[conf].SetValue(null, double.Parse(vals[conf]));
				}
				else if (_configs[conf].CustomAttributes.Count(ca => ca.AttributeType == _cattrs[1]) == 1)
					_configs[conf].SetValue(null, vals[conf]);
				else if (_configs[conf].CustomAttributes.Count(ca => ca.AttributeType == _cattrs[2]) == 1)
					_configs[conf].SetValue(null, _bool[vals[conf]]);
			}
		}

		/// <summary>
		/// Saves the information from the config class into the config file.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Save()
		{
			if (!_init) throw new InvalidOperationException("Config must be initialized first. Call 'ConfigHandler.Setup()'");

			File.WriteAllLines(_file, _configs.Select(kvp =>
			{
				if (kvp.Value.FieldType == typeof(bool))
					return $"{kvp.Key} = {kvp.Value.GetValue(null).ToString().ToLower()}";
				else return $"{kvp.Key} = {kvp.Value.GetValue(null)}";
			}));
		}
	}
}
