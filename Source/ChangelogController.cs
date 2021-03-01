using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace KerbalChangelog
{
	using MonoBehavior = MonoBehaviour;

	/// <summary>
	/// Manages the popup window with the changelog info,
	/// based on info received in the public fields
	/// </summary>
	public class ChangelogController : MonoBehavior
	{
		/// <summary>
		/// Settings to use
		/// </summary>
		public ChangelogSettings settings;

		/// <summary>
		/// Changelogs to display
		/// </summary>
		public List<Changelog> changelogs;

		private void Start()
		{
			Debug.Log("[KCL] Starting up");
			// Set up the window
			displayWindow = new Rect(
				(Screen.width  - windowWidth)  / (2 * GameSettings.UI_SCALE),
				(Screen.height - windowHeight) / (2 * GameSettings.UI_SCALE),
				windowWidth, windowHeight
			);
			// Force historical mode if there's nothing current
			// (it's up to calling code to not start us if it doesn't want this)
			numNewChanges  = changelogs.Count(cl => cl.HasUnseen(settings.SeenVersions(cl.modName)));
			showOldChanges = numNewChanges < 1;
			Debug.Log("[KCL] Displaying " + changelogs.Count + " changelogs");
			changesLoaded = true;
			changelogSelection = settings.defaultChangelogSelection
				&& changelogs.Count > 1;
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
			// Find one we're allowed to show
			dispcl = changelogs[dispIndex];
			if (!canShow(dispcl))
			{
				findValidIndex();
			}
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
			GUILayout.BeginHorizontal();
			if (dispcl.webpageValid)
			{
				if (GUILayout.Button(Localizer.Format("KerbalChangelog_webpageButtonCaption"), skin.button))
				{
					Application.OpenURL("https://" + dispcl.webpage);
				}
			}
			GUILayout.FlexibleSpace();
			GUI.enabled = numNewChanges > 0;
			showOldChanges = WorkingToggle(
				showOldChanges,
				Localizer.Format("KerbalChangeLog_showOldChangesCheckboxCaption")
			);
			GUI.enabled = true;
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
			}
			GUILayout.EndHorizontal();
			GUILayout.Label(dispcl.Header(), new GUIStyle(skin.label)
			{
				richText = true,
			}, GUILayout.ExpandWidth(true));
			changelogScrollPos = GUILayout.BeginScrollView(
				changelogScrollPos, skin.textArea
			);
			GUILayout.Label(
				dispcl.Body(showOldChanges ? null : settings.SeenVersions(dispcl.modName)),
				new GUIStyle(skin.label)
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
			if (showOldChanges ? changelogs.Count > 1 : numNewChanges > 1)
			{
				if (GUILayout.Button(Localizer.Format("KerbalChangelog_prevButtonCaption"), skin.button))
				{
					findValidIndex(false);
				}
			}
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_closeButtonCaption"), skin.button))
			{
				showChangelog = false;
			}
			if (showOldChanges ? changelogs.Count > 1 : numNewChanges > 1)
			{
				if (GUILayout.Button(Localizer.Format("KerbalChangelog_nextButtonCaption"), skin.button))
				{
					findValidIndex(true);
				}
			}
			GUILayout.EndHorizontal();
			GUI.DragWindow();
		}

		private void findValidIndex(bool forwards = true)
		{
			// Never loop infinitely
			int tried = 0;
			do
			{
				dispIndex = forwards
					? (dispIndex + 1) % changelogs.Count
					: (dispIndex + changelogs.Count - 1) % changelogs.Count;
				dispcl = changelogs[dispIndex];
			} while (++tried < changelogs.Count && !canShow(dispcl));
		}

		private bool canShow(Changelog cl)
		{
			return showOldChanges || cl.HasUnseen(settings.SeenVersions(cl.modName));
		}

		private void DrawChangelogSelection(int id)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			var startHere = WorkingToggle(
				settings.defaultChangelogSelection,
				Localizer.Format("KerbalChangelog_startHereCheckboxCaption")
			);
			if (startHere != settings.defaultChangelogSelection)
			{
				settings.defaultChangelogSelection = startHere;
			}
			GUI.enabled = numNewChanges > 0;
			showOldChanges = WorkingToggle(
				showOldChanges,
				Localizer.Format("KerbalChangeLog_showOldChangesCheckboxCaption")
			);
			GUI.enabled = true;
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_closeListingButtonCaption"), skin.button))
			{
				changelogSelection = false;
			}
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_skinButtonCaption"), skin.button))
			{
				skin = skin == HighLogic.Skin ? GUI.skin : HighLogic.Skin;
				settings.skinName = skin.name;
			}
			GUILayout.EndHorizontal();
			quickSelectionScrollPos = GUILayout.BeginScrollView(quickSelectionScrollPos, skin.textArea);

			foreach (Changelog cl in changelogs)
			{
				// Hide mods with no new changes if that checkbox is off
				if (canShow(cl))
				{
					if (GUILayout.Button($"{cl.modName} {cl.highestVersion}", skin.button))
					{
						dispIndex = changelogs.IndexOf(cl);
						changelogSelection = false;
					}
				}
			}
			GUILayout.EndScrollView();
			if (GUILayout.Button(Localizer.Format("KerbalChangelog_closeButtonCaption"), skin.button))
			{
				showChangelog = false;
			}
			GUI.DragWindow();
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

		private int dispIndex = 0;
		private Changelog dispcl;

		private bool showChangelog      = true;
		private bool changesLoaded      = false;
		private bool changelogSelection = false;
		private int  numNewChanges      = 0;
		private bool showOldChanges     = false;
	}
}
