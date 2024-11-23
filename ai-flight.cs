Vector3D targetPosition = new Vector3D(0, 0, 0); // Default target position
double targetAltitude = 100; // Desired altitude above the ground
Vector3D currentPosition;
Vector3D currentVelocity = Vector3D.Zero;

// PID Constants
double Kp = 0.1;
double Ki = 0.01;
double Kd = 0.05;

// Error variables
Vector3D previousError = Vector3D.Zero;
Vector3D integral = Vector3D.Zero;

// Tolerance for altitude and position
double altitudeTolerance = 5.0;
double positionTolerance = 10.0;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1; // Run script every tick
}

public bool SetTargetFromGPS(string gpsString)
{
    try
    {
        var parts = gpsString.Split(':');
        if (parts.Length < 5 || parts[0] != "GPS")
            throw new Exception("Invalid GPS format");

        double x = double.Parse(parts[2]);
        double y = double.Parse(parts[3]);
        double z = double.Parse(parts[4]);

        targetPosition = new Vector3D(x, y, z);
        return true;
    }
    catch (Exception ex)
    {
        Echo($"Error parsing GPS: {ex.Message}");
        return false;
    }
}

public void ApplyThrust(Vector3D thrustVector)
{
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType(thrusters);

    double thrustMagnitude = thrustVector.Length();
    if (thrustMagnitude > 0)
        thrustVector.Normalize();

    foreach (var thruster in thrusters)
    {
        Vector3D thrusterForward = thruster.WorldMatrix.Forward;
        double thrustPower = Vector3D.Dot(thrustVector, thrusterForward) * thrustMagnitude;
        thruster.ThrustOverride = (float)Math.Max(thrustPower, 0);
	Echo($"Thruster: {thruster.Name} - {thrustPower}");
    }
}

public void Main(string argument, UpdateType updateSource)
{
    // If GPS argument is passed, update target position
    if (!string.IsNullOrEmpty(argument))
    {
        if (SetTargetFromGPS(argument))
        {
            Echo($"New Target Set: {targetPosition}");
        }
        else
        {
            Echo("Failed to set target from GPS.");
            return;
        }
    }

    // Get the current position of the grid from a ship controller
    IMyShipController controller = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyShipController;
    if (controller == null)
    {
        Echo("No cockpit or remote control block found!");
        return;
    }

    currentPosition = controller.GetPosition();
    currentVelocity = controller.GetShipVelocities().LinearVelocity;

    // Gravity compensation
    Vector3D gravity = controller.GetNaturalGravity();

    // Determine current altitude
    double currentAltitude = 0;
    if (!controller.TryGetPlanetElevation(MyPlanetElevation.Surface, out currentAltitude))
    {
        Echo("No planet detected, flying in space.");
        currentAltitude = double.MaxValue; // Set to a high value in space
    }

    // Check if altitude is within tolerance
    bool altitudeReached = Math.Abs(targetAltitude - currentAltitude) <= altitudeTolerance;

    if (!altitudeReached)
    {
        // Adjust the target position to maintain the desired altitude
        double altitudeDifference = targetAltitude - currentAltitude;
        Vector3D altitudeCorrection = Vector3D.Up * altitudeDifference;

        // Apply altitude correction thrust
        ApplyThrust(altitudeCorrection - gravity);
        Echo($"Adjusting altitude: {currentAltitude} -> {targetAltitude}");
        return; // Exit to wait for altitude adjustment
    }

    // Altitude reached, now move toward target
    Echo($"Reached altitude.");
    Vector3D error = targetPosition - currentPosition;

    // Check if position is within tolerance
    if (error.Length() <= positionTolerance)
    {
        // Stop all thrusters
        List<IMyThrust> thrusters = new List<IMyThrust>();
        GridTerminalSystem.GetBlocksOfType(thrusters);
        foreach (var thruster in thrusters)
        {
            thruster.ThrustOverride = 0;
        }

        Echo("Target reached!");
        return;
    }

    // Calculate PID error
    Vector3D derivative = error - previousError;
    Vector3D proportional = error * Kp;
    integral += error * Ki;
    Vector3D derivativeTerm = derivative * Kd;

    // Compute thrust with PID terms
    Vector3D output = proportional + integral + derivativeTerm;

    // Compensate for gravity
    Vector3D compensatedThrust = output - gravity;

    // Clamp thrust to avoid overcorrection
    double maxThrust = 100000; // Adjust based on ship's thrust capabilities
    if (compensatedThrust.Length() > maxThrust)
    {
        compensatedThrust = Vector3D.Normalize(compensatedThrust) * maxThrust;
    }

    // Apply the calculated thrust
    ApplyThrust(compensatedThrust);

    // Update previous error
    previousError = error;

    // Debugging information
    Echo($"Target: {targetPosition}");
    Echo($"Current Position: {currentPosition}");
    Echo($"Current Altitude: {currentAltitude}");
    Echo($"Error: {error}");
    Echo($"Compensated Thrust: {compensatedThrust}");
}

