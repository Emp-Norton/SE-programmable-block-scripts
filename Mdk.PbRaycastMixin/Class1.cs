using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Mime;
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
    internal class RaycastUtility
    {
        public RaycastUtility() 
        {
            void SetupCast(double range, IMyCameraBlock camera, IMyTextPanel lcd)
            {
                
                Echo($"Range: {camera.AvailableScanRange.ToString()}");
                // Ensure the camera is enabled for raycasting
                camera.EnableRaycast = true;

               // IMySoundBlock sound = GridTerminalSystem.GetBlockWithName("Sound Block") as IMySoundBlock;

                //if (camera.AvailableScanRange >= (scanRange * 0.8)) { sound.Play(); } else { sound.Stop(); }
                if (camera.AvailableScanRange >= scanRange)
                {
                   // sound.Play();
                    // Perform the scan
                    MyDetectedEntityInfo target = camera.Raycast(scanRange);

                    // Prepare display text
                    string output;
                    if (!target.IsEmpty())
                    {
                        output = $"Camera Output\n\n" +
                                 $"Entity Detected:\n" +
                                 $"- Name: {target.Name}\n" +
                                 $"- Type: {target.Type}\n" +
                                 $"- Distance: {Vector3D.Distance(camera.GetPosition(), target.Position):F1} m\n" +
                                 $"- Position: {target.Position}";
                    }
                    else
                    {
                        output = "Camera Output\n\nNo entity detected within range.";
                    }

                    // Write to LCD
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    lcd.WriteText(output);

                    // Echo for in-game programmable block status
                    Echo("Camera scan complete.");
                }
            }
        }
    }
}
