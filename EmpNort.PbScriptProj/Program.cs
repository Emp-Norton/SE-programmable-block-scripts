using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        private readonly StatusDisplayUtility _utility;
        private EnemyDetectionUtility _enemyDetection;


        public Program()
        {
            // Pass the current instance to the utility
            _utility = new StatusDisplayUtility(this);
            _enemyDetection = new EnemyDetectionUtility(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
            // Optional save logic
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteries);
            IMyTextPanel batteryPanel = GridTerminalSystem.GetBlockWithName("batteryLcd") as IMyTextPanel;

            List<IMyGasTank> tanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlocksOfType(tanks, tank => tank.BlockDefinition.SubtypeName.Contains("HydrogenTank"));

            IMyTextPanel hydroPanel = GridTerminalSystem.GetBlockWithName("hydroLcd") as IMyTextPanel;

            List<IMyTerminalBlock> inventories = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inventories, block => block.HasInventory);

            if (hydroPanel != null && tanks.Count > 0 && inventories.Count > 0)
            {
                _utility.ShowHydrogen(hydroPanel, tanks, inventories);
            }
            else
            {
                Echo($"Unable to locate Hydrogen LCD or required components.");
            }

            if (batteryPanel != null && batteries.Count > 0)
            {
                _utility.BatteryStatusDisplay(batteryPanel, batteries);
            }
            else
            {
                Echo($"No batteries or battery panel available.");
            }


            var lcd = GridTerminalSystem.GetBlockWithName("enemies") as IMyTextPanel;
            var antennas = new List<IMyRadioAntenna>();
            GridTerminalSystem.GetBlocksOfType(antennas, a => a.IsFunctional);

            // Get the light group (optional, used to set light color)
            var lightGroup = GridTerminalSystem.GetBlockGroupWithName("Ship Lights");

            // Call the enemy detection utility
            _enemyDetection.DetectEnemiesFromAntennae(lcd, antennas, lightGroup);




        }
    }

}