using Jmy.Core.Attributes;
using Jmy.Engine;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Main.Providers
{
    [JmyExport]
    public static class DebugUtilities
    {
        [JmyExport]
        public static void ShowRegisteredClasses(RuntimeContext context)
        {
            var klasses = context.GetRegisteredClasses();
            foreach (var klass in klasses)
            {
                CliLogger.LogInfo(klass);
            }
        }
        [JmyExport]
        public static void Show(RuntimeContext context)
        {
            var values = context.GetStoredValues().OrderBy(x => x);
            foreach (var kv in values)
            {
                CliLogger.LogInfo($"{kv.Item1} := {(kv.Item2?.GetType().FullName ?? kv.Item2?.GetType().Name ?? "null")}");
            }
        }
    }
}
