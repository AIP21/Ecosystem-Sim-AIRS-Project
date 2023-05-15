using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeGrowth.Generation
{
    public class TreeTester : MonoBehaviour
    {
        public bool Generate = false;
        public bool GrowTick = false;
        public bool Reset = false;

        public TreeParameters Parameters;


        private TreeGenerator gen;

        private void Start()
        {
            gen = GetComponent<TreeGenerator>();
        }


        int tick = 0;

        private void FixedUpdate()
        {
            if (tick++ % 4 != 0)
                return;

            Random.State origState = Random.state;
            Random.InitState(Parameters.Seed);

            // if (Generate)
            // {
            // Generate = false;

            gen.Build(Parameters);
            // }

            if (GrowTick)
            {
                GrowTick = false;

                gen.IterateGrowth(Parameters);
            }

            if (Reset)
            {
                Reset = false;

                gen.Reset();
            }

            tickTree();

            Random.state = origState;
        }

        private void tickTree()
        {

        }
    }
}