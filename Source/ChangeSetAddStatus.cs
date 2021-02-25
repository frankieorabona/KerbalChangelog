namespace KerbalChangelog
{
	/// <summary>
	/// Represents the outcome of an attempt to add a change set to a change log
	/// </summary>
	public enum ChangeSetAddStatus
	{
		/// <summary>
		/// It worked!
		/// </summary>
		Success             = 0,
		/// <summary>
		/// Already had this version in the change log
		/// </summary>
		DuplicateVersioning = 1,
		/// <summary>
		/// No changes in the change set
		/// </summary>
		EmptyChangeList     = 2,
	}
}
