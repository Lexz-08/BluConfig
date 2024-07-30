using System;

namespace BluConfig
{
	/// <summary>
	/// Represents the type of file format used for configuration management.
	/// </summary>
	public enum Format
	{
		/// <summary>
		/// The original, default file format.<br/><br/>
		/// 
		/// Example:<br/>
		/// <code>
		/// Number = 0,
		/// Text = Hello World!
		/// Boolean = false
		/// </code>
		/// </summary>
		Blu,

		/// <summary>
		/// The JSON file format.<br/><br/>
		/// 
		/// Example:<br/>
		/// <code>
		/// {
		///	    "Number": 0,
		///	    "Text": "Hello World!",
		///	    "Boolean": false
		/// }
		/// </code>
		/// </summary>
		JSON,

		/// <summary>
		/// The XML file format.<br/><br/>
		/// 
		/// Example:<br/>
		/// <code>
		/// &lt;?xml version="1.0"&gt;
		/// &lt;config&gt;
		///     &lt;field name="Number" value="0" /&gt;
		///     &lt;field name="Text" value="" /&gt;
		///     &lt;field name="Boolean" value="false" /&gt;
		/// &lt;/config&gt;
		/// </code>
		/// </summary>
		XML
	}

	/// <summary>
	/// Marks a static class as containing config fields.<br/><br/>
	/// This class cannot be inherited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ConfigAttribute : Attribute
	{
		public readonly string File;
		public readonly Format Format;

		public ConfigAttribute(string File = "", Format Format = Format.Blu)
		{ this.File = File; this.Format = Format; }
	}
}
