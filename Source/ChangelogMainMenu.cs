using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalChangelog
{
	using MonoBehavior = MonoBehaviour;

	/// <summary>
	/// Our hook that runs in the main menu, loads the data
	/// and launches the popup if needed
	/// </summary>
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class ChangelogMainMenu : MonoBehavior
	{
		private void Start()
		{
			// Keep alive if another mod bypasses the main menu
			DontDestroyOnLoad(this);

			settings = new ChangelogSettings(GameDatabase.Instance);

			var changelogs = GameDatabase.Instance.GetConfigs("KERBALCHANGELOG")
				.Select(cfg => new Changelog(cfg.config, cfg))
				.Where(cl => cl.HasUnseen(settings.SeenVersions(cl.modName)))
				.ToList();

			for (int i = changelogs.Count - 1; i >= 0; --i)
			{
				var cl = changelogs[i];
				if (cl.alreadySeen)
				{
					// A previous version displayed these changes,
					// mark them as seen so they'll be hidden in the next update
					foreach (var cs in cl.versions)
					{
						settings.SetSeen(cl.modName, cs, true);
					}
					changelogs.Remove(cl);
				}
			}

			if (changelogs.Count > 0)
			{
				controller = gameObject.AddComponent<ChangelogController>();
				controller.settings   = settings;
				controller.changelogs = changelogs;
			}
		}

		private ChangelogSettings   settings;
		private ChangelogController controller;
	}
}
