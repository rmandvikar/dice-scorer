using System;

namespace rm.DiceScore
{
	/// <summary>
	/// Throw extensions.
	/// </summary>
	public static class ThrowEx
	{
		public static void ThrowIfNull(object obj, string paramName)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(paramName);
			}
		}

		public static void ThrowIfArgumentOutOfRange(int[] values, string paramName,
			int min, int max)
		{
			foreach (var value in values)
			{
				ThrowIfArgumentOutOfRange(value, paramName, min, max);
			}
		}

		public static void ThrowIfArgumentOutOfRange(int value, string paramName,
			int min, int max)
		{
			if (value < min || value > max)
			{
				throw new ArgumentOutOfRangeException(paramName,
					$"Value should be between {min} and {max} but was {value}.");
			}
		}

		public static void ThrowIfArgumentInValid(int value, string paramName,
			int expected)
		{
			if (value != expected)
			{
				throw new ArgumentException(paramName,
					$"Value should be {expected} but was {value}.");
			}
		}
	}
}
