using OsmSharp;
using OsmSharp.Changesets;
using OsmSharp.IO.Xml;
using OsmSharp.Streams;
using System.CommandLine;
using System.IO;

namespace Sandbox.Tasks
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>Make sure input file has all relations involved with any of the way nodes.</remarks>
	public class NodeAreaMerge : BaseTask
	{
		/*
		 * Generate source files with:
{{geocodeArea:Los Angeles, CA}}->.searchArea;
way(area.searchArea)[amenity=school];
foreach->.it
{
	node(area.it)[amenity=school];
	if(_.count(nodes)>0)
    {
      .it out body;
      ._ out body;
    }
}
		 */

		public override Command CreateSubcommand()
		{
			Command command = base.CreateSubcommand();
			command.Description = "Merges school nodes and ways if they are determined to match.";
			command.Options.Add(new Option<FileInfo>(ARG_IN)
			{
				Description = "Path to an input GeoJSON file containing school ways and school nodes.",
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

				OsmGeo current = source.Current();
			}

			// node merge to way: https://github.com/openstreetmap/iD/blob/b8c11a2b9cec5affc8766a3491a0baa49e1f032b/modules/actions/merge.js#L5

			FileInfo outFile = args.GetValue<FileInfo>(ARG_OUT);
			string outXml = change.SerializeToXml();
			File.WriteAllText(outFile.FullName, outXml);
			return 0;
		}
	}
}
