using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs.Details
{
    /// <summary>
    /// Configurator for an <see cref="AssetsDetail"/> instance.
    /// </summary>
    public sealed class AssetsDetailConfig : BaseDetailConfig<AssetsDetail>
    {
        /// <summary>
        /// Contains the lookup table from an asset name to its paths.
        /// </summary>
        public readonly List<Tuple<string, string>> entries = new List<Tuple<string, string>>();

        /// <inheritdoc />
        protected override sealed AssetsDetail CompileDetail()
        {
            var assetDetail = new AssetsDetail();

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.Item1) || string.IsNullOrEmpty(entry.Item2))
                    {
                        string message = "Asset name or path is not defined";
                        if (assetDetail.owner != null)
                        {
                            message += " on " + assetDetail.owner.id;
                        }
                        
                        throw new Exception (message);
                    }
                    
                    assetDetail.m_Names.Add(entry.Item1);
                    assetDetail.m_Values.Add(entry.Item2);
                }
            }

            return assetDetail;
        }

        /// <inheritdoc />
        protected override sealed void LinkDetail(CatalogBuilder builder)
        {}
    }
}
