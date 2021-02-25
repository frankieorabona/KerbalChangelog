namespace KerbalChangelog
{
	/// <summary>
	/// Represents the type of a change in a change set
	/// </summary>
	public enum ChangeType
	{
		/// <summary>
		/// Something the user should definitely see
		/// </summary>
		HighPriority,
		/// <summary>
		/// A new feature
		/// </summary>
		Add,
		/// <summary>
		/// Something that now works differently
		/// </summary>
		Change,
		/// <summary>
		/// Functionality that will be removed
		/// </summary>
		Deprecate,
		/// <summary>
		/// Functionality that has been removed
		/// </summary>
		Remove,
		/// <summary>
		/// Something that was broken and no longer is
		/// </summary>
		Fix,
		/// <summary>
		/// Something that may have allowed an attacker to compromise something
		/// </summary>
		Security,
		/// <summary>
		/// Something that speeds up the mod
		/// </summary>
		Performance,
		/// <summary>
		/// Nothing in particular
		/// </summary>
		None,
	}
}
