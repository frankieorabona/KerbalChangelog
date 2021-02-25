using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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
				(Screen.width  - windowWidth)  / 2,
				(Screen.height - windowHeight) / 2,
				windowWidth, windowHeight
			);
			changelogs = LoadChangelogs();
			Debug.Log("[KCL] Displaying " + changelogs.Count + " changelogs");
			changesLoaded = true;
			changelogSelection = settings.defaultChangelogSelection
				&& changelogs.Count > 1;
			// Keep alive if another mod bypasses the main menu
			DontDestroyOnLoad(this);
		}

		private List<Changelog> LoadChangelogs()
		{
			Debug.Log("[KCL] Loading changelogs...");
			var retList = new List<Changelog>();
			UrlDir.UrlConfig[] cfgDirs = GameDatabase.Instance.GetConfigs("KERBALCHANGELOG");
			foreach (var cfgDir in cfgDirs)
			{
				// Loads the config node from the directory
				ConfigNode kclNode = cfgDir.config;
				retList.Add(new Changelog(kclNode, cfgDir));

				// Sets changelogs to unviewable
				if (!kclNode.SetValue("showChangelog", false))
				{
					Debug.Log("[KCL] Unable to set value 'showChangelog' in " + cfgDir.ToString());
				}
				cfgDir.parent.SaveConfigs();
			}
			Debug.Log("[KCL] Loaded " + retList.Count + " valid changelogs");
			return retList.Where(cl => cl.showCL).ToList();
		}

		private void OnGUI()
		{
			if (!showChangelog || changelogs.Count == 0)
			{
				Destroy(this);
				return;
			}
			// Can't access GUI.skin outside OnGUI
			skin = settings.skinName == GUI.skin.name ? GUI.skin : HighLogic.Skin;
			dispcl = changelogs[dispIndex];
			if (showChangelog && changesLoaded && !changelogSelection)
			{
				displayWindow = GUILayout.Window(
					89156,
					displayWindow,
					DrawChangelogWindow,
					dispcl.modName + " " + dispcl.highestVersion.ToStringVersionName(),
					skin.window,
					GUILayout.Width(windowWidth),
					GUILayout.Height(windowHeight)
				);
			}
			else if (showChangelog && changesLoaded && changelogSelection)
			{
				displayWindow = GUILayout.Window(
					89157,
					displayWindow,
					DrawChangelogSelection,
					"Kerbal Changelog",
					skin.window,
					GUILayout.Width(windowWidth),
					GUILayout.Height(windowHeight)
				);
			}
		}

		private void DrawChangelogWindow(int id)
		{
			GUI.DragWindow(new Rect(0, 0, displayWindow.width, 20));
			GUILayout.BeginHorizontal();
			if (dispcl.webpageValid)
			{
				if (GUILayout.Button("Visit this mod's website", skin.button))
				{
					Application.OpenURL("https://" + dispcl.webpage);
				}
			}
			GUILayout.FlexibleSpace();
			if (changelogs.Count > 1)
			{
				if (GUILayout.Button("Select changelogs", skin.button))
				{
					changelogSelection = true;
				}
			}
			if (GUILayout.Button("Skin", skin.button))
			{
				skin = skin == HighLogic.Skin ? GUI.skin : HighLogic.Skin;
				settings.skinName = skin.name;
				settings.Save();
			}
			GUILayout.EndHorizontal();
			GUILayout.Label(dispcl.Header(), new GUIStyle(skin.label)
			{
				richText = true,
			});
			changelogScrollPos = GUILayout.BeginScrollView(
				changelogScrollPos, skin.textArea
			);
			GUILayout.Label(dispcl.Body(), new GUIStyle(skin.label)
			{
				richText = true,
			});

			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal();
			if (changelogs.Count > 1)
			{
				if (GUILayout.Button("Previous", skin.button))
				{
					dispIndex = (dispIndex + changelogs.Count - 1) % changelogs.Count;
				}
			}
			if (GUILayout.Button("Close", skin.button))
			{
				showChangelog = false;
			}
			if (changelogs.Count > 1)
			{
				if (GUILayout.Button("Next", skin.button))
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
				"Start here"
			);
			if (startHere != settings.defaultChangelogSelection)
			{
				settings.defaultChangelogSelection = startHere;
				settings.Save();
			}
			if (GUILayout.Button("Read changelogs", skin.button))
			{
				changelogSelection = false;
			}
			if (GUILayout.Button("Skin", skin.button))
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
			if (GUILayout.Button("Close", skin.button))
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
