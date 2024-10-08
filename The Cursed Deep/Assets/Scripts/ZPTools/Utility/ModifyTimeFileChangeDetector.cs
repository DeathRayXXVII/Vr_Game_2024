using System.IO;
using UnityEngine;

namespace ZPTools.Utility
{
    public class ModifiedTimeFileChangeDetector : FileChangeDetector
    {
        private System.DateTime _lastModifiedTime;

        public override bool HasChanged()
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                Debug.LogError("File not found: " + filePath);
                return false;
            }

            var currentModifiedTime = fileInfo.LastWriteTime;
            return currentModifiedTime > _lastModifiedTime;
        }

        public override void UpdateState()
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                Debug.LogError("File not found: " + filePath);
                return;
            }

            _lastModifiedTime = fileInfo.LastWriteTime;
        }
    }
}