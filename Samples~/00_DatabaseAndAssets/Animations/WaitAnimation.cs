using UnityEngine;

namespace UnityEngine.GameFoundation.Sample
{
    public class WaitAnimation : MonoBehaviour
    {
        public Transform waitCircle;

        void Update()
        {
            if (waitCircle != null)
            {
                waitCircle.Rotate(0f, 0f, -360f * Time.deltaTime);
            }
        }
    }
}
