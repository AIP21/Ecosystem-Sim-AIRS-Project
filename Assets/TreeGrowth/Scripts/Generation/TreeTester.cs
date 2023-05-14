using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeGrowth.Generation
{
    public class TreeTester : MonoBehaviour
    {
        public bool Generate = false;
        public bool GrowTick = false;
        public TreeParameters Parameters;

        private TreeGenerator gen;

        void Start()
        {
            gen = GetComponent<TreeGenerator>();
        }

        void Update()
        {
            Random.State origState = Random.state;
            Random.InitState(Parameters.Seed);

            if (Generate)
            {
                Generate = false;

                gen.Build(Parameters);
            }

            if (GrowTick)
            {
                GrowTick = false;

                gen.IterateGrowth(Parameters);
            }

            tickTree();

            Random.state = origState;
        }

        private void tickTree()
        {

        }
    }
}