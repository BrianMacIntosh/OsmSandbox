using OsmSharp;
using OsmSharp.Changesets;
using OsmSharp.IO.Xml;
using OsmSharp.Streams;
using OsmSharp.Tags;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;

namespace Sandbox.Tasks
{
	/// <summary>
	/// Adds tags to specified objects.
	/// </summary>
	public class AddTags : BaseTask
	{
		/*
{{geocodeArea:California}}->.searchArea;
node(area.searchArea)[man_made=surveillance]["contact:webcam"~"dot.ca.gov"]["surveillance:zone"=traffic][!"operator:wikidata"];
out meta;
		 */

		private static readonly List<Tag> Tags = new List<Tag>()
		{
			new Tag("operator", "California Department of Transportation"),
			new Tag("operator:short", "Caltrans"),
			new Tag("operator:type", "government"),
			new Tag("operator:wikidata", "Q127743"),
		};

		public override Command CreateSubcommand()
		{
			Command command = base.CreateSubcommand();
			command.Description = "Adds tags to specified objects.";
			command.Options.Add(new Option<FileInfo>(ARG_IN)
			{
				Description = "Path to an input OSM JSON file containing the objects to alter.",
				Required = true
			});
			command.Options.Add(new Option<FileInfo>(ARG_OUT)
			{
				Description = "Path to write an output OSMChange file."
			});
			return command;
		}

		protected override int Execute(ParseResult args)
		{
			OsmChange change = new OsmChange();
			change.Version = 0.6;

			FileInfo inFile = args.GetValue<FileInfo>(ARG_IN);
			using (FileStream inStream = File.OpenRead(inFile.FullName))
			{
				XmlOsmStreamSource source = new XmlOsmStreamSource(inStream);
				List<OsmGeo> modify = new List<OsmGeo>();
				foreach (OsmGeo geo in source)
				{
					foreach (Tag tag in Tags)
					{
						if (geo.Tags.TryGetValue(tag.Key, out string value))
						{
							ConsoleUtility.WriteLine(System.ConsoleColor.Yellow, "{0}->{1}", value, tag.Value);
							geo.Tags[tag.Key] = tag.Value;
						}
						else
						{
							geo.Tags.Add(tag);
						}
					}
					modify.Add(geo);
				}
				change.Modify = modify.ToArray();
			}

			FileInfo outFile = args.GetValue<FileInfo>(ARG_OUT);
			string outXml = change.SerializeToXml();
			File.WriteAllText(outFile.FullName, outXml);
			return 0;
		}
	}
}
