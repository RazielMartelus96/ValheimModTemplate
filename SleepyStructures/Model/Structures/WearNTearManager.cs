using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SleepyStructures.Model.Structures
{
    //TODO Get the motivation to actually fully document this class. 
    public class WearNTearManager
    {
        static int debugCounter = 0;
        static readonly int debugFrequency = 50;
        static readonly int NumTicksBeforeSleep = 50; // needs to be high, ticking is weird

        public static readonly Dictionary<WearNTear, WearNTearTracked> PieceToTracked = new Dictionary<WearNTear, WearNTearTracked>();

        public static readonly HashSet<StructureData> Structures = new HashSet<StructureData>();

        public static readonly HashSet<WearNTear> Ticking = new HashSet<WearNTear>();
        public static readonly HashSet<WearNTear> AwaitingInit = new HashSet<WearNTear>();
        public static readonly HashSet<WearNTear> AddNow = new HashSet<WearNTear>();
        public static readonly HashSet<WearNTearTracked> MaxSupport = new HashSet<WearNTearTracked>();

        
        public static WearNTearTracked GetStructure(WearNTear piece)
        {
            PieceToTracked.TryGetValue(piece, out var tracked);
            return tracked;
        }

        public static void AddEntry(WearNTear piece)
        {
            AwaitingInit.Add(piece);
        }

        public static void FullyAddEntry(WearNTear piece)
        {
            AwaitingInit.Remove(piece);

            // precaution: make sure the piece isn't destroyed
            if (piece == null)
            {
                Debug.Log("[FastStructure] Removing destroyed piece before it can pollute list.");
                RemoveEntry(piece);
                return;
            }

            piece.GetMaterialProperties(out var maxSupport, out _, out _, out _);
            var tracked = new WearNTearTracked
            {
                Wear = piece,
                LastSupport = piece.m_support,
                MaxSupport = maxSupport
            };

            PieceToTracked[piece] = tracked;

            var nearbyStructures = new HashSet<StructureData>();

            foreach (var collider in piece.m_supportColliders)
            {
                var t = collider.GetComponentInParent<WearNTear>();
                if (t != null)
                {
                    var otherTracked = GetStructure(t);
                    if (otherTracked != null && otherTracked.Structure != null)
                        nearbyStructures.Add(otherTracked.Structure);
                }
            }

            if (nearbyStructures.Count == 0)
            {
                var structure = new StructureData();
                structure.Pieces.Add(tracked);
                Structures.Add(structure);
                tracked.Structure = structure;
            }
            else if (nearbyStructures.Count == 1)
            {
                var structure = nearbyStructures.First();
                structure.Pieces.Add(tracked);
                structure.IdleTicks = 0;
                structure.Sleeping = false;
                tracked.Structure = structure;
            }
            else
            {
                var mainStructure = nearbyStructures.First();
                nearbyStructures.Remove(mainStructure);
                foreach (var structure in nearbyStructures)
                {
                    mainStructure.Pieces.AddRange(structure.Pieces);
                    foreach (var p in structure.Pieces)
                    {
                        p.Structure = mainStructure;
                    }
                    Structures.Remove(structure);
                }
                mainStructure.Pieces.Add(tracked);
                mainStructure.IdleTicks = 0;
                mainStructure.Sleeping = false;
                tracked.Structure = mainStructure;
            }
        }

        public static void RemoveEntry(WearNTear piece)
        {
            AwaitingInit.Remove(piece);
            AddNow.Remove(piece);

            if (PieceToTracked.TryGetValue(piece, out var tracked))
            {
                var structure = tracked.Structure;
                if (structure != null)
                {
                    structure.Pieces.Remove(tracked);
                    structure.IdleTicks = 0;
                    structure.Sleeping = false;
                    if (structure.Pieces.Count == 0)
                        Structures.Remove(structure);
                }
                PieceToTracked.Remove(piece);
            }
        }

        public static void Tick()
        {
            Ticking.Clear();
            foreach (var piece in AwaitingInit)
            {
                Ticking.Add(piece);
            }

            foreach (var structure in Structures)
            {
                if (structure.Sleeping)
                {
                    foreach (var tracked in structure.Pieces)
                    {
                        bool contains = MaxSupport.Contains(tracked);
                        if (Mathf.Approximately(tracked.Wear.m_support, tracked.MaxSupport))
                        {
                            Ticking.Add(tracked.Wear); 
                            if (!contains)
                                MaxSupport.Add(tracked);
                        }
                        else if (contains)
                        {
                            tracked.LastSupport = tracked.Wear.m_support;
                            structure.Sleeping = false;
                            structure.IdleTicks = 0;
                            Ticking.Add(tracked.Wear);
                            MaxSupport.Remove(tracked);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < structure.Pieces.Count; i++)
                    {
                        var tracked = structure.Pieces[i];
                        float diff = tracked.Wear.m_support - tracked.LastSupport;

                        if (diff > 0)
                        {
                            tracked.LastSupport = tracked.Wear.m_support;
                        }
                        else if (diff < -0.001f)
                        {
                            tracked.LastSupport = tracked.Wear.m_support;
                            structure.IdleTicks = 0;
                        }

                        Ticking.Add(tracked.Wear);

                        if (i == structure.Pieces.Count - 1)
                            structure.IdleTicks++;

                        bool contains = MaxSupport.Contains(tracked);
                        if (Mathf.Approximately(tracked.LastSupport, tracked.MaxSupport))
                        {
                            if (!contains)
                                MaxSupport.Add(tracked);
                        }
                        else if (contains)
                        {
                            MaxSupport.Remove(tracked);
                            structure.IdleTicks = 0;
                        }
                    }
                    if (structure.IdleTicks >= NumTicksBeforeSleep)
                        structure.Sleeping = true;
                }
            }
            DebugConsistencyCheck();
        }
        
        static void DebugConsistencyCheck()
        {
            if (debugCounter++ % debugFrequency != 0) return; // Reduce frequency

            int structureCount = Structures.Count;
            int trackedCount = PieceToTracked.Count;
            int sumOfPieces = Structures.Sum(s => s.Pieces.Count);

            if (trackedCount != sumOfPieces)
            {
                Debug.LogError($"[WearNTearManager] Mismatch detected! " +
                               $"Tracked: {trackedCount}, Summed Pieces: {sumOfPieces}, Structures: {structureCount}");
            }
            else
            {
                Debug.Log($"[WearNTearManager] Found {structureCount} structures.");
            }
        }
    }
}
