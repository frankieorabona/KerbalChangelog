using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalChangelog
{
	/// <summary>
	/// Represents an entire change log for all version of one mod
	/// </summary>
	public class Changelog
	{
		/// <summary>
		/// Initialize a change log given the mod name, whether to show, and the change sets
		/// </summary>
		/// <param name="mn">Name of the mod</param>
		/// <param name="cs">The changes for this mod</param>
		public Changelog(string mn, List<ChangeSet> cs)
		{
			modName    = mn;
			changeSets = cs;
		}

		/// <summary>
		/// Initialize a change log from a config node
		/// </summary>
		/// <param name="cn">The config node containing our change log</param>
		/// <param name="cfgDir">Something weird</param>
		public Changelog(ConfigNode cn, UrlDir.UrlConfig cfgDir)
		{
			string cfgDirName = cfgDir.url;
			string _modname = "";
			if (!cn.TryGetValue("modName", ref _modname))
			{
				Debug.Log("[KCL] Missing mod name for changelog file in directory: " + cfgDirName);
				Debug.Log("[KCL] Continuing using directory name as mod name...");
				modName = cfgDirName;
			}
			else
			{
				modName = _modname;
			}

			bool showChangelog = true;
			if (cn.TryGetValue("showChangelog", ref showChangelog) && !showChangelog)
			{
				// This will only be False if a previous version of KerbalChangelog displayed it
				alreadySeen = true;
			}

			string _author = "";
			if (cn.TryGetValue("author", ref _author))
			{
				author = _author;
			}
			string _license = "";
			if (cn.TryGetValue("license", ref _license))
			{
				license = _license;
			}
			string _website = "";
			cn.TryGetValue("website", ref _website); 
			webpage = _website;
			if (webpage != "")
			{
				webpageValid = ValidateWebsite(webpage);
			}

			foreach (ConfigNode vn in cn.GetNodes("VERSION"))
			{
				changeSets.Add(new ChangeSet(vn, cfgDirName));
			}
		}

		/// <summary>
		/// Name of the mod
		/// </summary>
		public string modName      { get; private set; }
		/// <summary>
		/// License of the mod
		/// </summary>
		public string license      { get; private set; } = null;
		/// <summary>
		/// Author of the mod
		/// </summary>
		public string author       { get; private set; } = null;
		/// <summary>
		/// Home page of the mod
		/// </summary>
		public string webpage      { get; private set; } = null;
		/// <summary>
		/// True if the home page is a valid URL, false otherwise
		/// </summary>
		public bool   webpageValid { get; private set; } = false;
		/// <summary>
		/// Whether a previous version of KerbalChangelog displayed this already
		/// </summary>
		public bool   alreadySeen  { get; private set; } = false;

		/// <summary>
		/// The versions of this mod in the changelog
		/// </summary>
		public IEnumerable<string> versions => changeSets.Select(cs => cs.version.ToStringPure());

		/// <summary>
		/// The latest version of the mod in the change log
		/// </summary>
		public ChangelogVersion highestVersion
		{
			get
			{
				changeSets.Sort();
				try
				{
					return changeSets[0].version;
				}
				catch (Exception)
				{
					Debug.Log("[KCL] No changesets exist.");
					return null;
				}
			}
		}

		/// <returns>
		/// Overall summary of this mod for display to the user
		/// </returns>
		public string Header()
		{
			var lines = new List<string> { $"<b><size=24>{modName}</size></b>" };
			if (!string.IsNullOrEmpty(author))
			{
				lines.Add($"Created by: {author}");
			}
			if (!string.IsNullOrEmpty(license))
			{
				lines.Add($"Licensed under the {license} license");
			}
			return string.Join("\n", lines);
		}

		/// <returns>
		/// A nice string representation of this change log
		/// </returns>
		/// <param name="versionsSeen">The versions the user has already seen, will be excluded from the display</param>
		public string Body(List<string> versionsSeen)
		{
			return string.Join("\n", changeSets
				.Where(cs => !versionsSeen.Contains(cs.version.ToStringPure()))
				.ToList());
		}

		/// <summary>
		/// Check if any of the versions haven't been shown yet
		/// </summary>
		/// <param name="versionsSeen">The versions the user has seen</param>
		/// <returns>
		/// true if any are unseen, false if all have been seen
		/// </returns>
		public bool HasUnseen(List<string> versionsSeen)
		{
			return changeSets.Any(cs => !versionsSeen.Contains(cs.version.ToStringPure()));
		}

		private bool ValidateWebsite(string url)
		{
			Debug.Log("Validating url: " + url);
			string uri = @"https://" + url;
			Debug.Log(uri);
			Uri siteuri = new Uri(uri);
			string site = siteuri.Host;

			if (validHosts.Contains(site))
			{
				return true;
			}
			return false;
		}

		private static readonly HashSet<string> validHosts = new HashSet<string>()
		{
			"github.com", 
			"forum.kerbalspaceprogram.com", 
			"kerbaltek.com", 
			"KerbalX.com", 
			"spacedock.info", 
			"kerbokatz.github.io", 
			"krpc.github.io", 
			"genhis.github.io", 
			"snjo.github.io",
			"www.curseforge.com",
			"ksp.sarbian.com"
		};

		private List<ChangeSet> changeSets = new List<ChangeSet>();
	}
}
