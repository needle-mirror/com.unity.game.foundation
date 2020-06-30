using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// This is a class for storing Definitions for a system that the user setup in the editor.
    /// Derived classes will specify each generic to specify which classes are used by their Catalog.
    /// </summary>
    public abstract partial class BaseCatalogAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <inheritdoc cref="database"/>
        [SerializeField, HideInInspector]
        internal GameFoundationDatabase m_Database;

        /// <summary>
        /// A reference to the database owning this catalog.
        /// </summary>
        public GameFoundationDatabase database => m_Database;

        /// <summary>
        /// Initializes this <see cref="BaseCatalogAsset"/> instance.
        /// </summary>
        internal void Initialize()
        {
            InitializeCatalog();
        }

        /// <summary>
        /// Initializes the specifics of the inherited type.
        /// </summary>
        protected virtual void InitializeCatalog() { }

        /// <summary>
        /// Initializes the <see cref="BaseCatalogAsset"/> instance.
        /// </summary>
        protected void Awake()
        {
            AwakeCatalog();
        }

        /// <summary>
        /// Override this method to initialize the specifics of the inherited
        /// class.
        /// </summary>
        protected virtual void AwakeCatalog() { }

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the content
        /// of this catalog.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        internal void Configure(CatalogBuilder builder)
        {
            ConfigureCatalog(builder);
        }

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the
        /// specific content of this catalog.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        protected virtual void ConfigureCatalog(CatalogBuilder builder) { }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
            => OnBeforeSerializeCatalog();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            OnAfterDeserializeCatalog();
        }

        protected virtual void OnBeforeSerializeCatalog() { }

        protected virtual void OnAfterDeserializeCatalog() { }
    }
}
