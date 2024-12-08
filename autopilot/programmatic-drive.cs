Vector3D targetPosition = new Vector3D(0, 0, 0); // Default target position
Vector3D currentPosition;
Vector3D currentVelocity = Vector3D.Zero;

// PID Constants
double Kp = 0.1;
double Ki = 0.01;
double Kd = 0.05;

// Error variables
Vector3D previousError = Vector3D.Zero;
Vector3D integral = Vector3D.Zero;

// Tolerance for reaching target
double positionTolerance = 2.0; // Stop within 2 meters of the target

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

public void ApplySteeringAndDrive(Vector3D movementVector)
{
    List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();
    GridTerminalSystem.GetBlocksOfType(wheels);

    if (wheels.Count == 0)
    {
        Echo("No wheels found!");
        return;
    }

    // Normalize movement vector to determine direction
    if (movementVector.Length() > 0)
        movementVector.Normalize();

    // Get forward and right vectors from the rover's orientation
    IMyShipController controller = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyShipController;
    if (controller == null)
    {
        Echo("No remote control found!");
        return;
    }
    Vector3D forwardVector = controller.WorldMatrix.Forward; // Forward direction of the rover
    Vector3D rightVector = controller.WorldMatrix.Right;     // Right direction of the rover

    // Calculate steering (dot product of target direction and right vector)
    double steering = Vector3D.Dot(movementVector, rightVector);

    // Apply steering and power to wheels
    foreach (var wheel in wheels)
    {
        if (wheel.IsAttached)
        {
            wheel.SteeringOverride = (float)steering; // Steering depends on direction
            wheel.PropulsionOverride = (float)(movementVector.Length() > 0 ? 1.0 : 0.0); // Full forward propulsion
        }
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
        Echo("No remote control block found!");
        return;
    }

    currentPosition = controller.GetPosition();
    currentVelocity = controller.GetShipVelocities().LinearVelocity;

    // Calculate error vector in the X-Z plane (ignore Y axis for ground movement)
    Vector3D error = targetPosition - currentPosition;
    error.Y = 0; // Eliminate vertical component

    // Check if position is within tolerance
    if (error.Length() <= positionTolerance)
    {
        // Stop all movement
        List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();
        GridTerminalSystem.GetBlocksOfType(wheels);
        foreach (var wheel in wheels)
        {
            if (wheel.IsAttached)
            {
                wheel.PropulsionOverride = 0.0f; // Stop propulsion
                wheel.SteeringOverride = 0.0f;  // Reset steering
            }
        }

        Echo("Target reached!");
        return;
    }

    // Calculate PID error
    Vector3D derivative = error - previousError;
    Vector3D proportional = error * Kp;
    integral += error * Ki;
    Vector3D derivativeTerm = derivative * Kd;

    // Compute movement vector with PID terms
    Vector3D output = proportional + integral + derivativeTerm;

    // Apply movement vector to wheels
    ApplySteeringAndDrive(output);

    // Update previous error
    previousError = error;

    // Debugging information
    Echo($"Target: {targetPosition}");
    Echo($"Current Position: {currentPosition}");
    Echo($"Error: {error}");
    Echo($"Movement Vector: {output}");
}
