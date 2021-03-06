﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Security;
using System.Diagnostics;

namespace PowerArgs
{
    internal static class ArgRevivers
    {
        private static Dictionary<Type, Func<string, string, object>> revivers;
        private static Dictionary<Type, Func<string, string, object>> Revivers
        {
            get
            {
                if (revivers == null)
                {
                    revivers = new Dictionary<Type, Func<string, string, object>>();
                    LoadDefaultRevivers(revivers);
                }
                return revivers;
            }
        }

        internal static bool CanRevive(Type t)
        {

            if (System.ComponentModel.TypeDescriptor.GetConverter(t).CanConvertFrom(typeof(string))) return true;

            if (Revivers.ContainsKey(t) || 
                t.IsEnum || 
                (t.GetInterfaces().Contains(typeof(IList)) && t.IsGenericType && CanRevive(t.GetGenericArguments()[0]) ) ||
                (t.IsArray && CanRevive(t.GetElementType()) ))
                return true;
            SearchAssemblyForRevivers(t.Assembly);
            return Revivers.ContainsKey(t);
        }

        internal static object ReviveEnum(Type t, string value, bool ignoreCase)
        {
            try
            {
                return Enum.Parse(t, value, ignoreCase);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw new ArgException(value+" is not a valid value for type "+t.Name+", options are "+string.Join(", ",Enum.GetNames(t)));
            }
        }

        internal static object Revive(Type t, string name, string value)
        {
            if (t.IsArray == false && t.GetInterfaces().Contains(typeof(IList)))
            {
                var list = (IList) Activator.CreateInstance(t);
                // TODO - Maybe support custom delimiters via an attribute on the property
                // TODO - Maybe do a full parse of the value to check for quoted strings

                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    foreach (var element in value.Split(','))
                    {
                        list.Add(Revive(t.GetGenericArguments()[0], name + "_element", element));
                    }
                }
                return list;
            }
            else if (t.IsArray)
            {
                var elements = value.Split(',');

                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    Array array = Array.CreateInstance(t.GetElementType(), elements.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        array.SetValue(Revive(t.GetElementType(), name + "[" + i + "]", elements[i]), i);
                    }
                    return array;
                }
                else
                {
                    return Array.CreateInstance(t.GetElementType(), 0);
                }
            }
            else if (Revivers.ContainsKey(t))
            {
                return Revivers[t].Invoke(name, value);
            }
            else if (System.ComponentModel.TypeDescriptor.GetConverter(t).CanConvertFrom(typeof(string)))
            {
                return System.ComponentModel.TypeDescriptor.GetConverter(t).ConvertFromString(value);
            }
            else
            {
                throw new ArgumentException("Cannot revive type "+t.FullName);
            }
        }

        private static void SearchAssemblyForRevivers(Assembly a)
        {
            foreach (var type in a.GetTypes())
            {
                var revivers = from m in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                               where m.HasAttr<ArgReviverAttribute>() &&
                                     m.GetParameters().Length == 2 &&
                                     m.GetParameters()[0].ParameterType == typeof(string) &&
                                     m.GetParameters()[1].ParameterType == typeof(string) &&
                                     m.ReturnType != typeof(void)
                               select m;

                foreach (var reviver in revivers)
                {
                    var r = reviver;
                    if (ArgRevivers.Revivers.ContainsKey(r.ReturnType) == false)
                    {
                        ArgRevivers.Revivers.Add(r.ReturnType, (key, val) =>
                        {
                            return r.Invoke(null, new object[] { key, val });
                        });
                    }
                }
            }
        }

        private static void LoadDefaultRevivers(Dictionary<Type, Func<string, string, object>> revivers)
        {
            revivers.Add(typeof(bool), (prop, val) =>
            {
                return val != null && val.ToLower().ToString() != "false" && val != "0"; // null means the switch value was not specified.  If it was specified then it's automatically true
            });

            revivers.Add(typeof(Guid), (prop, val) =>
            {
                Guid ret;
                if (Guid.TryParse(val, out ret) == false) throw new FormatException("value must be a Guid: " + val);
                return ret;
            });

            revivers.Add(typeof(byte), (prop, val) =>
            {
                byte ret;
                if (byte.TryParse(val, out ret) == false) throw new FormatException("value must be a byte: " + val);
                return ret;
            });

            revivers.Add(typeof(int), (prop, val) =>
            {
                int ret;
                if (int.TryParse(val, out ret) == false) throw new FormatException("value must be an integer: " + val);
                return ret;
            });

            revivers.Add(typeof(long), (prop, val) =>
            {
                long ret;
                if (long.TryParse(val, out ret) == false) throw new FormatException("value must be an integer: " + val);
                return ret;
            });

            revivers.Add(typeof(double), (prop, val) =>
            {
                double ret;
                if (double.TryParse(val, out ret) == false) throw new FormatException("value must be a number: " + val);
                return ret;
            });

            revivers.Add(typeof(string), (prop, val) =>
            {
                return val;
            });

            revivers.Add(typeof(DateTime), (prop, val) =>
            {
                DateTime ret;
                if (DateTime.TryParse(val, out ret) == false) throw new ArgumentException("value must be a valid date time: " + val);
                return ret;
            });

            revivers.Add(typeof(SecureStringArgument), (prop, val) =>
            {
                if (val != null) throw new ArgException("The value for " + prop + " cannot be specified on the command line");
                return new SecureStringArgument(prop);
            });
        }
    }
}
