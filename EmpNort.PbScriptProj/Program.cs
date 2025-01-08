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

        public bool IsLocal(IMyCubeBlock block) 
        {
            return block.CubeGrid.EntityId == Me.CubeGrid.EntityId;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var onlyGridOwned = argument.Contains("onlyGridOwned");

            var args = argument.Split(',');
            Echo($"Args: {args}");

            string gasPanelName = "gasStatusLcd";
            string batteryPanelName = "batteryStatusLcd";
            string turretPanelName = "turretStatusLcd";
            string inventoryPanelName = "inventoryStatusLcd";
            string raycastPanelName = "raycastStatusLcd";
            string enemyPanelName = "enemyStatusLcd";

            string enemyAlertLightsName = "enemyAlertLights";
            
            // Finder: Create battery list and LCD ref
            var batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteries);
            IMyTextPanel batteryPanel = GridTerminalSystem.GetBlockWithName(batteryPanelName) as IMyTextPanel;

            // Finder: Create hydrogen list and filter
            List<IMyGasTank> hydrogenTanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlocksOfType(hydrogenTanks, tank => tank.BlockDefinition.SubtypeName.Contains("HydrogenTank") && (onlyGridOwned ? tank.CubeGrid.EntityId == Me.CubeGrid.EntityId : true));

            // Finder: Create oxygen list and filter
            List<IMyGasTank> oxygenTanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlocksOfType(oxygenTanks, tank => tank.DetailedInfo.Contains("Oxygen") && (onlyGridOwned ? tank.CubeGrid.EntityId == Me.CubeGrid.EntityId : true));

            // Finder: Create gas status LCD ref
            IMyTextPanel gasStatusPanel = GridTerminalSystem.GetBlockWithName(gasPanelName) as IMyTextPanel;

            // Finder: Get Inventories
            List<IMyTerminalBlock> inventories = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inventories, block => block.HasInventory);

            if (gasStatusPanel != null && hydrogenTanks.Count > 0 && inventories.Count > 0)
            {
                _utility.ShowHydrogen(gasStatusPanel, hydrogenTanks, inventories);
            }
            else
            {
                Echo($"Unable to find Gas LCD, is it null: {gasStatusPanel == null}, or HydrogenTanks: #{hydrogenTanks.Count}.");
            }

            if (gasStatusPanel != null && oxygenTanks.Count > 0 && inventories.Count > 0)
            {
                _utility.ShowOxygen(gasStatusPanel, oxygenTanks, inventories);
            }
            else
            {
                Echo($"Unable to find Gas LCD, is it null: {gasStatusPanel == null},  or OxygenTanks: #{oxygenTanks.Count}");
            }

            if (batteryPanel != null && batteries.Count > 0)
            {
                _utility.BatteryStatusDisplay(batteryPanel, batteries);
            }
            else
            {
                Echo($"No batteries or battery panel available.");
            }


            var lcd = GridTerminalSystem.GetBlockWithName(enemyPanelName) as IMyTextPanel;
            var antennas = new List<IMyRadioAntenna>();
            GridTerminalSystem.GetBlocksOfType(antennas, a => a.IsFunctional);

            // Get the light group (optional, used to set light color)
            var lightGroup = GridTerminalSystem.GetBlockGroupWithName("Ship Lights");

            // Call the enemy detection utility
            _enemyDetection.DetectEnemiesFromAntennae(lcd, antennas, lightGroup);

            string lcdName = "turretsLcd";

            // Get the LCD panel
            IMyTextPanel turretLcd = GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;
            if (lcd == null)
            {
                Echo($"Error: LCD panel '{lcdName}' not found.");
                return;
            }

            // Create utility instance
            TurretStatusUtility turretUtility = new TurretStatusUtility(GridTerminalSystem);

            // Get turret status
            string turretStatus = turretUtility.GetTurretStatus();

            // Update LCD content
            lcd.ContentType = ContentType.TEXT_AND_IMAGE;
            lcd.WriteText(turretStatus);

            // Set LCD color based on ammo status
            if (turretUtility.IsAnyTurretEmpty())
            {
                turretLcd.FontColor =  VRageMath.Color.Red;
            }
            else
            {
                turretLcd.FontColor = VRageMath.Color.White;
            }

            Echo("Turret ammo status updated.");



        }
    }

}