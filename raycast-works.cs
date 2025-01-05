public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

void Main(string argument, UpdateType updateSource)
{
    // Configuration
    string cameraName = "raycam"; // Name of your camera block
    string lcdName = "Camera LCD"; // Name of your LCD panel block
    double scanRange = 2900000; // Scan range in meters

    // Get the camera and LCD panel
    IMyCameraBlock camera = GridTerminalSystem.GetBlockWithName(cameraName) as IMyCameraBlock;
    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextPanel;

        Echo($"Range: {camera.AvailableScanRange.ToString()}");
    // Ensure the camera is enabled for raycasting
   camera.EnableRaycast = true;

    IMySoundBlock sound = GridTerminalSystem.GetBlockWithName("Sound Block") as IMySoundBlock;

    if (camera.AvailableScanRange >= (scanRange * 0.8)) { sound.Play(); } else { sound.Stop();}
    if (camera.AvailableScanRange >= scanRange){
        sound.Play();
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
