using System;
using System.Text.RegularExpressions;
using UnityEngine;
using KSP.Localization;

namespace KerbalChangelog
{
	/// <summary>
	/// Represents a mod's version
	/// </summary>
	public class ChangelogVersion : IComparable
	{
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
			}
			else
			{
				var match = pattern.Match(version);
				if (!match.Success)
				{
					throw new ArgumentException($"Invalid version: {version}");
				}
				major = int.Parse(match.Groups["major"].Value);
				minor = int.Parse(match.Groups["minor"].Value);
				patch = match.Groups["patch"].Success ? int.Parse(match.Groups["patch"].Value) : 0;
				build = match.Groups["build"].Success ? (int?)int.Parse(match.Groups["build"].Value) : null;
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
			return versionNull
				? Localizer.Format("KerbalChangelog_versionDoesNotExist")
				: build.HasValue
					? versionNameExists
						? versionDateExists
							? versionKSPExists
								? Localizer.Format("KerbalChangelog_versionFourPieceWithNameDateCompat",
									major, minor, patch, build, versionName, versionDate, versionKSP)
								: Localizer.Format("KerbalChangelog_versionFourPieceWithNameDate",
									major, minor, patch, build, versionName, versionDate)
							: versionKSPExists
								? Localizer.Format("KerbalChangelog_versionFourPieceWithNameCompat",
									major, minor, patch, build, versionName, versionKSP)
								: Localizer.Format("KerbalChangelog_versionFourPieceWithName",
									major, minor, patch, build, versionName)
						: versionDateExists
							? versionKSPExists
								? Localizer.Format("KerbalChangelog_versionFourPieceWithDateCompat",
									major, minor, patch, build, versionDate, versionKSP)
								: Localizer.Format("KerbalChangelog_versionFourPieceWithDate",
									major, minor, patch, build, versionDate)
							: versionKSPExists
								? Localizer.Format("KerbalChangelog_versionFourPieceWithCompat",
									major, minor, patch, build, versionKSP)
								: Localizer.Format("KerbalChangelog_versionFourPiece",
									major, minor, patch, build)
					: versionNameExists
						? versionDateExists
							? versionKSPExists
								? Localizer.Format("KerbalChangelog_versionThreePieceWithNameDateCompat",
									major, minor, patch, versionName, versionDate, versionKSP)
								: Localizer.Format("KerbalChangelog_versionThreePieceWithNameDate",
									major, minor, patch, versionName, versionDate)
							: versionKSPExists
								? Localizer.Format("KerbalChangelog_versionThreePieceWithNameCompat",
									major, minor, patch, versionName, versionKSP)
								: Localizer.Format("KerbalChangelog_versionThreePieceWithName",
									major, minor, patch, versionName)
						: versionDateExists
							? versionKSPExists
								? Localizer.Format("KerbalChangelog_versionThreePieceWithDateCompat",
									major, minor, patch, versionDate, versionKSP)
								: Localizer.Format("KerbalChangelog_versionThreePieceWithDate",
									major, minor, patch, versionDate)
							: versionKSPExists
								? Localizer.Format("KerbalChangelog_versionThreePieceWithCompat",
									major, minor, patch, versionKSP)
								: Localizer.Format("KerbalChangelog_versionThreePiece",
									major, minor, patch);
		}

		/// <returns>
		/// A short string representation of this version
		/// </returns>
		public string ToStringPure()
		{
			return versionNull
				? Localizer.Format("KerbalChangelog_versionDoesNotExist")
				: build.HasValue
					? Localizer.Format("KerbalChangelog_versionFourPiece",
						major, minor, patch, build)
					: Localizer.Format("KerbalChangelog_versionThreePiece",
						major, minor, patch);
		}

		/// <returns>
		/// A string representation of this version that includes the custom name
		/// </returns>
		public string ToStringVersionName()
		{
			return versionNull
				? Localizer.Format("KerbalChangelog_versionDoesNotExist")
				: build.HasValue
					? versionNameExists
						? Localizer.Format("KerbalChangelog_versionFourPieceWithName",
							major, minor, patch, build, versionName)
						: Localizer.Format("KerbalChangelog_versionFourPiece",
							major, minor, patch, build)
					: versionNameExists
						? Localizer.Format("KerbalChangelog_versionThreePieceWithName",
							major, minor, patch, versionName)
						: Localizer.Format("KerbalChangelog_versionThreePiece",
							major, minor, patch);
		}

		/// <summary>
		/// Sort objects from highest version to lowest version
		/// </summary>
		/// <param name="obj">Another version</param>
		/// <returns>
		/// 1 if this&lt;obj, -1 if this&gt;obj, 0 if this==obj
		/// </returns>
		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (obj is ChangelogVersion oCLV)
			{
				int majCmp = oCLV.major.CompareTo(major);
				if (majCmp != 0)
				{
					return majCmp;
				}
				int minCmp = oCLV.minor.CompareTo(minor);
				if (minCmp != 0)
				{
					return minCmp;
				}
				int patCmp = oCLV.patch.CompareTo(patch);
				if (patCmp != 0)
				{
					return patCmp;
				}
				return oCLV.build.HasValue && build.HasValue ? oCLV.build.Value.CompareTo(build.Value)
					: oCLV.build.HasValue ?  1
					: build.HasValue      ? -1
					: 0;
			}
			else
			{
				throw new ArgumentException("Object is not a ChangelogVersion");
			}
		}

		// Matches version numbers starting at the beginning to the end of the string
		private static readonly Regex pattern = new Regex(
			@"^(?<major>\d+)\.(?<minor>\d+)(?:\.(?<patch>\d+))?(?:\.(?<build>\d+))?$",
			RegexOptions.Compiled
		);

		private bool versionNull = false;

		private int  major { get; set; }
		private int  minor { get; set; }
		private int  patch { get; set; }
		private int? build { get; set; } = null;

		private string versionName { get; set; } = null;
		private string versionDate { get; set; } = null;
		private string versionKSP  { get; set; } = null;

		private bool versionNameExists => !string.IsNullOrEmpty(versionName);
		private bool versionDateExists => !string.IsNullOrEmpty(versionDate);
		private bool versionKSPExists  => !string.IsNullOrEmpty(versionKSP);
	}
}
