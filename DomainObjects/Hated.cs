using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using Fusion;

namespace _Game
{
    public class Hated
	{
		public Hated(ICharacter targetEntity, float value)
		{
			Target = targetEntity;
			OriginalHatePosition = targetEntity.transform.position;
			AggroValue = value;
		}
		public ICharacter Target;
		public float AggroValue;
		public Vector3 OriginalHatePosition;
	}
}
