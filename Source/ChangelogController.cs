using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace KerbalChangelog
{
	using MonoBehavior = MonoBehaviour;

	/// <summary>
	/// The overall mod behavior
	/// </summary>
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class ChangelogController : MonoBehavior
	{
		private void Start()
		{
			Debug.Log("[KCL] Starting up");
			settings = new ChangelogSettings(GameDatabase.Instance);
			// Set up the window
			displayWindow = new Rect(
				(Screen.width  - windowWidth)  / (2 * GameSettings.UI_SCALE),
				(Screen.height - windowHeight) / (2 * GameSettings.UI_SCALE),
				windowWidth, windowHeight
			);
			changelogs = LoadChangelogs(GameDatabase.Instance).ToList();
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
			Debug.Log("[KCL] Displaying " + changelogs.Count + " changelogs");
			changesLoaded = true;
			changelogSelection = settings.defaultChangelogSelection
				&& changelogs.Count > 1;
			// Keep alive if another mod bypasses the main menu
			DontDestroyOnLoad(this);
		}

		private IEnumerable<Changelog> LoadChangelogs(GameDatabase db)
		{
			Debug.Log("[KCL] Loading changelogs...");
			return db.GetConfigs("KERBALCHANGELOG")
				.Select(cfg => new Changelog(cfg.config, cfg))
				.Where(cl => cl.HasUnseen(seenVersions(cl.modName)));
		}

		private void OnGUI()
		{
			if (!showChangelog || changelogs.Count == 0)
			{
				// Mark everything we saw as seen at close
				foreach (var cl in changelogs)
				{
					foreach (var cs in cl.versions)
					{
						settings.SetSeen(cl.modName, cs, true);
					}
				}
				settings.Save();
				Destroy(this);
				return;
			}
			// Can't access GUI.skin outside OnGUI
			skin = settings.skinName == GUI.skin.name ? GUI.skin : HighLogic.Skin;
			dispcl = changelogs[dispIndex];
			if (showChangelog && changesLoaded)
			{
				GUI.matrix = Matrix4x4.TRS(
					Vector3.zero, Quaternion.identity, new Vector3(GameSettings.UI_SCALE, GameSettings.UI_SCALE, 1f)
				);
				if (!changelogSelection)
				{
					displayWindow = GUILayout.Window(
						89156,
						displayWindow,
						DrawChangelogWindow,
						dispcl.modName + " " + dispcl.highestVersion.ToStringVersionName(),
						skin.window,
						GUILayout.Width(windowWidth / GameSettings.UI_SCALE),
						GUILayout.Height(windowHeight / GameSettings.UI_SCALE)
					);
				}
				else
				{
					displayWindow = GUILayout.Window(
						89157,
						displayWindow,
						DrawChangelogSelection,
						Localizer.Format("KerbalChangelog_listingTitle"),
						skin.window,
						GUILayout.Width(windowWidth / GameSettings.UI_SCALE),
						GUILayout.Height(windowHeight / GameSettings.UI_SCALE)
					);
				}
			}
		}

		private void DrawChangelogWindow(int id)
		{
			GUI.DragWindow(new Rect(0, 0, displayWindow.width, 20));
			GUILayout.BeginHorizontal();
			if (dispcl.webpageValid)
			{
				if (GUILayout.Button(Localizer.Format("KerbalChangelog_webpageButtonCaption"), skin.button))
				{
					Application.OpenURL("https://" + dispcl.webpage);
				}
			}
			GUILayout.FlexibleSpace();
			if (changelogs.Count > 1)
			{
				if (GUILayout.Button(Localizer.Format("KerbalChangelog_listingButtonCaption"), skin.button))
				{
					changelogSelection = true;
				}
			}
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_skinButtonCaption"), skin.button))
			{
				skin = skin == HighLogic.Skin ? GUI.skin : HighLogic.Skin;
				settings.skinName = skin.name;
				settings.Save();
			}
			GUILayout.EndHorizontal();
			GUILayout.Label(dispcl.Header(), new GUIStyle(skin.label)
			{
				richText = true,
			}, GUILayout.ExpandWidth(true));
			changelogScrollPos = GUILayout.BeginScrollView(
				changelogScrollPos, skin.textArea
			);
			GUILayout.Label(dispcl.Body(seenVersions(dispcl.modName)), new GUIStyle(skin.label)
			{
				richText = true,
				normal   = new GUIStyleState()
				{
					textColor  = skin.textArea.normal.textColor,
					background = skin.label.normal.background,
				},
			}, GUILayout.ExpandWidth(true));

			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal();
			if (changelogs.Count > 1)
			{
				if (GUILayout.Button(Localizer.Format("KerbalChangelog_prevButtonCaption"), skin.button))
				{
					dispIndex = (dispIndex + changelogs.Count - 1) % changelogs.Count;
				}
			}
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_closeButtonCaption"), skin.button))
			{
				showChangelog = false;
			}
			if (changelogs.Count > 1)
			{
				if (GUILayout.Button(Localizer.Format("KerbalChangelog_nextButtonCaption"), skin.button))
				{
					dispIndex = (dispIndex + 1) % changelogs.Count;
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawChangelogSelection(int id)
		{
			GUI.DragWindow(new Rect(0, 0, displayWindow.width, 20));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			var startHere = WorkingToggle(
				settings.defaultChangelogSelection,
				Localizer.Format("KerbalChangelog_startHereCheckboxCaption")
			);
			if (startHere != settings.defaultChangelogSelection)
			{
				settings.defaultChangelogSelection = startHere;
				settings.Save();
			}
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_closeListingButtonCaption"), skin.button))
			{
				changelogSelection = false;
			}
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_skinButtonCaption"), skin.button))
			{
				skin = skin == HighLogic.Skin ? GUI.skin : HighLogic.Skin;
				settings.skinName = skin.name;
				settings.Save();
			}
			GUILayout.EndHorizontal();
			quickSelectionScrollPos = GUILayout.BeginScrollView(quickSelectionScrollPos, skin.textArea);

			foreach (Changelog cl in changelogs)
			{
				if (GUILayout.Button($"{cl.modName} {cl.highestVersion}", skin.button))
				{
					dispIndex = changelogs.IndexOf(cl);
					changelogSelection = false;
				}
			}
			GUILayout.EndScrollView();
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_closeButtonCaption"), skin.button))
			{
				showChangelog = false;
			}
		}

		private bool WorkingToggle(bool value, string caption)
		{
			var content = new GUIContent(caption);
			var size = skin.textField.CalcSize(content);
			return GUILayout.Toggle(value, content, skin.toggle, GUILayout.Width(size.x + 24));
		}

		private List<string> seenVersions(string modName)
		{
			return settings.versionsSeen
				.Where(vs => vs.modName == modName)
				.SelectMany(vs => vs.versions.Select(sv => sv.version))
				.ToList();
		}

		private static readonly float windowWidth  = 600f * Screen.width  / 1920f;
		private static readonly float windowHeight = 800f * Screen.height / 1080f;

		private GUISkin skin;

		private Rect displayWindow;
		private Vector2 changelogScrollPos      = new Vector2();
		private Vector2 quickSelectionScrollPos = new Vector2();

		private List<Changelog> changelogs = new List<Changelog>();
		private int dispIndex = 0;
		private Changelog dispcl;

		private ChangelogSettings settings;

		private bool showChangelog = true;
		private bool changesLoaded = false;
		private bool changelogSelection = false;
	}
}
