using Sandbox.Tasks;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Device.Location;
using System.Numerics;

namespace OpenStreetMapSandbox.Tasks
{
	/// <summary>
	/// Task that filters the input features down to ones that look misidentified.
	/// </summary>
	internal class FilterMisidentifiedCourts : BaseSportPitchSize
	{
		/// <summary>
		/// Areas within this range (in meters) of another problem area will be lumped into one task.
		/// </summary>
		private const double deduplicateRange = 100.0;

		/// <summary>
		/// In meters.
		/// </summary>
		private const float maxOtherTypeError = 1f;

		private List<GeoCoordinate> loggedProblemSpots = new List<GeoCoordinate>();

		public override Command CreateSubcommand()
		{
			Command command = base.CreateSubcommand();
			command.Description = "Filters the input features down to ones that look misidentified.";
			return command;
		}

		protected override void ProcessArea(Feature feature, Vector2 dimensions)
		{
			//TODO: make more generic
			Sport pickleball = Sports.Data["pickleball"];

			if (Math.Abs(dimensions.Y - pickleball.PitchDimensions.Y) < maxOtherTypeError
				&& Math.Abs(dimensions.X - pickleball.PitchDimensions.X) < maxOtherTypeError)
			{
				ConsoleUtility.WriteLine(ConsoleColor.Cyan, "{0}: looks like a pickleball court", feature.id);
			
				GeoCoordinate thisCoordinate = feature.geometry.GetCoordinate(0, 0);
			
				// check if a nearby problem is already logged
				bool bDupeInRange = false;
				foreach (GeoCoordinate alreadyLogged in loggedProblemSpots)
				{
					if (alreadyLogged.GetDistanceTo(thisCoordinate) < deduplicateRange)
					{
						bDupeInRange = true;
						break;
					}
				}
			
				if (!bDupeInRange)
				{
					loggedProblemSpots.Add(thisCoordinate);
					outputCollection.features.Add(feature);
				}
				return;
			}
		}
	}
}
