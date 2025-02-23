using System.Collections.Generic;
using UnityEngine;

namespace Occlusion.Patches;

public class PiecePatches
{
    public static readonly Dictionary<Piece, MeshRenderer[]> MeshCache = new();
    public static void Init()
    {
        On.Piece.Awake += PieceOnAwake;
    }

    private static void PieceOnAwake(On.Piece.orig_Awake orig, Piece self)
    {
        orig(self);
        MeshRenderer meshRenderer = self.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = self.GetComponentInParent<MeshRenderer>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = self.GetComponentInChildren<MeshRenderer>();
        }

        if (meshRenderer == null)
        {
            Debug.Log($"[Occlusion] Found MeshRenderer for {self.gameObject.name}");
            
        }
        else
        {
            Debug.LogWarning($"[Occlusion] No MeshRenderer found for {self.gameObject.name}");
        }
        if (!MeshCache.ContainsKey(self))
        {
            MeshRenderer[] meshRenderers = self.GetComponentsInChildren<MeshRenderer>();
            MeshCache[self] = meshRenderers;
        }
    }
}