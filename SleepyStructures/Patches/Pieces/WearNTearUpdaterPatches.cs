using SleepyStructures.Model.Structures;

namespace SleepyStructures.Patches.Pieces;

/**
 * Patch for adding the Tick logic to the WearNTearUpdater class.
 */
public class WearNTearUpdaterPatches : IPatch
{
    public static bool DidTick;
    /**
     * <inheritdoc/>
     */
    public void InitPatch()
    {
        On.WearNTearUpdater.UpdateWearNTear += WearNTearUpdaterOnUpdateWearNTear;
    }

    /**
     * Adds the ticking logic prefix, and then adds any picked up pieces postfix to the <see cref="WearNTearManager"/>.
     */
    void WearNTearUpdaterOnUpdateWearNTear(On.WearNTearUpdater.orig_UpdateWearNTear orig, WearNTearUpdater self, float deltatime, float time)
    {
        DidTick = false;
        WearNTearManager.Tick();
        orig(self, deltatime, time);
        foreach (var piece in WearNTearManager.AddNow) WearNTearManager.FullyAddEntry(piece);
        WearNTearManager.AddNow.Clear();
    }
    
}
