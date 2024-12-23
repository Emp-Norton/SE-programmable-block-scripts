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
using System.Collections.Generic;
using System.Text;
using System;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public class EnemyDetectionUtility
    {
        private readonly MyGridProgram _program;

        public EnemyDetectionUtility(MyGridProgram program)
        {
            _program = program;
        }

        public void DetectEnemiesFromAntennae(IMyTextPanel lcd, List<IMyRadioAntenna> antennas, IMyBlockGroup lightGroup)
        {
            if (lcd == null)
            {
                _program.Echo("LCD panel is null.");
                return;
            }

            if (antennas.Count == 0)
            {
                _program.Echo("No antennas found.");
                lcd.WriteText("No enemies detected.");
                SetLightsColor(lightGroup, VRageMath.Color.White);
                return;
            }

            var detectedEnemies = new HashSet<string>();
            foreach (var antenna in antennas)
            {
                // Antennas detect entities broadcasting via beacon or antenna
                if (antenna.IsBroadcasting & antenna.OwnerId != _program.Me.OwnerId)
                {
                    detectedEnemies.Add(antenna.CustomName); // Only their own name is visible in vanilla
                }
            }

            if (detectedEnemies.Count > 0)
            {
                DisplayEnemies(lcd, detectedEnemies);
                SetLightsColor(lightGroup, VRageMath.Color.Red);
            }
            else
            {
                lcd.WriteText("No enemies detected.");
                SetLightsColor(lightGroup, VRageMath.Color.White);
            }
        }

        private void DisplayEnemies(IMyTextPanel lcd, HashSet<string> enemies)
        {
            var output = new StringBuilder();
            output.AppendLine("Detected Enemy Signals:");
            foreach (var enemy in enemies)
            {
                output.AppendLine(enemy);
            }
            lcd.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            lcd.WriteText(output.ToString());
            _program.Echo("Enemies detected and displayed.");
        }

        private void SetLightsColor(IMyBlockGroup lightGroup, VRageMath.Color color)
        {
            if (lightGroup == null)
            {
                _program.Echo("Light group is null.");
                return;
            }

            var lights = new List<IMyLightingBlock>();
            lightGroup.GetBlocksOfType(lights);

            foreach (var light in lights)
            {
                light.Color = color;
            }
            _program.Echo($"Lights set to color: {color}");
        }
    }
}
