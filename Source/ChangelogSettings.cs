using System.IO;
using System.Linq;

namespace KerbalChangelog
{
	/// <summary>
	/// Represents this mod's settings. Since we run on the main menu,
	/// we can't be a ScenarioModule.
	/// </summary>
	public class ChangelogSettings
	{
		/// <summary>
		/// Initialize the settings. Load if already defined in the game db,
		/// else start fresh.
		/// </summary>
		public ChangelogSettings(GameDatabase db)
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

}
