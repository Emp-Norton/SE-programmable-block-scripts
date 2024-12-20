using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
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
    internal class StatusDisplayUtilityClass : MyGridProgram
    {

        public void ShowHydrogen(string LcdPanelName)
        {

            {

                // Find the LCD
                IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(LcdPanelName) as IMyTextPanel;
                if (lcd == null)
                {
                    Echo($"LCD panel \"{LcdPanelName}\" not found.");
                    return;
                }

                // Find all hydrogen tanks
                List<IMyGasTank> hydrogenTanks = new List<IMyGasTank>();
                GridTerminalSystem.GetBlocksOfType(hydrogenTanks, tank => tank.BlockDefinition.SubtypeName.Contains("HydrogenTank"));

                if (hydrogenTanks.Count == 0)
                {
                    Echo("No hydrogen tanks found.");
                    lcd.WriteText("Hydrogen Stores: No tanks found\nRemaining Ice: Unknown");
                    return;
                }

                // Calculate total hydrogen in tanks
                double totalHydrogen = 0;
                double totalCapacity = 0;
                foreach (var tank in hydrogenTanks)
                {
                    totalHydrogen += tank.FilledRatio * tank.Capacity;
                    totalCapacity += tank.Capacity;
                }

                double hydrogenPercentage = (totalCapacity > 0) ? (totalHydrogen / totalCapacity) * 100 : 0;

                // Find all inventories for ice
                List<IMyTerminalBlock> inventories = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inventories, block => block.HasInventory);

                MyItemType iceType = MyItemType.MakeOre("Ice");
                double totalIce = 0;

                foreach (var block in inventories)
                {
                    var inventory = block.GetInventory();
                    var items = new List<MyInventoryItem>();
                    inventory.GetItems(items);

                    foreach (var item in items)
                    {
                        if (item.Type == iceType)
                        {
                            totalIce += (double)item.Amount;
                        }
                    }
                }

                // Format and output to LCD
                string output = $"Hydrogen Stores: {hydrogenPercentage:F1}%\nRemaining Ice: {totalIce:N0}";
                lcd.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(output);

                // Debugging output
                Echo("Script executed successfully.");
                Echo(output);
            }

        }

 

        public void BatteryStatusDisplay(IMyTextPanel screen, List<IMyBatteryBlock> batteries)
        {
            screen.ContentType = ContentType.TEXT_AND_IMAGE;
            screen.WriteText("Polling batteries for charge metrics...\n", false);
            
            var ScreenOutput = new StringBuilder();
            ScreenOutput.AppendLine("{_=_Battery Stats_=_}");
            foreach (var battery in batteries)
            {
                string name = battery.CustomName;
                float charge = battery.CurrentStoredPower;
                float maxCharge = battery.MaxStoredPower;
                float chargePerc = (maxCharge > 0) ? (charge / maxCharge) * 100f : 0f;

                ScreenOutput.AppendLine($"{name} :: {charge:F2} / {maxCharge:F2} MWh :: {chargePerc:F0}%");
            }

            screen.WriteText(ScreenOutput.ToString(), false);

        }

    }
}
