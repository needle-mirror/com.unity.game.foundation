using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs.Details
{
    /// <summary>
    /// Configurator for a <see cref="StatDetail"/> instance.
    /// </summary>
    public sealed class StatDetailConfig : BaseDetailConfig<StatDetail>
    {
        /// <summary>
        /// Contains the lookup table from a stat identifier to default value.
        /// </summary>
        public readonly Dictionary<string, StatValue> entries =
            new Dictionary<string, StatValue>();

        /// <inheritdoc />
        protected override sealed StatDetail CompileDetail()
            => new StatDetail();

        /// <inheritdoc />
        protected override sealed void LinkDetail(CatalogBuilder builder)
        {
            foreach (var entry in entries)
            {
                var statId = entry.Key;
                var value = entry.Value;

                var stat = builder.GetStatOrDie(statId);
                runtimeDetail.m_DefaultValues.Add(statId, value);
            }
        }
    }
}
