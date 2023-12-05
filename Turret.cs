using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Game
{
	public class Turret : MonoBehaviour
	{
		public event System.Action<Turret, ICharacter> OnFireAtTarget;

		public Transform ProjectileSpawnPoint;
		public GameObject ProjectilePrefab;

		[Range(0, 180f)] 
		public float angle = 45f;
		public float maxTurnSpeed = 90f;
		public float CooldownSecs = 3f;
		public float MaxRange = 200f;

		[ShowInInspector] [ReadOnly] public ICharacter Target { get; private set; } = null;

		List<Hated> Hatelist = new List<Hated>();

		void OnEnable()
		{
			StartCoroutine(AcquireTargetCo());
		}

		void Update()
		{
			if (Target != null && Target.NetworkObject == null)
				Target = null;

			if (Target != null)
				Aim(Target.NetworkObject.transform.position);
		}

		public void SetHatelist(List<Hated> hl) {
			Hatelist = hl;
		}

		public void ClearTarget() {
			Target = null;
		}

		float CooldownTime = 0f;

		IEnumerator AcquireTargetCo() 
		{
			while(Application.isPlaying)
			{
				List<Hated> removeList = new List<Hated>();
				Hated nearestTarget = null;
				float nearestDistance = float.MaxValue;
				foreach(var hated in Hatelist)
				{
					if (hated.Target == null || hated.Target.transform == null || !hated.Target.IsAlive)
					{
						Hatelist.Remove(hated);
						break;
					}
					var distance = Vector3.Distance(hated.Target.transform.position, transform.position);
					if (distance <= nearestDistance)
					{
						nearestDistance = distance;
						nearestTarget = hated;
					}
				}
				if (nearestTarget != null)
					Target = nearestTarget.Target;
				else
					Target = null;

				//if (Target != null)
				//{
				//	if (Aim(Target.NetworkObject.transform.position))
				//	{
				//		if (GameManager.IsServer)
				//			OnFireAtTarget?.Invoke(Target);
				//		yield return new WaitForSecondsRealtime(CooldownTime);
				//		break;
				//	}
				//}
				yield return new WaitForSecondsRealtime(1f);
			}
		}

		bool Aim(Vector3 targetPoint)
		{
			if (Vector3.Distance(targetPoint, transform.position) > MaxRange)
				return false;

			var turret = transform;
			var hardpoint = turret.parent;

			var targetDirection = targetPoint - turret.position;
			targetDirection = Vector3.ProjectOnPlane(targetDirection, hardpoint.up);
			var signedAngle = Vector3.SignedAngle(hardpoint.forward, targetDirection, hardpoint.up);

			bool outOfRange = false;
			if (Mathf.Abs(signedAngle) > angle)
			{
				outOfRange = true;
				targetDirection = hardpoint.rotation * Quaternion.Euler(0, Mathf.Clamp(signedAngle, -angle, angle), 0) * Vector3.forward;
			}

			var targetRotation = Quaternion.LookRotation(targetDirection, hardpoint.up);
			//bool aimed = false;
			//if (Quaternion.Angle(targetRotation, transform.rotation) <= 45f && !outOfRange)
				//aimed = true;
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxTurnSpeed * Time.deltaTime);
			if (!outOfRange && CooldownTime <= Time.time)
			{
				CooldownTime = Time.time + CooldownSecs;
				if (GameManager.IsServer)
					OnFireAtTarget?.Invoke(this, Target);
			}
			return !outOfRange;
		}

		void OnDrawGizmos()
		{
	#if UNITY_EDITOR
			var range = MaxRange;
			var dashLineSize = 2f;
			var turret = transform;
			var origin = turret.position;
			var hardpoint = turret.parent;
		
			if (!hardpoint) return;
			var from = Quaternion.AngleAxis(-angle, hardpoint.up) * hardpoint.forward;
		
			Handles.color = new Color(0, 1, 0, .2f);
			Handles.DrawSolidArc(origin, turret.up, from, angle * 2, range);

			if (Target == null) return;
		
			var projection = Vector3.ProjectOnPlane(Target.NetworkObject.transform.position - turret.position, hardpoint.up);

			// projection line
			Handles.color = Color.white;
			Handles.DrawDottedLine(Target.NetworkObject.transform.position, turret.position + projection, dashLineSize);
		
			// do not draw target indicator when out of angle
			if (Vector3.Angle(hardpoint.forward, projection) > angle) return;
		
			// target line
			Handles.color = Color.red;
			Handles.DrawLine(turret.position, turret.position + projection);
		
			// range line
			Handles.color = Color.green;
			Handles.DrawWireArc(origin, turret.up, from, angle * 2, projection.magnitude);
			Handles.DrawSolidDisc(turret.position + projection, turret.up, .5f);
	#endif
		}
	}
}
