using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// This class is the base for all the detail assets.
    /// It just defines the relation with its parent
    /// <see cref="CatalogItemAsset"/>, basic methods for the editor, and the
    /// build of the config object.
    /// </summary>
    public abstract partial class BaseDetailAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <inheritdoc cref="itemDefinition"/>
        [SerializeField, HideInInspector]
        internal CatalogItemAsset m_ItemDefinition;

        /// <summary>
        /// A reference to the owner of the this <see cref="BaseDetailAsset"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="CatalogItemAsset"/> this detail asset is attached to.
        /// </returns>
        public CatalogItemAsset itemDefinition => m_ItemDefinition;

        /// <summary>
        /// Returns a friendly name for this <see cref="BaseDetailAsset"/>.
        /// </summary>
        public abstract string DisplayName();

        /// <summary>
        /// Returns a description of the purpose of this
        /// <see cref="BaseDetailAsset"/>.
        /// It is displayed as a tooltip in editor.
        /// </summary>
        public virtual string TooltipMessage()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gives a chance to initialize some properties of this
        /// <see cref="BaseDetailAsset"/> instance.
        /// </summary>
        protected void Awake()
        {
            AwakeDetail();
        }

        /// <summary>
        /// Override this method to intialize the specifics of the inherited
        /// class.
        /// </summary>
        protected virtual void AwakeDetail() { }

        /// <summary>
        /// Creates a config object representing this
        /// <see cref="BaseDetailAsset"/> for the <see cref="CatalogBuilder"/>.
        /// </summary>
        /// <returns>The config class corresponding to this
        /// <see cref="BaseDetailAsset"/>.</returns>
        internal abstract BaseDetailConfig CreateConfig();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            DeserializeDetail();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        protected virtual void DeserializeDetail() { }
    }
}
