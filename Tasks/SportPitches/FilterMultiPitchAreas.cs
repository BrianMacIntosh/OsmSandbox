using Sandbox.Tasks;
using System;
using System.CommandLine;
using System.Numerics;

namespace OpenStreetMapSandbox.Tasks
{
	/// <summary>
	/// Task that filters the input features down to ones that appear much too large to be what they are tagged as.
	/// </summary>
	internal class FilterMultiPitchAreas : BaseSportPitchSize
	{
		private const float multiPitchAreaMultiplier = 2.5f;

		public override Command CreateSubcommand()
		{
			Command command = base.CreateSubcommand();
			command.Description = "Filters the input features down to ones that appear much too large to be what they are tagged as.";
			return command;
		}

		protected override void ProcessArea(Feature feature, Vector2 dimensions)
		{
			if (!feature.properties.TryGetValue("sport", out string sportKey))
			{
				ConsoleUtility.WriteLine(ConsoleColor.Red, "{0}: no sport tag", feature.id);
				return;
			}

			if (!Sports.Data.TryGetValue(sportKey, out Sport sport))
			{
				ConsoleUtility.WriteLine(ConsoleColor.Red, "{0}: unrecognized sport '{1}'", feature.id, sportKey);
				return;
			}

			if (dimensions.Y > sport.PitchDimensions.Y * multiPitchAreaMultiplier
				|| dimensions.X > sport.PitchDimensions.X * multiPitchAreaMultiplier)
			{
				ConsoleUtility.WriteLine(ConsoleColor.Cyan, "{0}: looks like a multi-pitch area", feature.id);
				outputCollection.features.Add(feature);
			}
		}
	}
}
