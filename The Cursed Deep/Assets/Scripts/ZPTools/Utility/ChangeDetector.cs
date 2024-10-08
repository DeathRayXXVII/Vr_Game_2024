using UnityEngine;

namespace ZPTools.Utility
{
    public abstract class ChangeDetector
    {
        // The method that derived classes must implement to check if the data has changed or is null.
        public abstract bool HasChanged();

        // An optional method to reset or update the state after a change is detected.
        public abstract void UpdateState();
    }
}