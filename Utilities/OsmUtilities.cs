using OsmSharp;
using OsmSharp.Changesets;
using OsmSharp.Complete;
using OsmSharp.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sandbox
{
	public static class OsmUtilities
	{
		/// <summary>
		/// Removes the specified node, transfering all of its information into the way.
		/// </summary>
		/// <param name="change">Change object to put the change into.</param>
		/// <param name="node"></param>
		/// <param name="way"></param>
		/// <remarks>Based on iD's merge operation (https://github.com/openstreetmap/iD/blob/b8c11a2b9cec5affc8766a3491a0baa49e1f032b/modules/actions/merge.js#L5)</remarks>
		public static void MergeNodeToWay(OsmChange change, OsmGeo node, CompleteWay way)
		{
			// check if node is already deleted
			OsmGeo[] delete = change.Delete;
			if (delete.Contains(node))
			{
				throw new ArgumentException(string.Format("Node {0} already deleted in change.", node.Id));
			}

			// merge tags
			CopyTags(node.Tags, way.Tags);

			// check that node is not part of any relations
			//TODO:

			// if node is new, no need to try to preserve it
			if (!node.Id.HasValue || node.Id < 0)
			{
				change.Delete = delete.Append(node).ToArray();
				return;
			}

			Node replaceNode = null;

			if (replaceNode == null)
			{
				// try to find an empty way node to replace
				replaceNode = way.Nodes.Where(pnode => pnode.Tags.Count == 0).FirstOrDefault();
			}

			if (replaceNode == null)
			{
				// try to find an uninteresting way node to replace
				replaceNode = way.Nodes.Where(pnode => IsUninterestingNode(pnode)).FirstOrDefault();
			}

			if (replaceNode == null)
			{
				// try to find a newer node to replace
				replaceNode = way.Nodes.Where(pnode => pnode.Id < node.Id).FirstOrDefault();
			}

			// make replacement
			//TODO:

			// delete extra node
			//change.Delete = delete.Append().ToArray();
		}

		/// <summary>
		/// Merges the tags in the 'from' collection into the 'to' collection.
		/// </summary>
		public static void CopyTags(TagsCollectionBase from, TagsCollectionBase to)
		{
			foreach (Tag fromTag in from)
			{
				if (to.TryGetValue(fromTag.Key, out string toValue))
				{
					if (fromTag.Value == toValue)
					{
						// alerady same value; skip
					}
					else
					{
						// differing values, combine
						string[] fromValues = fromTag.Value.Split(';');
						string[] toValues = toValue.Split(';');
						List<string> newValues = new List<string>(toValues);
						foreach (string fromValue in fromValues)
						{
							if (!newValues.Contains(fromValue))
							{
								newValues.Add(fromValue);
							}
						}
						to[fromTag.Key] = string.Join(";", newValues);
					}
				}
				else
				{
					to.Add(fromTag);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Sourced from iD editor.</remarks>
		private static readonly HashSet<string> uninterestingKeys = new HashSet<string>() {
			"attribution",
			"created_by",
			"import_uuid",
			"geobase:datasetName",
			"geobase:uuid",
			"KSJ2:curve_id",
			"KSJ2:lat",
			"KSJ2:long",
			"lat",
			"latitude",
			"lon",
			"longitude",
			"source",
			"source_ref",
			"odbl",
			"odbl:note"
		};
		private static readonly Regex uninterestingKeyRegex = new Regex(@"^(source(_ref)?|tiger):");

		/// <summary>
		/// Returns true if the node has no "interesting" tags on it.
		/// </summary>
		public static bool IsUninterestingNode(Node node)
		{
			return !node.Tags
				.Where(tag => !uninterestingKeys.Contains(tag.Key) && !uninterestingKeyRegex.IsMatch(tag.Key))
				.Any();
		}
	}
}
