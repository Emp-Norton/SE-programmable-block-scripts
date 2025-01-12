public Program()
{

}


string cameraName = "raycam";
string loggerLcd = "loggingLcd";
string outputLcd = "outputLcd";
string fireButton = "fireButton";
IMyCameraBlock camera;
IMyTextPanel logger;
IMyTextPanel output;
IMyButtonPanel button;

private T GetOrInitBlock<T>(T current, string name) where T : class, IMyTerminalBlock
{
    return current ?? GridTerminalSystem.GetBlockWithName(name) as T;
}
Vector3D GetDirectionVector(int pitch, int yaw)
{
    double pitchRad = MathHelper.ToRadians(pitch);
    double yawRad = MathHelper.ToRadians(yaw);

    double x = Math.Cos(pitchRad) * Math.Cos(yawRad);
    double y = Math.Sin(pitchRad);
    double z = Math.Cos(pitchRad) * Math.Sin(yawRad);

    return new Vector3D(x, y, z);
}
public void Main(string argument, UpdateType updateSource)
{
    camera = GetOrInitBlock(camera, cameraName);
    logger = GetOrInitBlock(logger, loggerLcd);
    output = GetOrInitBlock(output, outputLcd);
    button = GetOrInitBlock(button, fireButton);
    camera.EnableRaycast = true;
    int targetDist = 5000;
    for (var pitchAngle = 0; pitchAngle < 45; pitchAngle++) 
    {
        for (var yawAngle = 0; yawAngle < 45; yawAngle++) 
        {
            Echo($"Can scan? Dist: {targetDist}, pitch: {pitchAngle}, yaw: {yawAngle}\n{camera.CanScan(targetDist, pitchAngle, yawAngle)}");
        }
    }
}
