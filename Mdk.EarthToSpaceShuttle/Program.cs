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
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }


        Vector3D targetPosition = Vector3D.Zero;


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

        }
        void Main(string argument, UpdateType updateSource)
        {
            // Get the flight seat
            var flightSeat = GridTerminalSystem.GetBlockWithName("Flight Seat") as IMyCockpit;
            if (flightSeat == null)
            {
                Echo("Error: Flight Seat not found!");
                return;
            }

            // Access the single LCD panel
            IMyTextSurface panel = flightSeat.GetSurface(0);
            IMyTextSurface lcd_left = GridTerminalSystem.GetBlockWithName("lcd_left_pitch") as IMyTextSurface;
            IMyTextSurface lcd_right = GridTerminalSystem.GetBlockWithName("lcd_right_roll") as IMyTextSurface;
            double altitude;
            // Get altitude, gravity, pitch, and roll
            Vector3D gravity = flightSeat.GetNaturalGravity();
            double gravityMagnitude = gravity.Length(); // Gravity in m/s²
            flightSeat.TryGetPlanetElevation(MyPlanetElevation.Surface, out altitude);
            Color color = Color.White;
            Vector3D shipUp = flightSeat.WorldMatrix.Up; // "Up" direction of the ship
            double pitch = Math.Asin(Vector3D.Dot(Vector3D.Cross(shipUp, gravity), flightSeat.WorldMatrix.Right)) * (180 / Math.PI);
            double roll = Math.Asin(Vector3D.Dot(Vector3D.Cross(shipUp, gravity), flightSeat.WorldMatrix.Forward)) * (180 / Math.PI);
            if (pitch > 1 || pitch < -1)
            {
                color = Color.Red;
            }
            if (roll > 1 || roll < -1)
            {
                color = Color.Red;
            }
            ConfigurePanel(panel, color);
            // Write data to the LCD panel
            string altitudeText = $"Altitude: {altitude:F1} m";
            string gravityText = $"Gravity: {gravityMagnitude:F2} m/s²";
            string pitchRollText = $"Pitch: {pitch:F1}° :: Roll: {roll:F1}°";

            panel.WriteText($"{altitudeText} :: {gravityText}\n{pitchRollText}");

            // Debug information
            Echo("Altitude, Gravity, and Pitch/Roll written to panel.");
        }

        void ConfigurePanel(IMyTextSurface panel, Color color)
        {
            if (panel == null) return;

            // Set up text formatting
            panel.ContentType = ContentType.TEXT_AND_IMAGE; // Ensure text mode
            panel.FontSize = 4.0f;                          // Adjust font size
            panel.Alignment = TextAlignment.CENTER;           // Left-align text
            panel.FontColor = color;                  // Text color
            panel.BackgroundColor = Color.Black;            // Background color
        }

    }
}
