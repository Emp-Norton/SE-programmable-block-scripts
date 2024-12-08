Vector3D targetPosition = Vector3D.Zero;
IMyTextSurface panel;
public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1; // Only run when triggered

    // Try to find the cockpit block (using standard cockpit screen if present)
    IMyShipController controller = GetShipController();
    if (controller == null) // TODO: Extend this to handle remote controls, cockpit, etc
    {
        Echo("No ship controller found.");
        return;
    }
    panel = GridTerminalSystem.GetBlockWithName("panel") as IMyTextSurface; //TODO: Nix the random magic strings and nums, use constants
    if (panel == null) {
        Echo("Can't find panel");
        return;
    } else {
        panel.ContentType = ContentType.TEXT_AND_IMAGE;
        panel.WriteText(GetSubstring("readyone", 2)); // TODO: Get rid of this testing leftover
    }

}

public void Main(string argument, UpdateType updateSource)
{
    // Ensure a GPS argument is passed to set the target
    if (!string.IsNullOrWhiteSpace(argument))
    {
        if (!SetTargetFromGPS(argument))
        {
            Echo("Invalid GPS argument.");
            return;
        }
        Echo("GPS target set. Calculating direction...");
    }

    // Get the ship's current position
    IMyShipController controller = GetShipController();
    if (controller == null)
    {
        Echo("No ship controller found.");
        return;
    }

    Vector3D currentPosition = controller.GetPosition();

    // Calculate direction to target
    Vector3D directionToTarget = targetPosition - currentPosition;

    // Log the direction vector to the cockpit screen
    
    string output = $"Current Position: {currentPosition}\n";
    output += $"Target Position: {targetPosition}\n";
    output += $"Direction to Target: {directionToTarget}\n";
    panel.WriteText(output);
    Echo(output);  // Also log to the console for debugging
}

public string GetSubstring(string inputStr, int n)
{
    try 
    {
        return inputStr.Substring(inputStr.Length - n);
    }
    catch
    {
        string error = "Failed for reasons";
        Echo(error);
        return error;
    }
}
// Set the target GPS position
public bool SetTargetFromGPS(string gpsString)
{
    try
    {
        var parts = gpsString.Split(':');
        if (parts.Length < 5 || parts[0] != "GPS") throw new Exception("Invalid GPS format");

        double x = double.Parse(parts[2]);
        double y = double.Parse(parts[3]);
        double z = double.Parse(parts[4]);

        targetPosition = new Vector3D(x, y, z);
        Echo($"Target set to: {targetPosition}");
        return true;
    }
    catch (Exception ex)
    {
        Echo($"Error parsing GPS: {ex.Message}");
        return false;
    }
}

// Retrieve the primary ship controller
private IMyShipController GetShipController()
{
    List<IMyShipController> controllers = new List<IMyShipController>();
    GridTerminalSystem.GetBlocksOfType(controllers);

    if (controllers.Count == 0)
    {
        Echo("No ship controller found!");
        return null;
    }

    return controllers[0];
}


