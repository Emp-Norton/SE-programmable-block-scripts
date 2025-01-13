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



            void RaycastAndDispatch(IMyCameraBlock raycastCamera, double scanRange, IMyCockpit cockpit, IMyRemoteControl remote)
            {

                Vector3D targetPosition;
                bool targetReached = false;
                const double STOPPING_DISTANCE = 100.0; // meters
                const double MAX_SPEED = 100.0; // m/s
                const double ACCELERATION = 10.0; // m/s²
                const double PRECISION = 1.0; // meters

                IMyTextSurface lcd = cockpit.GetSurface(0);
                IMyTextSurface parsedPanel = cockpit.GetSurface(1);


                // Get the camera and LCD panel


                GetRaycastInfo(camera, scanRange, lcd);
                if (targetPosition != null && targetPosition != new Vector3D(0, 0, 0))
                {
                    GoThere(cockpit, remote);
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

            void GoThere(IMyShipController controller, IMyRemoteControl remote)
            {
                // Get required blocks

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

public Program()
{
    //    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

/*
Vector3D GetDirectionVector(float distance, int pitch, int yaw)
{
    double pitchRad = MathHelper.ToRadians(pitch);
    double yawRad = MathHelper.ToRadians(yaw);

    double x = distance * Math.Cos(pitchRad) * Math.Cos(yawRad);
    double y = distance * Math.Sin(pitchRad);
    double z = distance * Math.Cos(pitchRad) * Math.Sin(yawRad);

    return new Vector3D(x, y, z);
}
*/


IMyCameraBlock camera;
IMyTextPanel logger;
IMyTextPanel outputLcd;
IMyButtonPanel button;
IMyTextPanel updaterLcd;

public void Main(string argument, UpdateType updateSource)
{

    string scanBreak = "------------------------------------------------";
    string cameraName = "raycam";
    string loggerLcdName = "loggerLcd";
    string outputLcdName = "outputLcd";
    string fireButtonName = "fireButton";

    if (camera == null)
    {
        camera = GridTerminalSystem.GetBlockWithName(cameraName) as IMyCameraBlock;
    }
    if (logger == null) { logger = GridTerminalSystem.GetBlockWithName(loggerLcdName) as IMyTextPanel; }
    if (outputLcdName == null) { outputLcd = GridTerminalSystem.GetBlockWithName(outputLcdName) as IMyTextPanel; }


    camera.EnableRaycast = true;
    double targetDist = 10000;
    int maxAngle = 45;
    int pitch = 0;
    int yaw = 0;
    if (camera != null)
    {
        string log = "";



        for (var pitcha = 0; pitcha < 45; pitcha++)
        {
            for (var yawa = 0; yawa < 45; yawa++)
            {

                log += $"Trying to scan now @ {targetDist} - P {pitcha} - Y {yawa}\n";
                log += $"{scanBreak}\n Scanning: {targetDist}m of {camera.AvailableScanRange}m @ {camera.RaycastTimeMultiplier / 1000}s / m\n {scanBreak} \n";
           
                logger.WriteText(log, true);

                if (camera.AvailableScanRange >= targetDist)
                {
    
                    MyDetectedEntityInfo target = camera.Raycast(targetDist, pitch = pitcha, yaw = yawa);

                    // Prepare display text
                    string output = "";
                    if (!target.IsEmpty())
                    {
                        output = $"Camera Output\n\n" +
                                 $"Entity Detected:\n" +
                                 $"- Name: {target.Name}\n" +
                                 $"- Type: {target.Type}\n" +
                                 $"- Distance: {Vector3D.Distance(camera.GetPosition(), target.Position):F1} m\n" +
                                 $"- Position: {target.Position}\n\n" +
                                 $"- {target.ToString()}";


                    }
                    outputLcd.WriteText(output);
                }
                else
                {
                    Echo("No cam");
                }
            }
        }
    }


}