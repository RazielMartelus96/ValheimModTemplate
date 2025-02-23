using SleepyStructures.Model.Structures;

namespace SleepyStructures.Patches.Pieces;
/**
 * Patch for ensuring that WearNTear Logic utilising pieces are both added, and utilise the logic of, the
 * <see cref="WearNTearManager"/> during their awake call and Support Update call respectively. 
 */
public class WearNTearPatches : IPatch
{

    /**
     * <inheritdoc/>
     */
    public void InitPatch()
    {
        On.WearNTear.Awake += WearNTearOnAwake;
        On.WearNTear.Destroy += WearNTearOnDestroy;
        On.WearNTear.UpdateSupport += WearNTearOnUpdateSupport;
    }
    

    void WearNTearOnAwake(On.WearNTear.orig_Awake orig, WearNTear self)
    {
        WearNTearManager.AddEntry(self);
        orig(self);
    }

    
    void WearNTearOnDestroy(On.WearNTear.orig_Destroy orig, WearNTear self, HitData hitData, bool blockDrop)
    {
        WearNTearManager.RemoveEntry(self);
        orig(self, hitData, blockDrop);
    }

    /**
     * Ensures only a single tick is performed and, if the current piece is not currently ticking, skip the structure
     * check.
     */
    void WearNTearOnUpdateSupport(On.WearNTear.orig_UpdateSupport orig, WearNTear self)
    {
        if (!WearNTearManager.Ticking.Contains(self))
        {
            if (!WearNTearUpdaterPatches.DidTick)
            {
                WearNTearUpdaterPatches.DidTick = true;
                WearNTearManager.Tick();
                if (!WearNTearManager.Ticking.Contains(self)) return;
            }
            else
            {
                return;
            }
        }
        WearNTearManager.Ticking.Remove(self);
        if (WearNTearManager.AwaitingInit.Contains(self)) WearNTearManager.AddNow.Add(self);
        orig(self);
        


    }
}
