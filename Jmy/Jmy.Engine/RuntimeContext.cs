using Jmy.Core.Attributes;
using Jmy.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Engine
{
    public class RuntimeContext
    {
        public static class RuntimeSettings
        {
            public static bool IncludeOnlyExports { get; set; } = false;
        }
        class InvokableMethodInfo
        {
            public MethodInfo MethodInfo { get; set; }
            public object? Instance { get; set; }

            public object? Invoke(object?[] paramArr)
            {
                return MethodInfo.Invoke(Instance, paramArr);
            }
            public InvokableMethodInfo(object? instance, MethodInfo methodInfo)
            {
                MethodInfo = methodInfo;
                Instance = instance;
            }
        }

        public RuntimeContext()
        {
            RegisterMacro("CreateObject", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.CreateObject)) ?? throw new ArgumentNullException("CreateObject"));
            RegisterMacro("Invoke", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.Invoke)) ?? throw new ArgumentNullException("Invoke"));
            RegisterMacro("StaticInvoke", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.StaticInvoke)) ?? throw new ArgumentNullException("StaticInvoke"));
            RegisterMacro("GetProperty", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.GetProperty)) ?? throw new ArgumentNullException("GetProperty"));
            RegisterMacro("GetStaticProperty", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.GetStaticProperty)) ?? throw new ArgumentNullException("GetStaticProperty"));
            RegisterMacro("SetProperty", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.SetProperty)) ?? throw new ArgumentNullException("SetProperty"));
            RegisterMacro("SetStaticProperty", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.SetStaticProperty)) ?? throw new ArgumentNullException("SetStaticProperty"));
            RegisterMacro("GetField", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.GetField)) ?? throw new ArgumentNullException("GetField"));
            RegisterMacro("GetStaticField", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.GetStaticField)) ?? throw new ArgumentNullException("GetStaticField"));
            RegisterMacro("SetField", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.SetField)) ?? throw new ArgumentNullException("SetField"));
            RegisterMacro("SetStaticField", this, typeof(RuntimeContext).GetMethod(nameof(RuntimeContext.SetStaticField)) ?? throw new ArgumentNullException("SetStaticField"));
            Register(typeof(Console));
        }

        private Dictionary<string, List<ClassDef>> _definitions = new Dictionary<string, List<ClassDef>>();
        private Dictionary<string, InvokableMethodInfo> _macros = new Dictionary<string, InvokableMethodInfo>();
        private Dictionary<string, object?> _runtimeVariables = new Dictionary<string, object?>();


        #region ExposedMacros
        public object? CreateObject(string klass, object?[] paramArr)
        {
            var klassDef = GetClassDefinition(klass);
            return klassDef.Instantiate(paramArr);
        }

        public object? Invoke(object? instance, string method, object?[] paramArr)
        {
            if (instance == null) throw new Exception($"unable to invoke method {method} on null valued object");
            var klassDef = GetClassDefinition(instance.GetType());
            return klassDef.InvokeMember(method, instance, paramArr);
        }

        public object? StaticInvoke(string klass, string method, object?[] paramArr)
        {
            var klassDef = GetClassDefinition(klass);
            return klassDef.InvokeMember(method, paramArr);
        }

        public object? GetProperty(object? instance, string property)
        {
            if (instance == null) throw new Exception($"unable to access property {property} on null valued object");
            var klassDef = GetClassDefinition(instance.GetType());
            return klassDef.GetProperty(property, instance);
        }

        public object? GetStaticProperty(string klass, string property)
        {
            var klassDef = GetClassDefinition(klass);
            return klassDef.GetProperty(property);
        }

        public object? GetField(object? instance, string field)
        {
            if (instance == null) throw new Exception($"unable to access field {field} on null valued object");
            var klassDef = GetClassDefinition(instance.GetType());
            return klassDef.GetField(field, instance);
        }

        public object? GetStaticField(string klass, string field)
        {
            var klassDef = GetClassDefinition(klass);
            return klassDef.GetField(field);
        }

        public void SetProperty(object? instance, string property, object? value)
        {
            if (instance == null) throw new Exception($"unable to set property {property} on null valued object");
            var klassDef = GetClassDefinition(instance.GetType());
            klassDef.SetProperty(property, instance, value);
        }

        public void SetStaticProperty(string klass, string property, object? value)
        {
            var klassDef = GetClassDefinition(klass);
            klassDef.SetProperty(property, value);
        }

        public void SetField(object? instance, string field, object? value)
        {
            if (instance == null) throw new Exception($"unable to set field {field} on null valued object");
            var klassDef = GetClassDefinition(instance.GetType());
            klassDef.SetField(field, instance, value);
        }

        public void SetStaticField(string klass, string field, object? value)
        {
            var klassDef = GetClassDefinition(klass);
            klassDef.SetField(field, value);
        }

        #endregion

        public List<string> GetRegisteredClasses()
        {
            return _definitions.Keys.ToList();
        }

        public List<string> GetRegisteredMacros()
        {
            return _macros.Keys.ToList();
        }

        public List<(string, object?)> GetStoredValues()
        {
            return _runtimeVariables.Select(kv => (kv.Key, kv.Value)).ToList();
        }

        public bool TryRegisterAssembly(string pathToAssembly)
        {
            if (!File.Exists(pathToAssembly)) return false;
            try
            {
                RegisterAssembly(pathToAssembly);
                return true;
            }
            catch (Exception) 
            {
                return false; 
            }
        }

        public void RegisterAssembly(string pathToAssembly)
        {
            var asm = Assembly.LoadFrom(pathToAssembly);
            if (asm == null) throw new Exception($"unable to load assembly from path {pathToAssembly}");
            var types2 = asm.GetTypes();
            var types = asm.GetTypes().Where(ty => RuntimeSettings.IncludeOnlyExports? ty.IsDefined(typeof(JmyExportAttribute)) : true);
            foreach (var type in types)
            {
                AddClassDefinition(type);
            }
        }

        public void Register<Ty>()
        {
            Register(typeof(Ty));
        }

        public void Register(Type type)
        {
            if (!type.IsDefined(typeof(JmyNoExportAttribute)))
            {
                if (!RuntimeSettings.IncludeOnlyExports || type.IsDefined(typeof(JmyExportAttribute)))
                    AddClassDefinition(type);
            }
        }

        public object? InvokeMacro(string name, object?[] paramArr)
        {
            if (!_macros.TryGetValue(name, out var macro) || macro == null)
                throw new Exception($"macro {name} is not defined");
            var index = macro.MethodInfo.GetParameters().ToList().FindIndex(p => p.ParameterType == typeof(object?[]));
            if (index >= 0)
            {

                var segmentedArgs = new ArraySegment<object?>(paramArr);
                var args = segmentedArgs.Slice(0, index);
                var variadicArgs = segmentedArgs.Slice(index, paramArr.Length - index);
                return macro.Invoke(args.Append(variadicArgs.ToArray()).ToArray());
            }

            return macro.Invoke(paramArr);
        }

        public void RegisterMacro<Ty>(string name, MethodInfo methodInfo) where Ty : class
        {
            _macros.Add(name, new InvokableMethodInfo(typeof(Ty), methodInfo));
        }

        public void RegisterMacro(string name, object? instance, MethodInfo methodInfo)
        {
            _macros.Add(name, new InvokableMethodInfo(instance, methodInfo));
        }

        public ClassDef GetClassDefinition(string name)
        {
            if (!_definitions.TryGetValue(name, out var result) || result == null || !result.Any())
                throw new Exception($"{name} is not a valid type");
            if (result.Count > 1) throw new Exception($"reference to type {name} is ambiguous. Please use fully qualified name");
            return result[0];
        }

        public ClassDef GetClassDefinition(Type type)
        {
            if (!_definitions.TryGetValue(type.AssemblyQualifiedName ?? type.FullName ?? type.Name, out var result) || result == null || !result.Any())
                throw new Exception($"{type.Name} is not a registered type");
            if (result.Count > 1) throw new Exception($"reference to type {type.Name} is ambiguous. Please use fully qualified name");
            return result[0];
        }

        public object? GetStoredValue(string name)
        {
            if (_runtimeVariables.TryGetValue(name, out var result)) return result;
            throw new Exception($"runtime variable {name} is not defined");
        }

        public void StoreValue(string name, object? value)
        {
            _runtimeVariables.Add(name, value);
        }

        private void AddClassDefinition(Type type)
        {
            var def = new ClassDef(type);
            AddToDefinitions(type.Name, def);
            AddToDefinitions(type.FullName, def);
            AddToDefinitions(type.AssemblyQualifiedName, def);
        }

        private void AddToDefinitions(string? key, ClassDef classDef)
        {
            if (key == null) return;
            if (_definitions.TryGetValue(key, out var defs) && defs != null && defs.Any())
            {
                if (!defs.Contains(classDef)) defs.Add(classDef);
                return;
            } 
            _definitions.Add(key, new List<ClassDef> { classDef });
        }

    }
}
