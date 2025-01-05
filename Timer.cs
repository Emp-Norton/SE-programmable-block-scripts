public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public bool IsLocal(IMyCubeBlock block)
{
   bool local = block.CubeGrid.EntityId == Me.CubeGrid.EntityId;
    return local;
}

public void HandleLightSequence(List<IMyLightingBlock> lights)
{
    var idx = DateTime.UtcNow.Second % lights.Count;
    var next = (idx + 1) % lights.Count();

    lights[idx].Enabled = false;
    lights[next].Enabled = true;
}

public void Main()
{

    List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlocksOfType(lights, light => IsLocal(light) && light.CustomName.Contains("Corner Light"));
    HandleLightSequence(lights);

}
