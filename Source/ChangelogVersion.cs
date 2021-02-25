using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KerbalChangelog
{
	/// <summary>
	/// Represents a mod's version
	/// </summary>
	public class ChangelogVersion : IComparable
	{
		/// <summary>
		/// Initialize a version
		/// </summary>
		/// <param name="maj">Major version</param>
		/// <param name="min">Minor version</param>
		/// <param name="pat">Patch version</param>
		public ChangelogVersion(int maj, int min, int pat)
		{
			major = maj;
			minor = min;
			patch = pat;
			buildExisted = false;
		}

		/// <summary>
		/// Initialize a version
		/// </summary>
		/// <param name="maj">Major version</param>
		/// <param name="min">Minor version</param>
		/// <param name="pat">Patch version</param>
		/// <param name="bui">Build version</param>
		public ChangelogVersion(int maj, int min, int pat, int bui) : this(maj, min, pat)
		{
			build = bui;
			buildExisted = true;
		}

		/// <summary>
		/// Initialize a version
		/// </summary>
		/// <param name="maj">Major version</param>
		/// <param name="min">Minor version</param>
		/// <param name="pat">Patch version</param>
		/// <param name="vName">String description of the version</param>
		public ChangelogVersion(int maj, int min, int pat, string vName) : this(maj, min, pat)
		{
			versionName = vName;
		}

		/// <summary>
		/// Initialize a version
		/// </summary>
		/// <param name="maj">Major version</param>
		/// <param name="min">Minor version</param>
		/// <param name="pat">Patch version</param>
		/// <param name="bui">Build version</param>
		/// <param name="vName">String description of the version</param>
		public ChangelogVersion(int maj, int min, int pat, int bui, string vName) : this(maj, min, pat, bui)
		{
			versionName = vName;
		}

		/// <summary>
		/// Initialize a version
		/// May fail, needs a try/catch
		/// </summary>
		/// <param name="version">String to parse to get the version info</param>
		/// <param name="cfgDirName">Path where we found the file</param>
		public ChangelogVersion(string version, string cfgDirName)
		{
			if (version == "null")
			{
				versionNull = true;
				return;
			}

			if (!pattern.IsMatch(version))
			{
				if (!malformedPattern.IsMatch(version))
				{
					Debug.Log("[KCL] broken version string: " + version);
					throw new ArgumentException("version is not a valid version");
				}
				Debug.Log("[KCL] malformed version string: " + version + " in directory " + cfgDirName);
				malformedVersionString = true;
			}
			string[] splitVersions = version.Split('.');

			major = int.Parse(splitVersions[0]);
			minor = int.Parse(splitVersions[1]);
			if (!malformedVersionString)
				patch = int.Parse(splitVersions[2]);
			else
				patch = 0;
			if (splitVersions.Length > 3)
			{
				build = int.Parse(splitVersions[3]);
				buildExisted = true;
			}
			else
			{
				build = 0;
				buildExisted = false;
			}
		}

		/// <summary>
		/// Initialize a version
		/// May fail, needs a try/catch
		/// </summary>
		/// <param name="version">String to parse to get the version info</param>
		/// <param name="cfgDirName">Path where we found the file</param>
		/// <param name="vName">String description of the version</param>
		public ChangelogVersion(string version, string cfgDirName, string vName) : this(version, cfgDirName)
		{
			versionName = vName;
		}

		/// <summary>
		/// Initialize a version
		/// May fail, needs a try/catch
		/// </summary>
		/// <param name="version">String to parse to get the version info</param>
		/// <param name="cfgDirName">Path where we found the file</param>
		/// <param name="vName">String description of the version</param>
		/// <param name="vDate">Release date of this version</param>
		/// <param name="vKSP">Compatible game version of this version</param>
		public ChangelogVersion(string version, string cfgDirName, string vName, string vDate, string vKSP)
			: this(version, cfgDirName, vName)
		{
			versionDate = vDate;
			versionKSP  = vKSP;
		}

		/// <returns>
		/// A nice string representation of this version
		/// </returns>
		public override string ToString()
		{
			if (versionNull)
				return "D.N.E";
			if (!buildExisted)
				return $"{major}.{minor}.{patch}" + (versionNameExists ? " \"" + versionName + "\"" : "") + (versionDateExists ? ", released " + versionDate : "") + (versionKSPExists ? ", for KSP version " + versionKSP : "");
			return $"{major}.{minor}.{patch}.{build}" + (versionNameExists ? " \"" + versionName + "\"" : "") + (versionDateExists ? ", released " + versionDate : "") + (versionKSPExists ? ", for KSP version " + versionKSP : "");
		}

		/// <returns>
		/// A short string representation of this version
		/// </returns>
		public string ToStringPure()
		{
			if (versionNull)
				return "D.N.E";
			if (!buildExisted)
				return $"{major}.{minor}.{patch}";
			return $"{major}.{minor}.{patch}.{build}";
		}

		/// <returns>
		/// A string representation of this version that includes the custom name
		/// </returns>
		public string ToStringVersionName()
		{
			if (versionNull)
				return "D.N.E";
			if (!buildExisted)
				return $"{major}.{minor}.{patch}" + (versionNameExists ? " \"" + versionName + "\"" : "");
			return $"{major}.{minor}.{patch}" + (versionNameExists ? " \"" + versionName + "\"" : "");
		}

		/// <summary>
		/// Sort objects from highest version to lowest version
		/// </summary>
		/// <param name="obj">Another version</param>
		/// <returns>
		/// -1 if this&lt;obj, 1 if this&gt;obj, 0 if this==obj
		/// </returns>
		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (obj is ChangelogVersion oCLV)
			{
				if (oCLV.major - this.major == 0)
				{
					if (oCLV.minor - this.minor == 0)
					{
						if (oCLV.patch - this.patch == 0)
						{
							return oCLV.build.CompareTo(this.build);
						}
						return oCLV.patch.CompareTo(this.patch);
					}
					return oCLV.minor.CompareTo(this.minor);
				}
				return oCLV.major.CompareTo(this.major);
			}
			else
			{
				throw new ArgumentException("Object is not a ChangelogVersion");
			}
		}

		// Matches version numbers starting at the beginning to the end of the string
		private static readonly Regex pattern = new Regex(
			"(\\d+\\.\\d+\\.\\d+(\\.\\d+)?)",
			RegexOptions.Compiled
		);
		private static readonly Regex malformedPattern = new Regex(
			"\\d+\\.\\d+(\\.\\d+)?(\\.\\d+)?",
			RegexOptions.Compiled
		);

		private bool versionNull = false;
		private bool malformedVersionString = false;

		private int major { get; set; }
		private int minor { get; set; }
		private int patch { get; set; }
		private int build { get; set; }

		private bool buildExisted;

		private string versionName { get; set; } = null;
		private string versionDate { get; set; } = null;
		private string versionKSP  { get; set; } = null;

		private bool versionNameExists => !string.IsNullOrEmpty(versionName);
		private bool versionDateExists => !string.IsNullOrEmpty(versionDate);
		private bool versionKSPExists  => !string.IsNullOrEmpty(versionKSP);
	}
}
