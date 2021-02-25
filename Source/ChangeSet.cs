using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KerbalChangelog
{
	/// <summary>
	/// Represents the changes for one version of a mod
	/// </summary>
	public class ChangeSet : IComparable
	{
		/// <summary>
		/// Initialize a change set given version info and some changes
		/// </summary>
		/// <param name="v">The mod version of this change set</param>
		/// <param name="ch">The changes in this relase</param>
		public ChangeSet(ChangelogVersion v, List<Change> ch)
		{
			version = v;
			changes = ch;
		}

		/// <summary>
		/// Initialize a change set based on a config node
		/// </summary>
		/// <param name="vn">Config node for this change set</param>
		/// <param name="cfgDirName">Path where we found this file</param>
		public ChangeSet(ConfigNode vn, string cfgDirName)
		{
			//TryGetValue returns true if the node exists, and false if the node doesn't
			//By negating the value it allows you to catch bad TryGets
			string _version = "";
			if (!vn.TryGetValue("version", ref _version))
			{
				Debug.Log("[KCL] Badly formatted version in directory " + cfgDirName);
				_version = "null";
			}
			string _versionName = null;
			vn.TryGetValue("versionName", ref _versionName);
			string _versionDate = null;
			vn.TryGetValue("versionDate", ref _versionDate);
			string _versionKSP = null;
			vn.TryGetValue("versionKSP", ref _versionKSP);

			version = new ChangelogVersion(_version, cfgDirName, _versionName, _versionDate, _versionKSP);

			//loads change fields (needed for backwards compatibility
			foreach (string change in vn.GetValues("change"))
			{
				changes.Add(new Change(change, new List<string>()));
			}
			//loads change nodes
			foreach (ConfigNode chn in vn.GetNodes("CHANGE"))
			{
				changes.Add(new Change(chn, cfgDirName));
			}
			changes = changes.OrderBy(o => o.type).ToList();
		}

		/// <summary>
		/// Version info for this release
		/// </summary>
		public ChangelogVersion version { get; private set; }

		/// <returns>
		/// A nice string representation of this change set
		/// </returns>
		public override string ToString()
		{
			string ret = $"<b><size=20>{version}</size></b>\n";
			Change prev = null;

			ChangeType? curType = null;

			foreach (Change c in changes)
			{
				if (c.type != curType)
				{
					if (c.type != ChangeType.None)
					{
						var typeStr = c.type == ChangeType.HighPriority
							? "High Priority"
							: c.type.ToString();
						ret += $"<b>{typeStr}</b>\n";
					}
					curType = c.type;
				}
				ret += c.ToString();
				prev = c;
			}
			return ret;
		}

		/// <summary>
		/// Sort change sets by version
		/// </summary>
		/// <param name="obj">Another change set</param>
		/// <returns>
		/// -1 if this&lt;obj, 1 if this&gt;obj, 0 if this==obj
		/// </returns>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			if (obj is ChangeSet cs)
			{
				return version.CompareTo(cs.version);
			}
			throw new ArgumentException("Object is not a ChangeSet");

		}

		private List<Change> changes = new List<Change>();
	}
}
