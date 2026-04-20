using System;
using System.Collections.Generic;

[Serializable]
public class LockEntry
{
    public string filePath;
    public string lockedBy;
    public string timestamp;
}

[Serializable]
public class LockFile
{
    public List<LockEntry> locks = new List<LockEntry>();
}