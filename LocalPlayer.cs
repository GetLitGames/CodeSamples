using System.Collections;
using System.Collections.Generic;
using CompassNavigatorPro;
using Fusion;
using UnityEngine;

namespace _Game
{
    static public class LocalPlayer
    {
        static public bool IsServer;
        static public string PlayerId;
        static public string DisplayName;
        static public Player Player;
        static public PlayerRef PlayerRef { get { return Player.Object.InputAuthority; } }

        static public Ship Ship { get; private set; }
        static public Camera Camera { get; private set; }

        static public void SetShip(Ship ship)
        {
            if (ship)
            {
                ship.IsLocalPlayer = true;

                Ship = ship;
                Camera = ship.Camera;
                
                ShipCamera.Instance.SetShip(ship);
                PlayerHudPanel.Instance.SetShip(ship);
			    PlayerHudPanel.Instance.Show(true);
            }
            else
            {
                Ship = null;
                Camera = null;

                ShipCamera.Instance.SetShip(null);
                PlayerHudPanel.Instance.SetShip(null);
			    PlayerHudPanel.Instance.Show(false);
            }
        }
    }
}
