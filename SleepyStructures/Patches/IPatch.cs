namespace SleepyStructures.Patches;

/**
 * Interface representing the general functionality of a class whose purpose is to patch the base game via Harmony /
 * MMHOOK. This allows for a more controlled init of patches during the initial enable of the plugin. Also allows for
 * a centralised manager for both MMHOOK and Harmony based patches (if one is so inclined). 
 */
public interface IPatch
{
    /**
     * Initialises the Patch. 
     */
    void InitPatch();
}
