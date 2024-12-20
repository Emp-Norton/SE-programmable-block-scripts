using System.Collections.Generic;
using System.Text;
using System;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public class StatusDisplayUtility
    {
        private readonly MyGridProgram _program;

        public StatusDisplayUtility(MyGridProgram program)
        {
            _program = program;
        }

        public void ShowHydrogen(IMyTextPanel lcd, List<IMyGasTank> hydrogenTanks, List<IMyTerminalBlock> inventories)
        {
            if (lcd == null)
            {
                _program.Echo("LCD panel is null.");
                return;
            }

            if (hydrogenTanks.Count == 0)
            {
                _program.Echo("No hydrogen tanks found.");
                lcd.WriteText("Hydrogen Stores: No tanks found\nRemaining Ice: Unknown");
                return;
            }

            double totalHydrogen = 0;
            double totalCapacity = 0;
            foreach (var tank in hydrogenTanks)
            {
                totalHydrogen += tank.FilledRatio * tank.Capacity;
                totalCapacity += tank.Capacity;
            }

            double hydrogenPercentage = (totalCapacity > 0) ? (totalHydrogen / totalCapacity) * 100 : 0;

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

            string output = $"Hydrogen Stores: {hydrogenPercentage:F1}%\nRemaining Ice: {totalIce:N0}";
            lcd.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            lcd.WriteText(output);

            _program.Echo("Hydrogen status displayed successfully.");
            _program.Echo(output);
        }

        public void BatteryStatusDisplay(IMyTextPanel screen, List<IMyBatteryBlock> batteries)
        {
            screen.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            var output = new StringBuilder();
            output.AppendLine("{_=_Battery Stats_=_}");

            foreach (var battery in batteries)
            {
                string name = battery.CustomName;
                float charge = battery.CurrentStoredPower;
                float maxCharge = battery.MaxStoredPower;
                float chargePercentage = (maxCharge > 0) ? (charge / maxCharge) * 100f : 0f;

                output.AppendLine($"{name} :: {charge:F2} / {maxCharge:F2} MWh :: {chargePercentage:F0}%");
            }

            screen.WriteText(output.ToString());
            _program.Echo("Battery status displayed successfully.");
        }
    }
}