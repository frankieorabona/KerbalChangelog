# Kerbal Changelog

This project is meant to be a simple way for mod creators to add an in-game changelog for their users when they release a new version.

**THIS WILL DO NOTHING ON ITS OWN**

## Adding a changelog

To add a changelog, create a config file (.cfg) with the following nodes and fields (as an example):

	// Must use this node name
	KERBALCHANGELOG
	{
		// Add your mod's name here
		modName = Kerbal Changelog
		// Declares a version node
		VERSION
		{
			// Version number, numbers only with no spaces!
			version = 1.1
			// Any changes in that version. There can be as many change fields as you want
			change = Fixed window scrolling
			change = Added shiny buttons
			change = Removed bugs
			// Create a node to add more details to a change
			CHANGE
			{
				// Unity rich text format supported
				// https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html
				change = <color=#ff0000><b>Important breaking change!</b></color>
				type = Change
			}
		}
		VERSION
		{
			version = 1.0
			change = First release!
		}
	}

This will then be displayed in a changelog window that appears in the main menu the first time the user starts the game with a changelog that has the `showChangelog` set to `True`. After this initial load, the user will no longer see the changelog for that mod until the mod creator releases a new version with the changelog cfg file's `showChangelog` field set to `True`.

This will handle as many mods as have changelogs the user has installed, but please do not create multiple changelog files for a single mod. This will lead to multiple changelog pages showing up in the window, and confusion for everyone.
