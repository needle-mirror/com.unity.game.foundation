using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Reasons a Transaction can fail.
    /// </summary>
    public enum TransactionFailureReason
    {
        InvalidProductId,
        InsufficientItems,
        None
    }
}
