/*
 * Created by SharpDevelop.
 * User: Steffen
 * Date: 08-11-2009
 * Time: 12:11
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;

namespace DaLi.Tests
{
	/// <summary>
	/// Description of Extensions.
	/// </summary>
	public static class Extensions
	{
		public static T Invoke<T>(this object instance, string method, params object[] args)
		{
			if (instance == null)
				throw new ArgumentException("Instance cannot be null");
			MethodInfo mi = instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
			
			return (T)mi.Invoke(instance, args);
		}
	}
}
