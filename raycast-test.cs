public Program()
{
    // The constructor, called only once every session and
    // always before any other method is called. Use it to
    // initialize your script. 
    //     
    // The constructor is optional and can be removed if not
    // needed.
    // 
    // It's recommended to set RuntimeInfo.UpdateFrequency 
    // here, which will allow your script to run itself without a 
    // timer block.
  //  Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Save()
{
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means. 
    // 
    // This method is optional and can be removed if not
    // needed.
}
double SCAN_DISTANCE = 10000;
float PITCH = 0;
float YAW = 0;
 
 bool firstrun = true;
 MyDetectedEntityInfo info;
StringBuilder sb = new StringBuilder();

public void Main(string argument)
{
    IMyCockpit cockpit = GridTerminalSystem.GetBlockWithName("Flight Seat") as IMyCockpit;
    IMyTextSurface cpanel = cockpit.GetSurface(0);
    cpanel.WriteText("Ready");
    IMyCameraBlock camera = GridTerminalSystem.GetBlockWithName("raycam") as IMyCameraBlock;
 IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName("ray") as IMyTextPanel;
  if (firstrun)
  {
  firstrun = false;
   
 
 if (camera == null) Echo("No cam");

  camera.EnableRaycast = true;
    Echo("Enabled");
  }

  if (camera.CanScan(SCAN_DISTANCE)) {
  info = camera.Raycast(SCAN_DISTANCE,PITCH,YAW);

  sb.Clear();
  sb.Append("EntityID: " + info.EntityId);
  sb.AppendLine();
  sb.Append("Name: " + info.Name);
  sb.AppendLine();
  sb.Append("Type: " + info.Type);
  sb.AppendLine();
  sb.Append("Velocity: " + info.Velocity.ToString("0.000"));
  sb.AppendLine();
  sb.Append("Relationship: " + info.Relationship);
  sb.AppendLine();
  sb.Append("Size: " + info.BoundingBox.Size.ToString("0.000"));
  sb.AppendLine();
  sb.Append("Position: " + info.Position.ToString("0.000"));

  if(info.HitPosition.HasValue)
  {
    cpanel.WriteText(info.ToString());
  sb.AppendLine();
  sb.Append("Hit: " + info.HitPosition.Value.ToString("0.000"));
  sb.AppendLine();
  sb.Append("Distance: " + Vector3D.Distance(camera.GetPosition(), info.HitPosition.Value).ToString("0.00"));
  }

  sb.AppendLine();
    string availableRangeStr = "Range: " + camera.AvailableScanRange.ToString();
    cpanel.WriteText($"Scan range: {camera.AvailableScanRange.ToString()}");
  sb.Append(availableRangeStr);
    if (lcd != null) {
  lcd.WriteText(sb.ToString());
    } else {
        Echo("no lcd");
        Echo(sb.ToString());
    }
}

}
