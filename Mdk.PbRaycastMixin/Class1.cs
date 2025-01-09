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
            Vector3D targetPosition;
            bool targetReached = false;
            const double STOPPING_DISTANCE = 100.0; // meters
            const double MAX_SPEED = 100.0; // m/s
            const double ACCELERATION = 10.0; // m/s²
            const double PRECISION = 1.0; // meters
            string raycastCameraName = "Raycast Camera"
            string cockpitName = "RaycastCockpit";

            void RaycastToLcd(IMyCameraBlock camera, double scanRange, IMyTextPanel lcd)
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



            void Main(string argument, UpdateType updateSource)
            {

                IMyCockpit cockpit = GridTerminalSystem.GetBlockWithName(cockpitName) as IMyCockpit;
                IMyTextSurface lcd = cockpit.GetSurface(0);
                IMyTextSurface parsedPanel = cockpit.GetSurface(1);


                // Get the camera and LCD panel
                IMyCameraBlock camera = GridTerminalSystem.GetBlockWithName(raycastCameraName) as IMyCameraBlock;

                GetRaycastInfo(camera, 100000, lcd);
                if (targetPosition != null && targetPosition != new Vector3D(0, 0, 0))
                {
                    GoThere();
                }

                // Echo for in-game programmable block status
                Echo("Camera scan complete.");
            }
            void GetRaycastInfo(IMyCameraBlock camera, double scanRange, IMyTextSurface lcd)
            {

                Echo($"Range: {camera.AvailableScanRange.ToString()}");
                // Ensure the camera is enabled for raycasting
                camera.EnableRaycast = true;

                if (camera.AvailableScanRange >= scanRange)
                {
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
                    Echo(targetPosition.ToString());

                    targetPosition = new Vector3D(target.Position.X, target.Position.Y, target.Position.Z);
                    Echo($"Done: {targetPosition.ToString()}");
                }
            }

            void GoThere()
            {
                // Get required blocks
                IMyRemoteControl remote = GridTerminalSystem.GetBlockWithName("Remote Control 2") as IMyRemoteControl;
                IMyShipController controller = GridTerminalSystem.GetBlockWithName("cockpit") as IMyShipController;

                if (remote == null || controller == null)
                {
                    Echo("Error: Required blocks not found!");
                    return;
                }

                // Get current position and velocity
                Vector3D currentPosition = remote.GetPosition();
                Vector3D currentVelocity = controller.GetShipVelocities().LinearVelocity;

                // Calculate distance and direction to target
                Vector3D distanceVector = targetPosition - currentPosition;
                double distance = distanceVector.Length();
                Vector3D direction = Vector3D.Normalize(distanceVector);

                // Check if we've reached the target
                if (distance < PRECISION && currentVelocity.Length() < 0.1)
                {
                    targetReached = true;
                    remote.ClearWaypoints();
                    remote.SetAutoPilotEnabled(false);
                    Echo("Target reached!");
                    return;
                }

                if (!targetReached)
                {
                    // Calculate desired speed based on distance
                    double desiredSpeed = Math.Min(
                        Math.Sqrt(2 * ACCELERATION * Math.Max(0, distance - STOPPING_DISTANCE)),
                        MAX_SPEED
                    );

                    // Set autopilot waypoint
                    remote.ClearWaypoints();
                    remote.AddWaypoint(targetPosition, "Target");

                    // Configure autopilot settings
                    remote.SpeedLimit = (float)desiredSpeed;
                    remote.SetCollisionAvoidance(true);
                    remote.SetDockingMode(false);
                    remote.FlightMode = FlightMode.OneWay;

                    // Enable autopilot if not already enabled
                    if (!remote.IsAutoPilotEnabled)
                    {
                        remote.SetAutoPilotEnabled(true);
                    }

                    // Display status
                    Echo($"Distance to target: {distance:F1}m");
                    Echo($"Current speed: {currentVelocity.Length():F1}m/s");
                    Echo($"Target speed: {desiredSpeed:F1}m/s");
                }
            }
        }
    }
}