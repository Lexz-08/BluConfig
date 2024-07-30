using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using Newtonsoft.Json.Linq;

namespace BluConfig
{
	/// <summary>
	/// Handles the setup, loading, and saving of configuration information in a class marked with the <see cref="ConfigAttribute"/>.
	/// </summary>
	public static class ConfigHandler
	{
		private static bool _init = false;
		private static string _mainConf = "";
		private static Format _mainFmt = 0;
		private static Dictionary<string, Format> _multiFmt = new Dictionary<string, Format>();
		private static Dictionary<string, FieldInfo> _main = new Dictionary<string, FieldInfo>();
		private static Dictionary<string, Dictionary<string, FieldInfo>> _multi = new Dictionary<string, Dictionary<string, FieldInfo>>();

		private static readonly Dictionary<string, bool> _bool = new Dictionary<string, bool> { { "false", false }, { "true", true } };
		private static readonly Dictionary<bool, string> _boolr = new Dictionary<bool, string> { { false, "false" }, { true, "true" } };
		private static readonly string _file = Environment.CurrentDirectory + "\\config";

		static ConfigHandler()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
			{
				using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("BluConfig.Newtonsoft.Json.dll"))
				{
					byte[] buffer = new byte[s.Length];
					s.Read(buffer, 0, buffer.Length);

					return Assembly.Load(buffer);
				}
			};
		}

		/// <summary>
		/// Sets up the config fields for loading and saving and creates the config file if it does not already exist.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Setup()
		{
			if (_init) throw new InvalidOperationException("ConfigHandler already initialized configuration(s).");

			Type[] confs = Assembly.GetCallingAssembly().GetTypes()
				.Where(t => t.IsDefined(typeof(ConfigAttribute))).ToArray();
			Type[] mains = confs.Where(c => string.IsNullOrEmpty(c.GetCustomAttribute<ConfigAttribute>().File)).ToArray();

			if (mains.Length > 1)
				throw new InvalidOperationException("Can only have one main config class.");
			else if (confs.GroupBy(c => c.GetCustomAttribute<ConfigAttribute>().File).Any(grp => grp.Count() > 1))
				throw new InvalidOperationException("Can only have one config class per file.");
			else if (mains.Length > 0 && confs.Count(c => !string.IsNullOrEmpty(c.GetCustomAttribute<ConfigAttribute>().File)) > 0)
				throw new InvalidOperationException("Can only have one main config class or multiple unique config files.");
			else if (confs.Count(c => !(c.IsAbstract && c.IsSealed)) > 0)
				throw new InvalidOperationException($"'{confs.Where(c => !(c.IsAbstract && c.IsSealed)).First().Name}' must be static.");

			Type[] supported = new Type[] { typeof(int), typeof(float), typeof(double), typeof(string), typeof(bool) };
			if (mains.Length == 1)
			{
				Type main = mains[0];
				FieldInfo[] fields = main.GetFields(BindingFlags.Public | BindingFlags.Static);

				if (fields.Count(fl => !supported.Contains(fl.FieldType)) > 0)
					throw new InvalidOperationException($"'{main.Name}.{fields.Where(fl => !supported.Contains(fl.FieldType)).First().Name}' must be a supported type: int, float, double, string, or bool.");

				_mainFmt = main.GetCustomAttribute<ConfigAttribute>().Format;
				foreach (FieldInfo fl in fields) _main.Add(fl.Name, fl);
				_mainConf = "config";
			}
			else
			{
				Type[] named = confs.Where(c => !string.IsNullOrEmpty(c.GetCustomAttribute<ConfigAttribute>().File)).ToArray();

				if (named.Length > 1) foreach (Type nm in named)
					{
						Dictionary<string, FieldInfo> configs = new Dictionary<string, FieldInfo>();
						FieldInfo[] fields = nm.GetFields(BindingFlags.Public | BindingFlags.Static);

						if (fields.Count(fl => !supported.Contains(fl.FieldType)) > 0)
							throw new InvalidOperationException($"'{nm.Name}.{fields.Where(fl => !supported.Contains(fl.FieldType)).First().Name}' must be a supported type: int, float, double, string, or bool.");

						foreach (FieldInfo fl in fields) configs.Add(fl.Name, fl);

						ConfigAttribute attr = nm.GetCustomAttribute<ConfigAttribute>();
						_multiFmt.Add(attr.File, attr.Format);
						_multi.Add(attr.File, configs);
					}
				else if (named.Length == 1)
				{
					Type nm = named[0];
					FieldInfo[] fields = nm.GetFields(BindingFlags.Public | BindingFlags.Static);

					if (fields.Count(fl => !supported.Contains(fl.FieldType)) > 0)
						throw new InvalidOperationException($"'{nm.Name}.{fields.Where(fl => !supported.Contains(fl.FieldType)).First().Name}' must be a supported type: int, float ,double, string, or bool.");
					
					foreach (FieldInfo fl in fields) _main.Add(fl.Name, fl);
					ConfigAttribute attr = nm.GetCustomAttribute<ConfigAttribute>();
					_mainFmt = attr.Format;
					_mainConf = attr.File;
				}
			}

			if (_main.Count > 0 && !File.Exists(_mainConf))
			{
				switch (_mainFmt)
				{
					case Format.Blu:
						File.WriteAllLines(_mainConf, _main.Select(cf => $"{cf.Key} = {(cf.Value.FieldType == typeof(bool) ? _boolr[(bool)cf.Value.GetValue(null)] : cf.Value.GetValue(null))}"));
						break;
					case Format.JSON:
						{
							JObject values = new JObject();
							foreach (var (Name, Value) in _main.Select(kv => (Name: kv.Key, Value: kv.Value.GetValue(null))))
								values.Add(Name, Value.GetType() == typeof(bool) ? _boolr[(bool)Value] : Value.ToString());
							File.WriteAllText(_mainConf, values.ToString().Replace("  ", "\t"));
						}
						break;
					case Format.XML:
						{
							XmlDocument values = new XmlDocument();
							XmlDeclaration decl = values.CreateXmlDeclaration("1.0", "UTF-8", null);
							XmlElement root = values.DocumentElement;
							values.InsertBefore(decl, root);

							XmlElement config = values.CreateElement("config");
							values.AppendChild(config);

							foreach (var (Name, Value) in _main.Select(cf => (Name: cf.Key, Value: cf.Value.GetValue(null))))
							{
								XmlElement fieldEl = values.CreateElement("field");
								fieldEl.SetAttribute("name", Name);
								fieldEl.SetAttribute("value", Value.GetType() == typeof(bool) ? _boolr[(bool)Value] : Value.ToString());
								config.AppendChild(fieldEl);
							}

							values.Save(_mainConf);
						}
						break;
				}
			}
			else
			{
				foreach (string conf in _multi.Keys)
				{
					if (!File.Exists(conf))
						switch (_multiFmt[conf])
						{
							case Format.Blu:
								File.WriteAllLines(conf, _multi[conf].Select(cf => $"{cf.Key} = {(cf.Value.FieldType == typeof(bool) ? _boolr[(bool)cf.Value.GetValue(null)] : cf.Value.GetValue(null).ToString())}"));
								break;
							case Format.JSON:
								{
									JObject values = new JObject();
									foreach (var (Name, Value) in _multi[conf].Select(cf => (Name: cf.Key, Value: cf.Value.GetValue(null))))
										values.Add(Name, Value.GetType() == typeof(bool) ? _boolr[(bool)Value] : Value.ToString());
									File.WriteAllText(conf, values.ToString().Replace("  ", "\t"));
								}
								break;
							case Format.XML:
								{
									XmlDocument values = new XmlDocument();
									XmlDeclaration decl = values.CreateXmlDeclaration("1.0", "UTF-8", null);
									XmlElement root = values.DocumentElement;
									values.InsertBefore(decl, root);

									XmlElement config = values.CreateElement("config");
									values.AppendChild(config);

									foreach (var (Name, Value) in _multi[conf].Select(cf => (Name: cf.Key, Value: cf.Value.GetValue(null))))
									{
										XmlElement fieldEl = values.CreateElement("field");
										fieldEl.SetAttribute("name", Name);
										fieldEl.SetAttribute("value", Value.GetType() == typeof(bool) ? _boolr[(bool)Value] : Value.ToString());
										config.AppendChild(fieldEl);
									}

									values.Save(conf);
								}
								break;
						}
				}
			}

			_init = true;
		}

		/// <summary>
		/// Loads config information from the config file into the config class.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Load()
		{
			if (!_init) throw new InvalidOperationException("Config must be initialized first. Call 'ConfigHandler.Setup()'");

			if (_main.Count > 0)
			{
				switch (_mainFmt)
				{
					case Format.Blu:
						{
							Dictionary<string, string> values = File.ReadAllLines(_mainConf)
								.ToDictionary(k => k.Substring(0, k.IndexOf(' ')), v => v.Substring(v.IndexOf('=') + 2));
							foreach (string field in values.Keys)
							{
								if (_main[field].FieldType == typeof(int))
									_main[field].SetValue(null, int.Parse(values[field]));
								else if (_main[field].FieldType == typeof(float))
									_main[field].SetValue(null, float.Parse(values[field]));
								else if (_main[field].FieldType == typeof(double))
									_main[field].SetValue(null, double.Parse(values[field]));
								else if (_main[field].FieldType == typeof(string))
									_main[field].SetValue(null, values[field]);
								else if (_main[field].FieldType == typeof(bool))
									_main[field].SetValue(null, _bool[values[field]]);
							}
						}
						break;
					case Format.JSON:
						{
							JObject values = JObject.Parse(File.ReadAllText(_mainConf));
							foreach (JProperty value in values.Properties())
							{
								if (int.TryParse(value.Value<string>(), out _))
									_main[value.Name].SetValue(null, value.Value<int>());
								else if (float.TryParse(value.Value<string>(), out _))
									_main[value.Name].SetValue(null, value.Value<float>());
								else if (double.TryParse(value.Value<string>(), out _))
									_main[value.Name].SetValue(null, value.Value<double>());
								else if (_bool.ContainsKey(value.Value<string>()))
									_main[value.Name].SetValue(null, value.Value<bool>());
								else _main[value.Name].SetValue(null, value.Value<string>());
							}
						}
						break;
					case Format.XML:
						{
							XmlDocument values = new XmlDocument(); values.Load(_mainConf);
							XmlNodeList nodes = values.SelectNodes("/config/field");

							foreach (XmlNode field in nodes)
							{
								string name = field.Attributes["name"].Value;
								string value = field.Attributes["value"].Value;

								if (int.TryParse(value, out _)) _main[name].SetValue(null, int.Parse(value));
								else if (float.TryParse(value, out _)) _main[name].SetValue(null, float.Parse(value));
								else if (double.TryParse(value, out _)) _main[name].SetValue(null, double.Parse(value));
								else if (_bool.ContainsKey(value)) _main[name].SetValue(null, _bool[value]);
								else _main[name].SetValue(null, value);
							}
						}
						break;
				}
			}
			else
			{
				foreach (string conf in _multi.Keys)
				{
					switch (_multiFmt[conf])
					{
						case Format.Blu:
							{
								Dictionary<string, string> values = File.ReadAllLines(conf)
									.ToDictionary(k => k.Substring(0, k.IndexOf(' ')), v => v.Substring(v.IndexOf('=') + 2));
								foreach (string field in values.Keys)
								{
									if (int.TryParse(values[field], out _)) _multi[conf][field].SetValue(null, int.Parse(values[field]));
									else if (float.TryParse(values[field], out _)) _multi[conf][field].SetValue(null, float.Parse(values[field]));
									else if (double.TryParse(values[field], out _)) _multi[conf][field].SetValue(null, double.Parse(values[field]));
									else if (_bool.ContainsKey(values[field])) _multi[conf][field].SetValue(null, _bool[values[field]]);
									else _multi[conf][field].SetValue(null, values[field]);
								}
							}
							break;
						case Format.JSON:
							{
								JObject values = JObject.Parse(File.ReadAllText(conf));
								foreach (JProperty field in values.Properties())
								{
									if (int.TryParse(field.Value<string>(), out _)) _multi[conf][field.Name].SetValue(null, int.Parse(field.Value<string>()));
									else if (float.TryParse(field.Value<string>(), out _)) _multi[conf][field.Name].SetValue(null, float.Parse(field.Value<string>()));
									else if (double.TryParse(field.Value<string>(), out _)) _multi[conf][field.Name].SetValue(null, double.Parse(field.Value<string>()));
									else if (_bool.ContainsKey(field.Value<string>())) _multi[conf][field.Name].SetValue(null, _bool[field.Value<string>()]);
									else _multi[conf][field.Name].SetValue(null, field.Value<string>());
								}
							}
							break;
						case Format.XML:
							{
								XmlDocument values = new XmlDocument();
								XmlNodeList nodes = values.SelectNodes("/config/field");

								foreach (XmlNode field in nodes)
								{
									string name = field.Attributes["name"].Value;
									string value = field.Attributes["value"].Value;

									if (int.TryParse(value, out _)) _multi[conf][name].SetValue(null, int.Parse(value));
									else if (float.TryParse(value, out _)) _multi[conf][name].SetValue(null, float.Parse(value));
									else if (double.TryParse(value, out _)) _multi[conf][name].SetValue(null, double.Parse(value));
									else if (_bool.ContainsKey(value)) _multi[conf][name].SetValue(null, _bool[value]);
									else _multi[conf][name].SetValue(null, value);
								}
							}
							break;
					}
				}
			}
		}

		/// <summary>
		/// Saves the information from the config class into the config file.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Save()
		{
			if (!_init) throw new InvalidOperationException("Config must be initialized first. Call 'ConfigHandler.Setup()'");

			if (_main.Count > 0)
			{
				switch (_mainFmt)
				{
					case Format.Blu:
						File.WriteAllLines(_mainConf, _main.Select(cf => $"{cf.Key} = {(cf.Value.FieldType == typeof(bool) ? _boolr[(bool)cf.Value.GetValue(null)] : cf.Value.GetValue(null))}"));
						break;
					case Format.JSON:
						{
							JObject values = new JObject();
							foreach (var (Name, Value) in _main.Select(kv => (Name: kv.Key, Value: kv.Value.GetValue(null))))
								values.Add(Name, Value.GetType() == typeof(bool) ? _boolr[(bool)Value] : Value.ToString());
							File.WriteAllText(_mainConf, values.ToString().Replace("  ", "\t"));
						}
						break;
					case Format.XML:
						{
							XmlDocument values = new XmlDocument();
							XmlDeclaration decl = values.CreateXmlDeclaration("1.0", "UTF-8", null);
							XmlElement root = values.DocumentElement;
							values.InsertBefore(decl, root);

							XmlElement config = values.CreateElement("config");
							values.AppendChild(config);

							foreach (var (Name, Value) in _main.Select(cf => (Name: cf.Key, Value: cf.Value.GetValue(null))))
							{
								XmlElement fieldEl = values.CreateElement("field");
								fieldEl.SetAttribute("name", Name);
								fieldEl.SetAttribute("value", Value.GetType() == typeof(bool) ? _boolr[(bool)Value] : Value.ToString());
								config.AppendChild(fieldEl);
							}

							values.Save(_mainConf);
						}
						break;
				}
			}
			else
			{
				foreach (string conf in _multi.Keys)
				{
					switch (_multiFmt[conf])
					{
						case Format.Blu:
							File.WriteAllLines(conf, _multi[conf].Select(cf => $"{cf.Key} = {(cf.Value.FieldType == typeof(bool) ? _boolr[(bool)cf.Value.GetValue(null)] : cf.Value.GetValue(null).ToString())}"));
							break;
						case Format.JSON:
							{
								JObject values = new JObject();
								foreach (var (Name, Value) in _multi[conf].Select(cf => (Name: cf.Key, Value: cf.Value.GetValue(null))))
									values.Add(Name, Value.GetType() == typeof(bool) ? _boolr[(bool)Value] : Value.ToString());
								File.WriteAllText(conf, values.ToString().Replace("  ", "\t"));
							}
							break;
						case Format.XML:
							{
								XmlDocument values = new XmlDocument();
								XmlDeclaration decl = values.CreateXmlDeclaration("1.0", "UTF-8", null);
								XmlElement root = values.DocumentElement;
								values.InsertBefore(decl, root);

								XmlElement config = values.CreateElement("config");
								values.AppendChild(config);

								foreach (var (Name, Value) in _multi[conf].Select(cf => (Name: cf.Key, Value: cf.Value.GetValue(null))))
								{
									XmlElement fieldEl = values.CreateElement("field");
									fieldEl.SetAttribute("name", Name);
									fieldEl.SetAttribute("value", Value.GetType() == typeof(bool) ? _boolr[(bool)Value] : Value.ToString());
									config.AppendChild(fieldEl);
								}

								values.Save(conf);
							}
							break;
					}
				}
			}
		}
	}
}
