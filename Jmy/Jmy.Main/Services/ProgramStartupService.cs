using CliParser;
using Jmy.Engine;
using Jmy.Interpreter;
using Jmy.Main.Providers;
using Jmy.Parser;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.Main.Services
{
    [Entry("Jmy.Main")]
    internal class ProgramStartupService
    {
        public ProgramStartupService()
        {
            _interpreter = new DefaultInterpreter(_runtimeContext);
        }
        private DefaultParser _parser = new DefaultParser();
        private RuntimeContext _runtimeContext = new RuntimeContext();
        private DefaultInterpreter _interpreter;

        [Command("run")]
        public void Run(string runFile = "", bool suppressLogging = false, bool noStdLib = false, bool minimalRuntime = false, bool enableReflection = true)
        {
            CliLogger.LoggingEnabled = !suppressLogging;
            SetupContext(noStdLib, minimalRuntime, enableReflection);
            if (string.IsNullOrWhiteSpace(runFile)) {
                Repl();
                return;
            }
            try
            {
                foreach (var statement in _parser.Parse(File.ReadAllText(runFile)))
                {
                    _interpreter.Accept(statement);
                }
            }
            catch (Exception ex)
            {
                CliLogger.LogError(ex.ToString());
            }
        }

        private void SetupContext(bool noStdLib, bool minimalRuntime, bool enableReflection)
        {
            _runtimeContext = new RuntimeContext();

            if (enableReflection)
            {
                _runtimeContext.StoreValue("_thiscontext_", _runtimeContext);
            }
            if (!minimalRuntime)
            {
                _runtimeContext.Register<int>();
                _runtimeContext.Register<string>();
                _runtimeContext.Register<bool>();
                _runtimeContext.Register<char>();
                _runtimeContext.Register<Array>();
#if DEBUG
                _runtimeContext.Register(typeof(DebugUtilities));
#endif
            }
            if (!noStdLib && !_runtimeContext.TryRegisterAssembly("Jmy.StdLib.dll"))
            {
                CliLogger.LogWarning("WARN: Unable to load standard library. Functionality will be limited.");
            }

            _interpreter = new DefaultInterpreter(_runtimeContext);
            _parser = new DefaultParser(_runtimeContext);
        }
        private void Repl()
        {
            bool finished = false;
            while (!finished)
            {
                Console.Write(".$ ");
                string? script = Console.ReadLine();
                if (script == null) continue;
                if (script.Trim().ToLower() == "exit") { finished = true; break; }
               try
               {
                    foreach (var statement in _parser.Parse(script))
                    {
                        _interpreter.Accept(statement);
                    }
                } catch (Exception ex)
                {
                    CliLogger.LogError(ex.ToString());
                }
            }
        }

    }
}
