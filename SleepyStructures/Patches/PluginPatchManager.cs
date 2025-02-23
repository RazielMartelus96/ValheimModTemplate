using System.Collections.Generic;

namespace SleepyStructures.Patches;

/**
 * Manger for handling the registration and initialisation of <see cref="IPatch"> Valheim Patches</see>.
 */
public class PluginPatchManager
{
    List<IPatch> patches = new();
    
    /**
     * Private constructor to prevent construction outside the Instance call. 
     */
    private PluginPatchManager()
    {
    }
    
    /**
     * Getter for the manager, as per the Singleton Pattern. 
     */
    public static PluginPatchManager Instance { get; } = new PluginPatchManager();

    /**
     * Registers a patch with the manager. 
     */
    public void RegisterPatch(IPatch patch)
    {
        if (patches.Contains(patch))
            return;
        patches.Add(patch);
    }

    /**
     * Unregisters a patch from the manager, could have a potential use in future for removing patches before
     * they are initialised (if certain logic is met, such as certain config params perhaps?) 
     */
    public void UnregisterPatch(IPatch patch)
    {
        if (!patches.Contains(patch))
            return;
        patches.Remove(patch);
    }

    public void InitPatches()
    {
        foreach (var patch in patches)
        {
            patch.InitPatch();
        }
    }
    
    
}
