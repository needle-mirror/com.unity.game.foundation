
//TODO API: this should derive from base stats details definition so it registers properly with stats manager

//namespace UnityEngine.GameFoundation
//{
//    public class MaxQuantityDetailsDefinition : BaseDetailsDefinition
//    {
//        public MaxQuantityDetailsDefinition(int maxQuantity)
//        {
//            m_MaxQuantity = maxQuantity;
//        }
//
//        // NOTE: we do NOT override runtime details constructor because we do NOT
//        //   …    require a runtime version of this def (we are const)
//        // internal virtual BaseDetails<T1, T2> CreateDetails()
//
//
//        private readonly int m_MaxQuantity;
//
//        public int maxQuantity => m_MaxQuantity;
//    }
//}
// TODO API : Commented it out for now, going to need to fix