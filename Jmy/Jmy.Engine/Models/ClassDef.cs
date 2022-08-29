using Jmy.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Engine.Models
{
    public class ClassDef
    {
        private Type _type;
        public ClassDef(Type type)
        {
            _type = type;
        }

        public object Instantiate(object?[] paramArr)
        {
            var instance = _type.InvokeMember(_type.Name, BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null, paramArr);
            if (instance == null) throw new Exception($"unable to instantiate object of type '{_type.FullName}'");
            return instance;
        }

        public object? InvokeMember(string name, object?[] paramArr)
        {
            var method = GetMethod(name, paramArr);
            if (method == null) throw new Exception($"method {name} does not exist on type {_type.FullName}");
            if (!method.IsStatic) throw new Exception($"nonstatic method {name} requires instance to be invoked");
            return method.Invoke(_type, paramArr);
        }

        public object? InvokeMember(string name, object instance, object?[] paramArr)
        {
            var method = GetMethod(name, paramArr);
            if (method == null) throw new Exception($"method {name} does not exist on type {_type.FullName}");
            if (method.IsStatic) throw new Exception($"static method {name} does not require instance to be invoked");
            return method.Invoke(instance, paramArr);
        }

        public object? GetProperty(string name)
        {
            var property = EnsurePropertyIsAvailable(_type.GetProperty(name));
            if (property == null) throw new Exception($"property {name} does not exist on type {_type.FullName}");
            var get = EnsureMethodIsAvailable(property.GetGetMethod());
            if (get == null) throw new Exception($"property {name} has no defined get accessor");
            if (!get.IsStatic) throw new Exception($"nonstatic property {name} requires instance to be invoked");
            return get.Invoke(_type, new object?[] { });
        }

        public object? GetProperty(string name, object? instance)
        {
            var property = EnsurePropertyIsAvailable(_type.GetProperty(name));
            if (property == null) throw new Exception($"property {name} does not exist on type {_type.FullName}");
            var get = EnsureMethodIsAvailable(property.GetGetMethod());
            if (get == null) throw new Exception($"property {name} has no defined get accessor");
            if (get.IsStatic) throw new Exception($"static property {name} does not require instance to be invoked");
            return get.Invoke(instance, new object?[] { });
        }

        public void SetProperty(string name, object? value)
        {
            var property = EnsurePropertyIsAvailable(_type.GetProperty(name));
            if (property == null) throw new Exception($"property {name} does not exist on type {_type.FullName}");
            var set = EnsureMethodIsAvailable(property.GetSetMethod());
            if (set == null) throw new Exception($"property {name} has no defined get accessor");
            if (!set.IsStatic) throw new Exception($"nonstatic property {name} requires instance to be invoked");
            set.Invoke(_type, new object?[] { value });
        }

        public void SetProperty(string name, object? instance, object? value)
        {
            var property = EnsurePropertyIsAvailable(_type.GetProperty(name));
            if (property == null) throw new Exception($"property {name} does not exist on type {_type.FullName}");
            var set = EnsureMethodIsAvailable(property.GetGetMethod());
            if (set == null) throw new Exception($"property {name} has no defined set accessor");
            if (set.IsStatic) throw new Exception($"static property {name} does not require instance to be invoked");
            set.Invoke(instance, new object?[] { value });
        }

        public object? GetField(string name)
        {
            var field = _type.GetField(name);
            if (field == null) throw new Exception($"type {_type.FullName} does not contain field named {name}");
            
            if (!field.IsStatic) throw new Exception($"nonstatic field {name} requires instance to be retrieved");
            return field.GetValue(_type);
        }

        public object? GetField(string name, object? instance)
        {
            var field = _type.GetField(name);
            if (field == null) throw new Exception($"type {_type.FullName} does not contain field named {name}");

            if (field.IsStatic) throw new Exception($"static field {name} does not require instance to be retrieved");
            return field.GetValue(instance);
        }

        public void SetField(string name, object? value)
        {
            var field = _type.GetField(name);
            if (field == null) throw new Exception($"type {_type.FullName} does not contain field named {name}");

            if (!field.IsStatic) throw new Exception($"nonstatic field {name} requires instance to be set");
            field.SetValue(_type, value);
        }

        public void SetField(string name, object? instance, object? value)
        {
            var field = _type.GetField(name);
            if (field == null) throw new Exception($"type {_type.FullName} does not contain field named {name}");

            if (field.IsStatic) throw new Exception($"static field {name} does not require instance to be set");
            field.SetValue(instance, value);
        }

        private MethodInfo? GetMethod(string name, object?[] paramArr)
        {
            try
            {
                return EnsureMethodIsAvailable(_type.GetMethod(name));
            } catch(AmbiguousMatchException)
            {
                return GetMethodUsingTypes(name, paramArr);
            }
        }

        private MethodInfo? GetMethodUsingTypes(string name, object?[] paramArr)
        {
            return EnsureMethodIsAvailable(_type.GetMethod(name, paramArr.Select(p => p != null? p.GetType() : throw new Exception($"method name {name} is ambigious due inability to determine type of arguments provided")).ToArray()));
        }

        private MethodInfo? EnsureMethodIsAvailable(MethodInfo? methodInfo)
        {
            if (methodInfo == null) return null;
            if (RuntimeContext.RuntimeSettings.IncludeOnlyExports && !Attribute.IsDefined(methodInfo, typeof(JmyExportAttribute)))
                throw new Exception($"method {methodInfo.Name} cannot be called as it is not marked for export");
            if (Attribute.IsDefined(methodInfo, typeof(JmyNoExportAttribute)))
                throw new Exception($"method {methodInfo.Name} is marked as NoExport and cannot be called");
            return methodInfo;
        }

        private PropertyInfo? EnsurePropertyIsAvailable(PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null) return null;
            if (RuntimeContext.RuntimeSettings.IncludeOnlyExports && !Attribute.IsDefined(propertyInfo, typeof(JmyExportAttribute)))
                throw new Exception($"property {propertyInfo.Name} cannot be referenced as it is not marked for export");
            if (Attribute.IsDefined(propertyInfo, typeof(JmyNoExportAttribute)))
                throw new Exception($"property {propertyInfo.Name} is marked as NoExport and cannot be referenced");
            return propertyInfo;
        }
    }
}
