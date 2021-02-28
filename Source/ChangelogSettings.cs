using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace KerbalChangelog
{
	/// <summary>
	/// Represents this mod's settings. Since we run on the main menu,
	/// we can't be a ScenarioModule.
	/// </summary>
	public class ChangelogSettings
	{
		/// <summary>
		/// A factory method disguising a singleton
		/// </summary>
		/// <param name="db">Game database</param>
		/// <returns>
		/// Existing instance if exists, a new one otherwise
		/// </returns>
		public static ChangelogSettings Load(GameDatabase db)
		{
			if (Instance == null)
			{
				Instance = new ChangelogSettings(db);
			}
			return Instance;
		}

		/// <summary>
		/// Initialize the settings. Load if already defined in the game db,
		/// else start fresh.
		/// </summary>
		private ChangelogSettings(GameDatabase db)
		{
			foreach (UrlDir.UrlConfig cfg in db.GetConfigs(nodeName))
			{
				ConfigNode.LoadObjectFromConfig(this, cfg.config);
				saveFile = cfg.parent;
			}
			if (saveFile == null)
			{
				// No saved configs yet, use the default location
				saveFile = GetDefaultFile();
			}
		}

		private static ChangelogSettings Instance = null;

		/// <summary>
		/// Save settings to disk, overwriting if already there or
		/// creating otherwise.
		/// </summary>
		public void Save()
		{
			if (saveFile != null)
			{
				saveFile.configs.Clear();
				saveFile.configs.Add(new UrlDir.UrlConfig(
					saveFile,
					ConfigNode.CreateConfigFromObject(this, new ConfigNode(nodeName))
				));
				saveFile.SaveConfigs();
			}
		}

		/// <summary>
		/// If true, then start out on the summary listing when available,
		/// otherwise show the first change log.
		/// </summary>
		[Persistent]
		public bool defaultChangelogSelection = false;

		/// <summary>
		/// Name of skin to use for the UI
		/// </summary>
		[Persistent]
		public string skinName = "";

		/// <summary>
		/// Things the use has already seen
		/// </summary>
		[Persistent]
		public List<VersionsSeen> versionsSeen = new List<VersionsSeen>();

		/// <summary>
		/// Return the versions of a mod that the user has seen
		/// </summary>
		/// <param name="modName">Name of the mods</param>
		/// <returns>
		/// List of versions user already saw, if any
		/// </returns>
		public List<string> SeenVersions(string modName)
		{
			return versionsSeen
				.Where(vs => vs.modName == modName)
				.SelectMany(vs => vs.versions.Select(sv => sv.version))
				.ToList();
		}

		/// <summary>
		/// Set a mod version as seen or unseen
		/// </summary>
		/// <param name="modName">Name of the mod</param>
		/// <param name="version">Descriptor of the version</param>
		/// <param name="seen">true if seen ,false if unseen</param>
		public void SetSeen(string modName, string version, bool seen)
		{
			var mod = versionsSeen.FirstOrDefault(vs => vs.modName == modName);
			if (mod == null)
			{
				mod = new VersionsSeen() { modName = modName };
				versionsSeen.Add(mod);
			}
			if (seen)
			{
				if (!mod.versions.Any(sv => sv.version == version))
				{
					mod.versions.Add(new SeenVersion() { version = version });
				}
			}
			else
			{
				mod.versions.RemoveAll(sv => sv.version == version);
			}
		}

		private UrlDir.UrlFile GetDefaultFile()
		{
			var gameData = GameDatabase.Instance.root.children
				.FirstOrDefault(d => d.type == UrlDir.DirectoryType.GameData);
			var modFolder = gameData?.GetDirectory("KerbalChangelog");
			return modFolder == null ? null : new UrlDir.UrlFile(
				modFolder,
				new FileInfo($"{modFolder.path}/KerbalChangelogSettings.cfg"));
		}

		private const string nodeName = "KERBALCHANGELOGSETTINGS";
		private UrlDir.UrlFile saveFile = null;
	}

	/// <summary>
	/// Represents the versions of a particular mod that the user has already seen
	/// </summary>
	public class VersionsSeen
	{
		/// <summary>
		/// Name of the mod
		/// </summary>
		[Persistent]
		public string modName = "";

		/// <summary>
		/// Versions the user has seen
		/// </summary>
		[Persistent]
		public List<SeenVersion> versions = new List<SeenVersion>();
	}

	/// <summary>
	/// A version the user has seen
	/// </summary>
	public class SeenVersion
	{
		/// <summary>
		/// The version
		/// </summary>
		[Persistent]
		public string version;
	}

}
