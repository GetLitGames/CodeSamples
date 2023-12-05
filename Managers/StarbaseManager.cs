using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace _Game
{
    public class StarbaseManager : SingletonNetworkBehaviour<StarbaseManager>
    {
        public StarbaseDefinition[] StarbaseDefinitions;

        static List<Starbase> Starbases = new List<Starbase>();

        static public void AddStarbase(Starbase Starbase)
        {
            AllStarbases.Add(Starbase);
        }

        static public void RemoveStarbase(Starbase Starbase)
        {
            AllStarbases.Remove(Starbase);
        }

        static public List<Starbase> AllStarbases
        {
            get
            {
                return Starbases;
            }
        }

        public StarbaseDefinition GetStarbaseDefinition(int id) {
            return StarbaseDefinitions.FirstOrDefault(x => x.Id == id);
        }
    }
}
