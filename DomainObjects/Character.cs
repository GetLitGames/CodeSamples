using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Forge3D;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

namespace _Game
{
	public class Character : NetworkBehaviour, ICharacter // where T: EntityDefinition 
	{
		public event System.Action<EventInfo> OnHit;
		public event System.Action<EventInfo> OnDie;

		public CharacterDefinition BaseDef;
		CharacterDefinition ICharacter.BaseDef { get { return BaseDef; } }

		//public EntityDefinition TheDef { get { return Def; } set { Def = value; } }
		//public EntityDefinition TheBaseDef { get { return BaseDef; } set { BaseDef = value; } }

		[Networked] public NetworkBool IsPlayer { get; set; }
		public virtual bool IsIndestructible { get; protected set; }

		public bool IsValid { get { return Object?.IsValid ?? false; } }
		[Networked] public NetworkObject NetworkObject { get; protected set; }
		[Networked] public virtual PlayerRef PlayerRef { get; protected set; }

		[Networked] public virtual FactionEnum Faction { get; protected set; }
		[Networked(OnChanged = nameof(OnHealthChangedStatic))]
		public virtual float Health { get; protected set; }
		public virtual float MaxHealth { get { return BaseDef.MaxHealth; } }

		[Networked(OnChanged = nameof(OnNetworkDisplayNameChangedStatic))]
		public virtual NetworkString<_32> NetworkDisplayName { get; protected set; }
		protected virtual void OnNetworkDisplayNameChanged(Changed<Character> entity) { _displayName = entity.Behaviour.NetworkDisplayName.Value; }
		public static void OnNetworkDisplayNameChangedStatic(Changed<Character> entity)
		{
			entity.Behaviour.OnNetworkDisplayNameChanged(entity);
		}

		[Networked(OnChanged = nameof(OnNetworkPlayerIdChangedStatic))]
		public NetworkString<_16> NetworkPlayerId { get; protected set; }
		protected virtual void OnNetworkPlayerIdChanged(Changed<Character> entity) { _playerId = entity.Behaviour.NetworkPlayerId.Value; }
		public static void OnNetworkPlayerIdChangedStatic(Changed<Character> entity)
		{
			entity.Behaviour.OnNetworkPlayerIdChanged(entity);
		}
		string _playerId;
		public string PlayerId { 
			get { return _playerId; } 
		}
		public void SetPlayerId(string id)
		{
			if (Object.HasStateAuthority)
			{
				_playerId = id;
				NetworkPlayerId = id;
			}
		}

		string _displayName;
		public string DisplayName { 
			get { return _displayName; } 
		}
		public void SetDisplayName(string s)
		{
			if (Object.HasStateAuthority)
			{
				_displayName = s;
				NetworkDisplayName = s;
			}
		}

		[Networked(OnChanged = nameof(OnIsAliveChangedStatic))]
		public virtual NetworkBool IsAlive { get; protected set; }
		protected bool _isAlive;
		public Collider[] Colliders;
		public float HatelistClearTimeSecs = 120f;

		public float InfoDistanceMax = 120f;
		public float InteractDistanceMax = 100f;

		public float InfoDistance { get { return InfoDistanceMax; } }
		public float InteractDistance { get { return InteractDistanceMax; } }

		public virtual bool CanInteract(ICharacter entity) { return Vector3.Distance(entity.transform.position, base.transform.position) <= InteractDistanceMax; }

		protected Rigidbody _rb = null;
		protected NavMeshAgent _nav = null;

		protected Vector3 _originalPosition;
		protected Vector3 _targetOriginalPosition;
		protected Vector3 _targetClosestPoint;
		protected float _nextWanderTime;
		protected ICharacter _target = null;
		protected CharacterDefinition _chrDef { get { return BaseDef; } }
		protected Player _player = null;
		protected CompassNavigatorPro.CompassProPOI _poi = null;

		[System.NonSerialized]
		public List<Hated> Hatelist = new List<Hated>();

		CharacterVisualSimulation _visualController;
		float _nextHatelistClearTime = 0;

		protected virtual void Awake()
		{
			_rb = GetComponent<Rigidbody>();
			_nav = GetComponent<NavMeshAgent>();
			_visualController = GetComponent<CharacterVisualSimulation>();

			CharacterManager.AddCharacter(this);
		}

		void OnDestroy()
		{
			CharacterManager.RemoveCharacter(this);
		}

		protected virtual void Update()
		{
			if (!GameManager.IsServer || !Object.IsValid)
				return;

			if (_target != null && _nav && _nav.isActiveAndEnabled && _nav.isOnNavMesh)
				FaceTarget(_target.transform.position, .01f);
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();

			if (_poi && _poi.title != DisplayName)
				_poi.title = DisplayName;
		}

		float _nextNavStopResetTime = 0;
		public void OnCharacterManagerUpdate()
		{
			if (!GameManager.IsServer || !Object.IsValid)
				return;

			if (IsPlayer)
				return;

			if (_nav && _nav.speed != BaseDef.NavMeshMovementSpeed)
				_nav.speed = BaseDef.NavMeshMovementSpeed;
			if (_nav && _nav.acceleration != BaseDef.NavMeshAcceleration)
				_nav.acceleration = BaseDef.NavMeshAcceleration;
			if (_nav)
			{
				if (_nextNavStopResetTime < Time.time)
				{
					_nav.updateRotation = true;
					_nextNavStopResetTime = Time.time + 1f;
				}

				if (_nav.stoppingDistance != BaseDef.NavMeshStoppingDistance)
					_nav.stoppingDistance = BaseDef.NavMeshStoppingDistance;
			}
			if (_nav && _nav.angularSpeed != BaseDef.NavMeshAngularSpeed)
				_nav.angularSpeed = BaseDef.NavMeshAngularSpeed;

			FindTarget();

			if (_nextHatelistClearTime <= Time.time)
			{
				ClearHatelist();
			}

			if (_target != null)
				_targetClosestPoint = _target.transform.position; //target.gameObject.ClosestPointOnAnyCollider(ViewOrigin.position);

			if (_nav && _nav.isActiveAndEnabled && _nav.isOnNavMesh)
			{
				if (_target != null) // && Vector3.Distance(transform.position, target.transform.position) > nav.stoppingDistance + 2f)
					_nav.SetDestination(_targetClosestPoint);

				//if (nav.velocity.sqrMagnitude == 0f && IsLegacy && !string.IsNullOrEmpty(LegacyRunName) && anim.IsPlaying(LegacyRunName))
				//	anim.Stop();

				if (!_nav.pathPending)
				{
					if (_target != null && (Vector3.Distance(transform.position, _targetClosestPoint) > (_nav.stoppingDistance + 2f)))
					{
						//nav.SetDestination(targetClosestPoint);
					}
					else
					{
						//if (!float.IsPositiveInfinity(nav.remainingDistance) && nav.remainingDistance <= nav.stoppingDistance) // positive infinity means it doesnt know
						{
							//if (!nav.hasPath || nav.velocity.sqrMagnitude == 0f)
							{
								if (_chrDef.MaxWanderDistance > 0)
								{
									if (_nextWanderTime <= Time.time)
									{
										_nextWanderTime = Time.time + Random.Range(_chrDef.WanderIntervalMax/2, _chrDef.WanderIntervalMax);
										Vector3 newPosition = FindWanderPosition(_originalPosition);
										//if (aipath)
										//aipath.destination = newPosition;
										Pause(false);
										_nav.SetDestination(newPosition);
									}
								}
							}
						}
					}
				}
			}
		}

		void FaceTarget(Vector3 destination, float updateAmount = .25f)
		{
			Vector3 lookPos = destination - transform.position;
			lookPos.y = 0;
			Quaternion rotation = Quaternion.LookRotation(lookPos);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, updateAmount);
		}

		public override void Spawned()
		{
			base.Spawned();
			_isAlive = true;
			_originalPosition = transform.position;
			if (_poi)
				_poi.enabled = true;

			if (Object.IsValid && Object.HasStateAuthority)
			{
				IsAlive = true;
				Health = BaseDef.MaxHealth;
				NetworkDisplayName = BaseDef.Title;
				NetworkObject = Object;
				Faction = BaseDef.Faction;
				if (Faction != FactionEnum.Player && _nav)
					_nav.enabled = true;

				if (Object.InputAuthority.IsValid)
				{
					_player = NetworkManager.GetPlayer(Object.InputAuthority);
					if (_player)
					{
						SetPlayerId(_player.PlayerId.Value);
						PlayerRef = _player.PlayerRef;
					}
				}
			}
		}

		public virtual void Hit(EventInfo info, bool isRPC = false)
		{
			if (!_isAlive)
				return;

			print($"Hit - {info}");
			switch (info.Type)
			{
				case EventInfo.EventType.Damage:
				{
					if (!IsIndestructible)
					{
						//print($"NewHitPoint: {info.Point}");
						if (Object.HasStateAuthority)
						{
							Health -= info.Amount;
							if (Health <= 0)
								Die(info);
						}
						else
						{
							if (Health - info.Amount <= 0)
								Die(info);
						}
					}
				}
				break;
			}

			if (_isAlive)
				OnHit?.Invoke(info);

			if (!GameManager.IsServer) // info.Source is only available on the server
				return;

			if (IsPlayer)
				return;

			if (_chrDef.FleeType == CharacterFleeType.AlwaysOnHit || (_chrDef.FleeType == CharacterFleeType.LowHealth && Health < _chrDef.MaxHealth / 25f))
			{
				if (GameManager.IsServer)
				{
					var runTo = FindFleePosition(info.Source.transform);
					Pause(false);
					_nav.SetDestination(runTo);
				}
			}

			if (info.Type == EventInfo.EventType.Damage && info.Source != null)
			{
				var hated = Hatelist.FirstOrDefault(x => x.Target == info.Source);
				if (hated == null)
					Hatelist.Add(new Hated(info.Source, info.Amount));
				else
					hated.AggroValue += info.Amount;

				_nextHatelistClearTime = Time.time + HatelistClearTimeSecs;
				info.Source.OnDie += HatelistSource_OnDie;
			}
		}

		protected virtual void HatelistSource_OnDie(EventInfo info)
		{
			var hated = Hatelist.FirstOrDefault(x => x.Target == info.Target);
			if (hated != null)
				Hatelist.Remove(hated);
		}

		protected virtual void Die(EventInfo info)
		{
			_isAlive = false;
			if (Object.HasStateAuthority)
				IsAlive = false;

			if (_rb)
			{
				_rb.angularVelocity = Vector3.zero;
				_rb.velocity = Vector3.zero;
			}
			if (_poi)
				_poi.enabled = false;

			Colliders.ForEach(x => { if (x) x.enabled = false; });

			OnDie?.Invoke(info);
		}

		public void Pause(bool p)
		{
			if (p)
			{
				_rb.velocity = Vector3.zero;
				_nav.isStopped = true;
			}
			else
			{
				_nav.isStopped = false;
			}
		}

		protected virtual void ClearHatelist()
		{
			_nextHatelistClearTime = Time.time + HatelistClearTimeSecs;
			Hatelist.Clear();
		}

		protected virtual void OnIsAliveChanged(Changed<Character> entity)
		{
			try
			{
				// Read the previous value
				entity.LoadOld();
				var wasAlive = entity.Behaviour.IsAlive;

				// Read the current value
				entity.LoadNew();
				var isAlive = entity.Behaviour.IsAlive;

				entity.Behaviour.ToggleVisuals(wasAlive, isAlive);
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public void ToggleVisuals(bool wasAlive, bool alive)
		{
			if (!_visualController)
				return;

			// Check if the spaceship was just brought to life
			if (wasAlive == false && alive == true)
			{
				_visualController.TriggerSpawn();
			}
			// or whether it just got destroyed.
			else if (wasAlive == true && alive == false)
			{
				_visualController.TriggerDestruction();
				if (BaseDef.DestroySound)
					SoundManager.Instance.PlaySound(BaseDef.DestroySound, base.transform.position);
				if (BaseDef.DestroyEffectPrefab)
					GameObject.Instantiate(BaseDef.DestroyEffectPrefab, base.transform.position, base.transform.rotation);
			}
		}

		public static void OnIsAliveChangedStatic(Changed<Character> entity)
		{
			entity.Behaviour.OnIsAliveChanged(entity);
		}

		protected virtual void OnHealthChanged(Changed<Character> entity) { }
		public static void OnHealthChangedStatic(Changed<Character> entity)
		{
			entity.Behaviour.OnHealthChanged(entity);
		}

		//public void InvokeOnHit(EventInfo info, bool isRPC = false)
		//{
		//	OnHit?.Invoke(info);
		//}
		//public void InvokeOnDie(EventInfo info, bool isRPC = false)
		//{
		//	OnDie?.Invoke(info);
		//}

		public enum EnemyCheckType { HitBy, Attack };
		public bool IsEnemy(Character targetChr)
		{
			return IsEnemy(targetChr.transform);
		}

		public bool IsEnemy(Transform otherTransform)
		{
			FactionEnum targetFaction = FactionEnum.Scav;

			Character otherChr = otherTransform.GetComponent<Character>();
			if (otherChr)
			{
				targetFaction = otherChr.Faction;
				if (targetFaction == FactionEnum.None)
					Debug.Log("Faction is NONE on " + otherChr.name);
			}

			return CharacterManager.Instance.IsEnemy(Faction, targetFaction);
		}

		Vector3 FindWanderPosition(Vector3 ofPosition)
		{
			Vector3 randomPosition = Vector3.zero;
			Vector3 newPosition = Vector3.zero;
			float farthestDistance = 0f;
			Vector3 farthestPosition = Vector3.zero;
			UnityEngine.AI.NavMeshHit hit;

			for (int i = 0; i < 20; i++) // try 20 times to find something outside the MinWanderDistance then giveup
			{
				randomPosition = new Vector3(Random.insideUnitSphere.x * BaseDef.MaxWanderDistance, 0, Random.insideUnitSphere.z * BaseDef.MaxWanderDistance);
				//randomPosition = Random.insideUnitSphere * MaxWanderDistance;
				//randomPosition.y = transform.position.y;
				//randomPosition = MaxWanderDistance * RandHollowSphere(MinWanderDistance);
				newPosition = randomPosition + ofPosition;

				//if (newPosition.y <= SurvivalWaterBody.Instance.transform.position.y)
				//	continue;

				UnityEngine.AI.NavMesh.SamplePosition(newPosition, out hit, BaseDef.MaxWanderDistance, _nav.areaMask);

				var distance = Vector3.SqrMagnitude(hit.position - this._originalPosition);
				if (distance >= farthestDistance)
				{
					farthestDistance = distance;
					farthestPosition = newPosition;
				}

				if (distance >= BaseDef.MaxWanderDistance / 2)
				{
					break;
				}
			}

			return farthestPosition;
		}

		Vector3 FindFleePosition(Transform hitFrom)
		{
			transform.rotation = Quaternion.LookRotation(transform.position - hitFrom.position);

			Vector3 runTo = transform.position;
			NavMeshHit hit;
			Vector3 randomPosition;

			for (int i = 0; i < 10; i++) // try 10 times to find something outside the MinWanderDistance then giveup
			{
				runTo = transform.position + transform.forward * 100f;
				randomPosition = new Vector3(Random.insideUnitSphere.x * 5, 0, Random.insideUnitSphere.z * 5);
				runTo = runTo + randomPosition;

				if (NavMesh.SamplePosition(runTo, out hit, BaseDef.MaxWanderDistance, _nav.areaMask))
					return hit.position;
			}
			return runTo;
		}

		void AddHate(ICharacter t, float damage)
		{
			Hated h = Hatelist.Where(x => x.Target == t).FirstOrDefault();
			if (h != null)
				h.AggroValue += damage;
			else
				Hatelist.Add(new Hated(t, damage));
		}

		Hated GetTopOfHate()
		{
			Hatelist = Hatelist.Where(x => x.Target != null).OrderByDescending(x => x.AggroValue).ToList();
			var h = Hatelist.FirstOrDefault();
			if (h != null)
				return h;

			return null;
		}

		void RemoveHate(Hated h)
		{
			if (h != null)
				Hatelist.Remove(h);
		}

		void RemoveHate(ICharacter chr)
		{
			if (chr != null)
			{
				var h = Hatelist.Where(x => x.Target == chr).FirstOrDefault();
				if (h != null)
					Hatelist.Remove(h);
			}
		}

		public ICharacter GetTarget()
		{
			return _target;
		}
		public void SetTarget(ICharacter chr)
		{
			_target = chr;
			_targetOriginalPosition = chr.transform.position;
		}

		void ClearTarget()
		{
			_target = null;
			_targetOriginalPosition = Vector3.zero;
		}

		ICharacter FindTarget()
		{
			if (_target != null)
			{
				if (!_target.IsAlive)
				{
					RemoveHate(_target);
					ClearTarget();

					//SetAnimState(AnimState.IsAggro, false);
				}
			}

			Hated h = GetTopOfHate();
			while (h != null)
			{
				if (Vector3.Distance(h.Target.transform.position, h.OriginalHatePosition) > BaseDef.MaxChaseDistance)
				{
					RemoveHate(h);
					h = GetTopOfHate();
					continue;
				}

				SetTarget(h.Target);
				return h.Target;
			}

			Collider[] colliders = Physics.OverlapSphere(transform.position, BaseDef.ViewDistance, BaseDef.AggroLayerMask);
			for (int i = 0; i < colliders.Length; i++)
			{
				var col = colliders[i];
				var chr = col.gameObject.GetComponentInParent<ICharacter>();
				if (chr == this)
					continue;

				if (chr != null && chr.IsAlive && IsEnemy(chr.transform))
				{
					var closestPoint = col.transform.position; //col.gameObject.ClosestPointOnAnyCollider(ViewOrigin.position);
					if (_target != chr &&
						(transform.IsWithinDistance(closestPoint, BaseDef.AggroDistance) || transform.IsWithinCenteredForwardAngleAndDistance(closestPoint, BaseDef.ViewAngle, BaseDef.ViewDistance)))
					{
						AddHate(chr, 1);
						Pause(false);

						h = GetTopOfHate();
						SetTarget(h.Target);
						return h.Target;
					}
				}
			}

			if (_target != null)
				ClearTarget();

			return _target;
		}
	}
}
