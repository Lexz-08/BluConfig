using System;

namespace BluConfig
{
	/// <summary>
	/// Marks a static class as containing config fields.<br/><br/>
	/// This class cannot be inherited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ConfigAttribute : Attribute { }

	/// <summary>
	/// Marks a config field as a numeric value. Can be an <see langword="int"/>, <see langword="float"/>, or <see langword="double"/>.<br/><br/>
	/// This class cannot be inherited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class NumberAttribute : Attribute { }

	/// <summary>
	/// Marks a config field as a plain text value. Value can contain any characters.<br/><br/>
	/// This class cannot be inherited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class TextAttribute : Attribute { }

	/// <summary>
	/// Marks a config field as a boolean/binary value.<br/><br/>
	/// This class cannot be inherited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class BoolAttribute : Attribute { }
}
