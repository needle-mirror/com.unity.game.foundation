#if UNITY_EDITOR

using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class StoreItemObject
    {
        /// <summary>
        /// Sets the <see cref="enabled"/> value of the store item.
        /// </summary>
        /// <param name="enable">The new value.</param>
        internal void Editor_SetEnabled(bool enable)
        {
            if (m_Enabled == enable) return;
            m_Enabled = enable;
            EditorUtility.SetDirty(store);
        }
    }
}

#endif
