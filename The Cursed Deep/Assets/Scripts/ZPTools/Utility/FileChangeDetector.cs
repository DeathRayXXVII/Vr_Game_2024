using UnityEngine;

namespace ZPTools.Utility
{
    public abstract class FileChangeDetector : ChangeDetector
    {
        // The file path to check for changes.
        protected string filePath { get; set; }
    }
}