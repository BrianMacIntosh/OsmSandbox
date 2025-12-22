using Sandbox.Tasks;
using System;
using System.CommandLine;

namespace Sandbox
{
	internal class Program
	{
		static int Main(string[] args)
		{
			RootCommand rootCommand = new RootCommand("Sandbox application for manipulating OpenStreetMap data.");
			foreach (Type type in typeof(Program).Assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(BaseTask)) && !type.IsAbstract)
				{
					BaseTask previewTask = (BaseTask)Activator.CreateInstance(type);
					Command taskCommand = previewTask.CreateSubcommand();
					rootCommand.Subcommands.Add(taskCommand);
				}
			}

			ParseResult parsedArgs = rootCommand.Parse(args);
			int code = parsedArgs.Invoke();
			ConsoleUtility.WriteLine(ConsoleColor.Cyan, "Done");
			Console.ReadLine();
			return code;
		}
	}
}
