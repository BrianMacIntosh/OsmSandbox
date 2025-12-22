using System.CommandLine;

namespace Sandbox.Tasks
{
	public abstract class BaseTask
	{
		protected const string ARG_IN = "--in";
		protected const string ARG_OUT = "--out";

		/// <summary>
		/// Creates and returns a <see cref="Command"/> representing this task.
		/// </summary>
		public virtual Command CreateSubcommand()
		{
			Command command = new Command(GetType().Name);
			command.SetAction((ParseResult args) =>
			{
				return Execute(args);
			});
			return command;
		}

		/// <summary>
		/// Runs the task logic.
		/// </summary>
		protected abstract int Execute(ParseResult args);
	}
}
