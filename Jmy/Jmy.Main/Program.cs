
using CliParser;
using Jmy.Main.Services;

namespace Jmy.Main
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var test = new string[] { "run" };
            test.Resolve(new ProgramStartupService());

        }
    }

}
