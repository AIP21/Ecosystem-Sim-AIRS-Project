using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TreeGrowth.Generation;

namespace TreeGrowth
{
    [System.Serializable]
    public record TreeParameters
    {
        #region Growth Parameters
        [Header("Growth Parameters")]
        [Range(0.0f, 0.6f)]
        public float StemSize = 0.3f;

        [Space(5)]
        [Range(0.0f, 0.1f)]
        public float BranchSize = 0.1f;

        [Range(0.1f, 4f)]
        public float SizeFalloff = 0.9f;

        [Space(5)]
        [Range(0f, 5f)]
        public float BranchLength = 2f;
        [Range(0f, 5f)]
        public float TrunkLength = 2f;
        [Range(0f, 1f)]
        public float TrunkThreshold = 0.2f;

        [Range(0.75f, 1f)]
        public float BranchLengthFalloff = 0.9f;

        [Space(5)]
        [Range(0f, 360)]
        public float BranchAngle = 30f;

        [Range(0f, 1f)]
        public float RadialBias = 0.2f;

        [Range(0f, 2f)]
        public float AngleRandomness = 0.5f;
        #endregion

        #region Toggles
        [Header("Toggles")]
        public bool GenerateBranchMesh = true;
        public bool GenerateLeafMesh = true;
        [Space(5)]
        public bool GenerateLeafColliders = false;
        public bool GenerateBranchColliders = false;
        #endregion

        #region Thresholds
        [Header("Thresholds")]
        public float GrowThreshold = 60;
        public float PruneThreshold = 40;
        public float ReproduceThreshold = 80;

        public int ReproduceAge = 70;

        public int ReproduceAmount = 1;
        #endregion

        public LayerMask GroundLayerMask;

        #region Mesh Generation
        [Header("Mesh Generation")]
        [Range(2, 32)]
        public int QuadsPerLeaf = 6;

        [Range(0.2f, 3f)]
        public float LeafQuadRadius = 1.5f;

        [Range(0.1f, 0.5f)]
        public float LeafColliderSize = 0.2f;

        [Space(5)]
        public int MeshSubdivisions = 5;

        public int BranchColliderDepth = 6;
        #endregion

        #region Genetics
        [Header("Genetics")]
        [Range(0f, 1f)]
        public float MutationAmount = 0.1f;

        [Range(0f, 1f)]
        public float MutationRate = 0.1f;
        #endregion

        #region Other
        [Header("Other")]
        [Range(10, 1000)]
        public int Iterations = 100;

        [Range(1, 20)]
        public int BatchSize = 5;

        public int MaxBranchesPerNode = 3;

        public float PrunePercentage = 0.06f;

        [Space(5)]
        public int Seed = 0;
        #endregion

        public TreeParameters Copy()
        {
            return new TreeParameters()
            {
                QuadsPerLeaf = QuadsPerLeaf,
                StemSize = StemSize,
                BranchSize = BranchSize,
                SizeFalloff = SizeFalloff,
                AngleRandomness = AngleRandomness,
                BranchLength = BranchLength,
                TrunkLength = TrunkLength,
                TrunkThreshold = TrunkThreshold,
                RadialBias = RadialBias,
                LeafColliderSize = LeafColliderSize,
                BranchAngle = BranchAngle,
                BranchLengthFalloff = BranchLengthFalloff,
                Iterations = Iterations,
                GenerateLeafMesh = GenerateLeafMesh,
                GenerateLeafColliders = GenerateLeafColliders,
                GenerateBranchColliders = GenerateBranchColliders,
                BranchColliderDepth = BranchColliderDepth,
                LeafQuadRadius = LeafQuadRadius,
                MaxBranchesPerNode = MaxBranchesPerNode,
                MeshSubdivisions = MeshSubdivisions,
                BatchSize = BatchSize,
                GenerateBranchMesh = GenerateBranchMesh,
                GrowThreshold = GrowThreshold,
                PruneThreshold = PruneThreshold,
                ReproduceThreshold = ReproduceThreshold,
                ReproduceAge = ReproduceAge,
                ReproduceAmount = ReproduceAmount,
                PrunePercentage = PrunePercentage,
                MutationAmount = MutationAmount,
                MutationRate = MutationRate,
                GroundLayerMask = GroundLayerMask,
                Seed = Seed
            };
        }

        public TreeParameters Mutate()
        {
            TreeParameters newParams = Copy();
            
            newParams.QuadsPerLeaf = MutateInt(QuadsPerLeaf, 2, 32, MutationRate, MutationAmount);
            newParams.StemSize = MutateFloat(StemSize, 0.0f, 0.6f, MutationRate, MutationAmount);
            newParams.BranchSize = MutateFloat(BranchSize, 0.0f, 0.1f, MutationRate, MutationAmount);
            newParams.SizeFalloff = MutateFloat(SizeFalloff, 0.1f, 1f, MutationRate, MutationAmount);
            newParams.AngleRandomness = MutateFloat(AngleRandomness, 0.0f, 1.2f, MutationRate, MutationAmount);
            newParams.BranchLength = MutateFloat(BranchLength, 0.0f, 2f, MutationRate, MutationAmount);
            newParams.TrunkLength = MutateFloat(TrunkLength, 0.0f, 3f, MutationRate, MutationAmount);
            newParams.TrunkThreshold = MutateFloat(TrunkThreshold, 0.0f, 1f, MutationRate, MutationAmount);
            newParams.RadialBias = MutateFloat(RadialBias, 0.0f, 1f, MutationRate, MutationAmount);
            newParams.LeafColliderSize = MutateFloat(LeafColliderSize, 0.1f, 0.5f, MutationRate, MutationAmount);
            newParams.BranchAngle = MutateFloat(BranchAngle, 0.0f, 90f, MutationRate, MutationAmount);
            newParams.BranchLengthFalloff = MutateFloat(BranchLengthFalloff, 0.75f, 1f, MutationRate, MutationAmount);
            newParams.LeafQuadRadius = MutateFloat(LeafQuadRadius, 0.2f, 1.5f, MutationRate, MutationAmount);
            newParams.MutationAmount = MutateFloat(MutationAmount, 0.0f, 1f, MutationRate, MutationAmount);
            newParams.MutationRate = MutateFloat(MutationRate, 0.0f, 1f, MutationRate, MutationAmount);
            newParams.GrowThreshold = MutateFloat(GrowThreshold, 0.0f, 1f, MutationRate, MutationAmount);
            newParams.PruneThreshold = MutateFloat(PruneThreshold, 0.0f, 1f, MutationRate, MutationAmount);
            newParams.ReproduceThreshold = MutateFloat(ReproduceThreshold, 0.0f, 1f, MutationRate, MutationAmount);
            newParams.ReproduceAge = MutateInt(ReproduceAge, 0, 100, MutationRate, MutationAmount);
            newParams.PrunePercentage = MutateFloat(PrunePercentage, 0.0f, 0.5f, MutationRate, MutationAmount);
            newParams.ReproduceAmount = MutateInt(ReproduceAmount, 0, 10, MutationRate, MutationAmount);
            

            return newParams;
        }

        public static TreeParameters Random(){
            TreeParameters newParams = new TreeParameters();
            
            newParams.QuadsPerLeaf = UnityEngine.Random.Range(2, 32);
            newParams.StemSize = UnityEngine.Random.Range(0.0f, 0.6f);
            newParams.BranchSize = UnityEngine.Random.Range(0.0f, 0.1f);
            newParams.SizeFalloff = UnityEngine.Random.Range(0.1f, 1f);
            newParams.AngleRandomness = UnityEngine.Random.Range(0.0f, 1.2f);
            newParams.BranchLength = UnityEngine.Random.Range(0.0f, 2f);
            newParams.TrunkLength = UnityEngine.Random.Range(0.0f, 3f);
            newParams.TrunkThreshold = UnityEngine.Random.Range(0.0f, 1f);
            newParams.RadialBias = UnityEngine.Random.Range(0.0f, 1f);
            newParams.LeafColliderSize = UnityEngine.Random.Range(0.1f, 0.5f);
            newParams.BranchAngle = UnityEngine.Random.Range(0.0f, 90f);
            newParams.BranchLengthFalloff = UnityEngine.Random.Range(0.75f, 1f);
            newParams.LeafQuadRadius = UnityEngine.Random.Range(0.2f, 1.5f);
            newParams.MutationAmount = UnityEngine.Random.Range(0.0f, 1f);
            newParams.MutationRate = UnityEngine.Random.Range(0.0f, 1f);
            newParams.GrowThreshold = UnityEngine.Random.Range(0.0f, 1f);
            newParams.PruneThreshold = UnityEngine.Random.Range(0.0f, 1f);
            newParams.ReproduceThreshold = UnityEngine.Random.Range(0.0f, 1f);
            newParams.ReproduceAge = UnityEngine.Random.Range(0, 100);
            newParams.PrunePercentage = UnityEngine.Random.Range(0.0f, 0.5f);
            newParams.ReproduceAmount = UnityEngine.Random.Range(0, 10);

            return newParams;
        }

        private float MutateFloat(float value, float min, float max, float mutationRate, float mutationAmount)
        {
            float newValue = value;

            if (UnityEngine.Random.Range(0f, 1f) < mutationRate)
                newValue = value + UnityEngine.Random.Range(-1f, 1f) * (max - min) * mutationAmount;

            return newValue;
        }

        private int MutateInt(int value, int min, int max, float mutationRate, float mutationAmount)
        {
            int newValue = value;

            if (UnityEngine.Random.Range(0f, 1f) < mutationRate)
                newValue = value + (int)(UnityEngine.Random.Range(-1f, 1f) * (max - min) * mutationAmount);

            return newValue;
        }
    }
}