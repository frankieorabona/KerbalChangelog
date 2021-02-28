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
			// Keep alive so we can save settings at exit
			DontDestroyOnLoad(this);

			settings = ChangelogSettings.Load(GameDatabase.Instance);

			changelogs = GameDatabase.Instance.GetConfigs("KERBALCHANGELOG")
				.Select(cfg => new Changelog(cfg.config, cfg))
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
				}
			}

			// Don't auto-open at start if the user's already seen everything
			if (changelogs.Any(cl => cl.HasUnseen(settings.SeenVersions(cl.modName))))
			{
				var controller = gameObject.AddComponent<ChangelogController>();
				controller.settings   = settings;
				controller.changelogs = changelogs;
			}
		}

		private void OnDisable()
		{
			// Mark everything we saw as seen at exit
			foreach (var cl in changelogs)
			{
				foreach (var cs in cl.versions)
				{
					settings.SetSeen(cl.modName, cs, true);
				}
			}
			// Note this also saves EVERYTHING to avoid saving seen versions too early
			settings.Save();
		}

		private ChangelogSettings settings;
		private List<Changelog>   changelogs;
	}
}
