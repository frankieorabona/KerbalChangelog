using System.Linq;
using UnityEngine;
using KSP.UI.Screens;

namespace KerbalChangelog
{
	using MonoBehavior = MonoBehaviour;

	/// <summary>
	/// Our hook that runs in the main menu, loads the data
	/// and launches the popup if needed
	/// </summary>
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class ChangelogSpaceCenter : MonoBehavior
	{
		private void Start()
		{
			// This event fires when KSP is ready for mods to add toolbar buttons
			GameEvents.onGUIApplicationLauncherReady.Add(AddLauncher);

			// This event fires when KSP wants mods to remove their toolbar buttons
			GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveLauncher);
		}

		private void OnDisable()
		{
			// Unclick the launcher
			launcher.SetFalse(true);

			// The "dead" copy of our object will re-add itself if we don't unsubscribe to this!
			GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncher);

			// This event fires when KSP wants mods to remove their toolbar buttons
			GameEvents.onGUIApplicationLauncherDestroyed.Remove(RemoveLauncher);

			// Clean up after the popup
			if (controller != null)
			{
				Destroy(controller);
				controller = null;
			}

			// The launcher destroyed event doesn't always fire when we need it (?)
			RemoveLauncher();
		}

		private static readonly Texture2D AppIcon = GameDatabase.Instance.GetTexture(
			$"KerbalChangelog/Icons/KerbalChangelog", false);

		private ApplicationLauncherButton launcher;

		private void AddLauncher()
		{
			if (ApplicationLauncher.Ready && launcher == null)
			{
				launcher = ApplicationLauncher.Instance.AddModApplication(
					onAppLaunchToggleOn, null,
					null,                null,
					null,                null,
					ApplicationLauncher.AppScenes.SPACECENTER,
					AppIcon);

				launcher?.gameObject?.SetTooltip(
					"KerbalChangelog_mainTitle",
					"KerbalChangelog_mainTooltip"
				);
			}
		}

		private void RemoveLauncher()
		{
			if (launcher != null) {
				ApplicationLauncher.Instance.RemoveModApplication(launcher);
				launcher = null;
			}
		}

		/// <summary>
		/// This is called when they click our toolbar button
		/// </summary>
		private void onAppLaunchToggleOn()
		{
			if (controller != null)
			{
				Destroy(controller);
			}
			controller = gameObject.AddComponent<ChangelogController>();
			controller.settings   = ChangelogSettings.Load(GameDatabase.Instance);
			controller.changelogs = GameDatabase.Instance.GetConfigs("KERBALCHANGELOG")
				.Select(cfg => new Changelog(cfg.config, cfg))
				.ToList();

			// Unclick the launcher, since we don't want a toggle
			launcher.SetFalse(true);
		}

		private ChangelogController controller;
	}
}
