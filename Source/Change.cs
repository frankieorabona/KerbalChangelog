using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace KerbalChangelog
{
	/// <summary>
	/// Represents one change from a release
	/// </summary>
	public class Change
	{
		/// <summary>
		/// Whether it's a fix, enhancement, etc.
		/// </summary>
		public ChangeType type { get; private set; } = ChangeType.None;

		/// <summary>
		/// Initialize a simple change with one description
		/// </summary>
		/// <param name="c">The description of this change</param>
		public Change(string c)
		{
			change = c;
		}

		/// <summary>
		/// Initialize a change with a description and some sub-changes
		/// </summary>
		/// <param name="c">Overall description of the change</param>
		/// <param name="sc">Descriptions of the sub-changes</param>
		public Change(string c, List<string> sc) : this(c)
		{
			subchanges = sc;
		}

		/// <summary>
		/// Initialize a change from a config node
		/// </summary>
		/// <param name="chn">The config node from the cfg file</param>
		/// <param name="cfgDirName">Path of the cfg file for logging purposes</param>
		public Change(ConfigNode chn, string cfgDirName)
		{
			if (!chn.TryGetValue("change", ref change))
			{
				Debug.Log("[KCL] Could not find a needed change field in directory " + cfgDirName);
			}
			string changeType = "";
			if (chn.TryGetValue("type", ref changeType))
			{
				switch (changeType.Substring(0, 1).ToUpper())
				{
					case "A":
						type = ChangeType.Add;
						break;
					case "C":
						type = ChangeType.Change;
						break;
					case "D":
						type = ChangeType.Deprecate;
						break;
					case "R":
						type = ChangeType.Remove;
						break;
					case "F":
						type = ChangeType.Fix;
						break;
					case "S":
						type = ChangeType.Security;
						break;
					case "H":
						type = ChangeType.HighPriority;
						break;
					default:
						type = ChangeType.None;
						break;
				}
			}
			else
			{
				type = ChangeType.None;
			}
			foreach (string sc in chn.GetValues("subchange"))
			{
				subchanges.Add(sc);
			}
		}

		/// <returns>
		/// Localized string description of the type of this change
		/// </returns>
		public string TypeString()
		{
			switch (type)
			{
				case ChangeType.HighPriority:
					return Localizer.Format("KerbalChangelog_ChangeTypeHighPriority");
				case ChangeType.Add:
					return Localizer.Format("KerbalChangelog_ChangeTypeAdd");
				case ChangeType.Change:
					return Localizer.Format("KerbalChangelog_ChangeTypeChange");
				case ChangeType.Deprecate:
					return Localizer.Format("KerbalChangelog_ChangeTypeDeprecate");
				case ChangeType.Remove:
					return Localizer.Format("KerbalChangelog_ChangeTypeRemove");
				case ChangeType.Fix:
					return Localizer.Format("KerbalChangelog_ChangeTypeFix");
				case ChangeType.Security:
					return Localizer.Format("KerbalChangelog_ChangeTypeSecurity");
				case ChangeType.Performance:
					return Localizer.Format("KerbalChangelog_ChangeTypePerformance");
				case ChangeType.None:
					return Localizer.Format("KerbalChangelog_ChangeTypeNone");
				default:
					return "";
			}
		}

		/// <returns>
		/// A nice string representation of this change
		/// </returns>
		public override string ToString()
		{
			string ret = "";
			ret += " * " + change + "\n";
			foreach (string sc in subchanges)
			{
				//6 spaces ought to look good (or it does to me)
				ret += "   " + "   " + "- " + sc + "\n";
			}
			return ret;
		}

		private string       change     = "";
		private List<string> subchanges = new List<string>();
	}
}
