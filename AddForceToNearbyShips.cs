using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game
{
    public class AddForceToNearbyShips : MonoBehaviour
    {
		public float Force = 100f;
        public float MaxDistance = 100f;
		public ForceMode Mode = ForceMode.Impulse;
		public float DelaySecs = 0;

		Collider[] nearbyColliders = new Collider[40];
		int nearbyCollidersTotal = 0;

        void Start()
        {
			if (!GameManager.IsServer)
				return;

			StartCoroutine(StartCo());
		}

		IEnumerator StartCo() {
			if (DelaySecs > 0)
				yield return new WaitForSecondsRealtime(DelaySecs);

			nearbyCollidersTotal = Physics.OverlapSphereNonAlloc(transform.position, MaxDistance, nearbyColliders, GameManager.Instance.SpaceshipsLayerMask);
			if (nearbyCollidersTotal > 0)
			{
				for(int i=0; i < nearbyCollidersTotal; i++) {
					var collider = nearbyColliders[i];
					var ship = collider.GetComponentInParent<Ship>();
					if (ship)
					{
						var direction = (ship.transform.position - transform.position);
						ship.AddForce(direction.normalized * (Force / (direction.magnitude/(MaxDistance/3))), Mode);
					}
				};
			}
        }
    }
}
