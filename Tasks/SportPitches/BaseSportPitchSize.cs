using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Device.Location;
using System.IO;
using System.Numerics;

namespace Sandbox.Tasks
{
	public class Sport
	{
		/// <summary>
		/// Dimensions of a typical pitch for this sport. Meters. Longest dimensions should be first (X).
		/// </summary>
		public Vector2 PitchDimensions;
	}

	public static class Sports
	{
		public static readonly Dictionary<string, Sport> Data = new Dictionary<string, Sport>()
		{
			{
				"tennis", new Sport()
				{
					PitchDimensions = new Vector2(23.77f, 10.97f)
				}
			},
			{
				"pickleball", new Sport()
				{
					PitchDimensions = new Vector2(13.4112f, 6.096f)
				}
			},
			{
				"basketball", new Sport()
				{
					PitchDimensions = new Vector2(28, 15)
				}
			},
		};
	}

	/// <summary>
	/// Filters sport pitch areas from geojson data based on their dimensions.
	/// </summary>
	public abstract class BaseSportPitchSize : BaseTask
	{
		protected FeatureCollection outputCollection = new FeatureCollection();

		public override Command CreateSubcommand()
		{
			Command command = base.CreateSubcommand();
			command.Options.Add(new Option<FileInfo>(ARG_IN)
			{
				Description = "Path to an input GeoJSON file or directory of files containing unfiltered areas.",
				Required = true
			});
			command.Options.Add(new Option<FileInfo>(ARG_OUT)
			{
				Description = "Path to write an output GeoJSON file containing the filtered areas."
			});
			return command;
		}

		protected override int Execute(ParseResult args)
		{
			outputCollection.type = "FeatureCollection";
			outputCollection.timestamp = DateTime.Now.ToString("O");

			FileInfo inFile = args.GetValue<FileInfo>(ARG_IN);
			if (inFile.Exists)
			{
				ProcessFile(inFile);
			}
			else if (Directory.Exists(inFile.FullName))
			{
				foreach (string file in Directory.GetFiles(inFile.FullName, "*.geojson", SearchOption.TopDirectoryOnly))
				{
					ProcessFile(new FileInfo(file));
				}
			}

			FileInfo outFile = args.GetValue<FileInfo>(ARG_OUT);
			if (outFile == null)
			{
				outFile = new FileInfo(Path.Combine(inFile.Directory.FullName, Path.GetFileNameWithoutExtension(inFile.Name) + "_OUT.geojson"));
			}

			string outJson = JsonConvert.SerializeObject(outputCollection);
			File.WriteAllText(outFile.FullName, outJson);
			return 0;
		}

		private void ProcessFile(FileInfo file)
		{
			string json = File.ReadAllText(file.FullName);
			FeatureCollection inFeatureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json);

			foreach (Feature feature in inFeatureCollection.features)
			{
				if (feature.geometry.type != "Polygon")
				{
					ConsoleUtility.WriteLine(ConsoleColor.Red, "{0}: Unrecognized geometry type '{0}'.", feature.id, feature.geometry.type);
					continue;
				}

				if (feature.geometry.coordinates.Length != 1)
				{
					ConsoleUtility.WriteLine(ConsoleColor.Red, "{0}: Too many coordinate sets.", feature.id);
					continue;
				}

				if (feature.geometry.coordinates[0].Length != 5)
				{
					ConsoleUtility.WriteLine(ConsoleColor.Red, "{0}: Too many coordinates.", feature.id);
					continue;
				}

				GeoCoordinate[] coordinates = feature.geometry.GetCoordinates(0);
				if (coordinates[0] != coordinates[coordinates.Length - 1])
				{
					ConsoleUtility.WriteLine(ConsoleColor.Red, "{0}: Geometry is not a closed loop.", feature.id);
					continue;
				}

				// calculate dimensions
				Vector2 dim1 = new Vector2(
					(float)coordinates[0].GetDistanceTo(coordinates[1]),
					(float)coordinates[1].GetDistanceTo(coordinates[2]));
				Vector2 dim2 = new Vector2(
					(float)coordinates[2].GetDistanceTo(coordinates[3]),
					(float)coordinates[3].GetDistanceTo(coordinates[4]));

				if (dim1.Y > dim1.X)
				{
					(dim1.X, dim1.Y) = (dim1.Y, dim1.X);
					(dim2.X, dim2.Y) = (dim2.Y, dim2.X);
				}

				if (Math.Abs(dim1.X - dim2.X) > 1 || Math.Abs(dim1.Y - dim2.Y) > 1)
				{
					ConsoleUtility.WriteLine(ConsoleColor.Red, "{0}: Geometry is not rectangular.", feature.id);
					continue;
				}

				//ConsoleUtility.WriteLine(ConsoleColor.White, "{0}", feature.id);

				Vector2 dim = new Vector2(dim1.X + dim2.X, dim1.Y + dim2.Y) / 2f;
				ProcessArea(feature, dim);
			}
		}

		protected abstract void ProcessArea(Feature feature, Vector2 dimensions);
	}
}
