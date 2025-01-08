using System.Collections.Generic;
using System.Text;
using System;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Drawing;

namespace IngameScript
{
    public class StatusDisplayUtility
    {
        private readonly MyGridProgram _program;

        public StatusDisplayUtility(MyGridProgram program)
        {
            _program = program;
        }

        public static string PercentageBar(double upTo)
        {
            string percentageVisual = "[";
            for (var i=0; i < 40; i++)
            {
                switch (i <= (upTo/100) * 40)
                {
                    case true: { percentageVisual += "|"; }; break;
                    case false: { percentageVisual += " "; }; break;
                }
            }
            percentageVisual += "]";

            return percentageVisual;
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

            string percBar = PercentageBar(hydrogenPercentage);
            string output = $"Hydrogen Stores: {hydrogenPercentage:F1}%\nRemaining Ice: {totalIce:N0}\n{percBar}\n\n";
            lcd.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            if (totalIce < 50000 || hydrogenPercentage < 50)
            {
                lcd.FontColor = VRageMath.Color.Red;
            }
            lcd.WriteText(output, true);

            _program.Echo("Hydrogen status displayed successfully.");
            _program.Echo(output);
        }

        public void ShowOxygen(IMyTextPanel lcd, List<IMyGasTank> oxygenTanks, List<IMyTerminalBlock> inventories) 
        { 

            double totalOxygen = 0;
            double totalCapacity = 0;
            foreach (var tank in oxygenTanks)
            {
                totalOxygen += tank.FilledRatio * tank.Capacity;
                totalCapacity += tank.Capacity;
            }

            double oxygenPercentage = (totalCapacity > 0) ? (totalOxygen / totalCapacity) * 100 : 0;

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

            string percBar = PercentageBar(oxygenPercentage);
            string output = $"Oxygen Stores: {oxygenPercentage:F1}%\nRemaining Ice: {totalIce:N0}\n{percBar}\n\n";
            lcd.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            if (totalIce < 50000 || oxygenPercentage < 50)
            {
                lcd.FontColor = VRageMath.Color.Red;
            }
            lcd.WriteText(output, true);

            _program.Echo("Oxygen status displayed successfully.");
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
                string batteryPercBar = PercentageBar(chargePercentage);
                output.AppendLine($"{name} :: {charge:F2} / {maxCharge:F2} MWh :: {chargePercentage:F0}%\n{batteryPercBar}");
            }

            screen.WriteText(output.ToString());
            _program.Echo("Battery status displayed successfully.");
        }
    }

    public class TurretStatusUtility
    {
        private IMyGridTerminalSystem GridTerminalSystem;
        private StringBuilder _output;
        private bool _anyTurretEmpty;

        public TurretStatusUtility(IMyGridTerminalSystem gridTerminalSystem)
        {
            GridTerminalSystem = gridTerminalSystem;
            _output = new StringBuilder();
            _anyTurretEmpty = false;
        }

        public string GetTurretStatus()
        {
            _output.Clear();
            _anyTurretEmpty = false;

            // Find all turrets on the grid
            List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
            GridTerminalSystem.GetBlocksOfType(turrets);

            // Process each turret
            foreach (IMyLargeTurretBase turret in turrets)
            {
                // Get turret inventory
                IMyInventory inventory = turret.GetInventory();
                if (inventory == null) continue;

                // Count ammo items
                var items = new List<MyInventoryItem>();
                inventory.GetItems(items);

                int ammoCount = 0;
                int totalCapacity = (int)inventory.MaxVolume;
                string ammoType = "None";

                foreach (var item in items)
                {
                    ammoCount += item.Amount.ToIntSafe();
                    ammoType = item.Type.SubtypeId; // Get the ammo type of the first item
                }

                // Add turret information to the output
                _output.AppendLine($"{turret.CustomName}: {ammoCount} / {totalCapacity} : {ammoType}");

                // Check if the turret is out of ammo
                if (ammoCount == 0)
                {
                    _anyTurretEmpty = true;
                }
            }

            return _output.ToString();
        }

        public bool IsAnyTurretEmpty()
        {
            return _anyTurretEmpty;
        }
    }

}