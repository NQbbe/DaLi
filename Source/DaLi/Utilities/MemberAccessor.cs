/*
 *  DaLi is a small libray to ease the use of MsSQL / MySQL databases from .Net.
 *  Copyright (C) 2009 Steffen Skov

 *  This file is part of DaLi.

 *  DaLi is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  DaLi is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.

 *  You should have received a copy of the GNU Lesser General Public License
 *  along with DaLi.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Ckode.DaLi.Utilities
{
	public class MemberAccessor
	{
	    internal readonly string name;
	    public string Name { get { return name; } }
	    private static readonly Func<object, object> NoGetter =
	        obj => { throw new InvalidOperationException("No getter for property"); };
	    private static readonly Action<object, object> NoSetter =
	        (obj, val) => { throw new InvalidOperationException("No setter for property"); };
	
	    public object GetValue(object target) { return getter(target); }
	    public void SetValue(object target, object value) { setter(target, value); }
	    internal readonly Func<object, object> getter;
	    internal readonly Action<object, object> setter;
	
	    public MemberAccessor(PropertyInfo property)
	    {
	        if (property == null) throw new ArgumentNullException();
	        name = property.Name;
	        MethodInfo method = property.GetGetMethod(true);
	        if (method == null)
	        {
	            getter = NoGetter;
	        }
	        else
	        {
	            if (method.IsStatic) throw new ArgumentException("Static properties not supported");
	            var dm = new DynamicMethod("__get_" + property.Name, typeof(object), new Type[] { typeof(object) }, property.DeclaringType);
	            var il = dm.GetILGenerator();
	            il.Emit(OpCodes.Ldarg_0);
	            il.Emit(OpCodes.Castclass, property.DeclaringType);
	            il.Emit(OpCodes.Callvirt, method);
	            if (property.PropertyType.IsValueType)
	            {
	                il.Emit(OpCodes.Box, property.PropertyType);
	            }
	            il.Emit(OpCodes.Ret);
	            getter = (Func<object, object>)dm.CreateDelegate(typeof(Func<object, object>));
	        }
	        method = property.GetSetMethod(true);
	        if (method == null)
	        {
	            setter = NoSetter;
	        }
	        else
	        {
	            if (method.IsStatic) throw new ArgumentException("Static properties not supported");
	            var dm = new DynamicMethod("__set_" + property.Name, null, new Type[] { typeof(object), typeof(object) }, property.DeclaringType);
	            var il = dm.GetILGenerator();
	            il.Emit(OpCodes.Ldarg_0);
	            il.Emit(OpCodes.Castclass, property.DeclaringType);
	            il.Emit(OpCodes.Ldarg_1);
	            if (property.PropertyType.IsValueType)
	            {
	                il.Emit(OpCodes.Unbox_Any, property.PropertyType);
	            }
	            else
	            {
	                il.Emit(OpCodes.Castclass, property.PropertyType);
	            }
	            il.Emit(OpCodes.Callvirt, method);
	            il.Emit(OpCodes.Ret);
	            setter = (Action<object, object>)dm.CreateDelegate(typeof(Action<object, object>));
	        }
	    }
	    public MemberAccessor(FieldInfo field)
	    {
	        if (field == null) throw new ArgumentNullException("field");
	        if (field.IsStatic) throw new ArgumentException("Static fields not supported");
	        name = field.Name;
	        var method = new DynamicMethod("__get_" + field.Name, typeof(object), new Type[] { typeof(object) }, field.DeclaringType);
	        var il = method.GetILGenerator();
	        il.Emit(OpCodes.Ldarg_0);
	        il.Emit(OpCodes.Castclass, field.DeclaringType);
	        il.Emit(OpCodes.Ldfld, field);
	        if (field.FieldType.IsValueType)
	        {
	            il.Emit(OpCodes.Box, field.FieldType);
	        }
	        il.Emit(OpCodes.Ret);
	        getter = (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
	
	        method = new DynamicMethod("__set_" + field.Name, null, new Type[] { typeof(object), typeof(object) }, field.DeclaringType);
	        il = method.GetILGenerator();
	        il.Emit(OpCodes.Ldarg_0);
	        il.Emit(OpCodes.Castclass, field.DeclaringType);
	        il.Emit(OpCodes.Ldarg_1);
	        if (field.FieldType.IsValueType)
	        {
	            il.Emit(OpCodes.Unbox_Any, field.FieldType);
	        }
	        else
	        {
	            il.Emit(OpCodes.Castclass, field.FieldType);
	        }
	        il.Emit(OpCodes.Stfld, field);
	        il.Emit(OpCodes.Ret);
	        setter = (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
	    }
	}
}
