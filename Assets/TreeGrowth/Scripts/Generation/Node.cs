using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;
using Random = UnityEngine.Random;

namespace TreeGrowth.Generation
{
    public class Node
    {
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        public readonly TreeGenerator Tree;

        public readonly int Depth;
        public float Energy;

        public readonly Node Parent;
        public Node[] Children;

        public SphereCollider LeafCollider;
        public Vector3 MeshOrientation;

        public int SubtreeSize
        {
            get;
            private set;
        }

        public Node(Vector3 position, TreeGenerator tree)
        {
            this.Tree = tree;
            this.Position = position;
            this.Direction = Vector3.up;
            this.Depth = 0;
            this.Children = new Node[] { };
        }

        public Node(Vector3 position, Node parent)
        {
            this.Tree = parent.Tree;
            this.Position = position;
            this.Direction = (this.Position - parent.Position).normalized;
            this.Depth = parent.Depth + 1;
            this.Children = new Node[] { };
            parent.Children = parent.Children.Concat(new Node[] { this }).ToArray();
            this.CreateLeafCollider();
            this.Parent = parent;

        }

        public void CreateLeafCollider()
        {
            this.LeafCollider = this.Tree.LeafColliders.AddComponent<SphereCollider>();
            this.LeafCollider.center = this.Position;
            this.LeafCollider.radius = this.Tree.parameters.LeafColliderSize;
        }

        public void RemoveLeafCollider()
        {
            if (this.LeafCollider == null)
                return;

            GameObject.DestroyImmediate(this.LeafCollider);
        }

        public IEnumerable<Node> GetTree()
        {
            yield return this;

            Node currentNode = this;
            Stack<int> positions = new Stack<int>();
            int currentPosition = 0;

            while (true)
            {
                if (currentPosition < currentNode.Children.Length)
                {
                    Node child = currentNode.Children[currentPosition];

                    currentPosition++;

                    yield return child;

                    if (child.Children.Length > 0)
                    {
                        currentNode = child;
                        positions.Push(currentPosition);
                        currentPosition = 0;
                    }
                }
                else if (currentNode.Parent != null)
                {
                    currentPosition = positions.Pop();
                    currentNode = currentNode.Parent;
                }
                else
                {
                    yield break;
                }
            }
        }

        /**
        Try to branch off of this branch. Only for if this branch already has any children.
        **/
        public void Branch()
        {
            if (this.Children.Count() == 0) // If this branch already has any children
                return;

            // The math for the falloff is as follows: (branch length falloff param ^ depth of this branch in the tree)
            float nextLengthFalloffMult = Mathf.Pow(this.Tree.parameters.BranchLengthFalloff, this.Depth);

            // Get the length of a new branch if it were to grow off of this one
            // The math is as follows: (branch length param * branch length falloff multiplier)
            // That falloff makes it so that branches get shorter the farther out in the tree they are
            float nextLength = this.Tree.parameters.BranchLength * nextLengthFalloffMult;

            // Get the direction of the first child branch (the one created by Grow())
            Vector3 childDir = this.Children[0].Direction;

            // Take the direction of the first child branch and slightly randomize it
            // This is to make sure that the new branch doesn't grow in the exact same direction as the first child branch
            // The math is as follows:
            // (direction of first child branch * cos(branch angle param in rad) + cross product of direction of first child branch and a random direction * sign of branch angle param in deg)
            Func<Vector3> jitterFunc = () => childDir * Mathf.Cos(this.Tree.parameters.BranchAngle * Mathf.Deg2Rad) + Vector3.Cross(childDir, Random.onUnitSphere) * Mathf.Sign(this.Tree.parameters.BranchAngle * Mathf.Rad2Deg); // TODO: Why is this converting from degrees to degrees???

            // Get the optimal direction for a new branch to grow in
            // You pass a vector function that slightly randomizes the direction a bit every attempt
            // You also pass a minimum distance for that direction to be valid
            Vector3 direction = this.getGrowthDirection(jitterFunc, nextLength);

            if (!Mathf.Approximately(direction.magnitude, 0))
            {
                new Node(this.Position + direction * nextLength, this);
            }
        }

        /**
        Try to grow a new branch from this one. Only for if this branch doesn't already have any children.
        **/
        public void Grow()
        {
            if (this.Children.Count() != 0) // If this branch doesn't already have any children
                return;

            // The math for the falloff is as follows: (branch length falloff param ^ depth of this branch in the tree)
            float nextLengthFalloffMult = Mathf.Pow(this.Tree.parameters.BranchLengthFalloff, this.Depth);

            // Get the length of a new branch if it were to grow off of this one
            // The math is as follows: (branch length param * branch length falloff multiplier)
            // That falloff makes it so that branches get shorter the farther out in the tree they are
            float nextLength = this.Tree.parameters.BranchLength * nextLengthFalloffMult;

            // Get the optimal direction for a new branch to grow in
            // You pass a vector function that slightly randomizes the direction a bit every attempt:
            // (current branch direction + branch randomness) where branch randomness: (distort param * random direction)
            // You also pass a minimum distance for that direction to be valid
            Vector3 nextDirection = this.getGrowthDirection(() => this.Direction + this.Tree.parameters.Distort * Random.onUnitSphere, nextLength);

            // If the returned direction is zero, then there is no possible valid direction for a new branch. So don't create one.
            if (!Mathf.Approximately(nextDirection.magnitude, 0))
                new Node(this.Position + nextDirection * nextLength, this);
        }

        /**
        Get the optimal direction for a new branch to grow in
        You pass a vector function that is used to jitter the direction a bit every attempt
        **/
        private Vector3 getGrowthDirection(Func<Vector3> vectorGenerator, float minDistance)
        {
            Random.InitState(this.Tree.parameters.Seed);

            Vector3 result = Vector3.zero;
            float longestDistance = minDistance;

            // Perform at most 20 attempts to find the optimal growth direction
            // With every attempt it randomized the direction a bit and checks if there is an obstacle in the way
            // If there is no obstacle in the way, then instantly return that test direction
            // If not, then check if that obstacle is farther away than the current farthest obstacle
            // If not, then discard that attempt
            // If so, then keep track of that distance and the direction that caused it
            for (int i = 0; i < 20; i++)
            {
                // Random.InitState(this.Tree.parameters.Seed);

                Vector3 growthDirection = vectorGenerator.Invoke().normalized; // Get the current direction + a bit of randomization
                float range = this.raycast(this.Position, growthDirection, 0.01f); // Get the distance to the nearest object in the growth direction

                if (System.Single.IsPositiveInfinity(range)) // There is no obstacle in the way, so just return the growth direction
                    return growthDirection;

                // If there is an obstacle in the way, but it's farther away than the given minimum distance for this raycast to be valid, then this raycast is still valid.
                if (range > longestDistance)
                {
                    result = growthDirection;
                    longestDistance = range;
                }
            }

            // Return the best possible direction found. If no direction was found, then it will be zero
            return result;
        }
        public void CalculateEnergy()
        {
            this.Energy = 1f - 0.001f * this.Depth - Mathf.Exp(-this.raycast(this.Position, Vector3.up, this.Tree.parameters.LeafColliderSize * 1.1f));
        }

        /**
        Cast a ray from a given position in a given direction

        Returns the distance to the first object hit by the ray
        **/
        private float raycast(Vector3 position, Vector3 direction, float skip = 0f)
        {
            this.Tree.RayCastCount++; // Increment the raycast count done by this branch's tree

            // Create a ray from the calculated position(tree root position + the given position + the normalized given direction * the skip distance) with the given direction
            // Skip is used to offset the origin a tiny bit, so that it doesn't hit this branch
            Ray ray = new Ray(this.Tree.transform.position + position + direction.normalized * skip, direction);
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 1f);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) // If we hit something, then return the distance to that object + the skip distance
                return hit.distance + skip;
            else // If we didn't hit anything, then return an infinite distance
                return System.Single.PositiveInfinity;
        }

        public void CalculateSubtreeSize()
        {
            if (this.Children.Length == 0)
            {
                this.SubtreeSize = 1;
            }
            else
            {
                foreach (var child in this.Children)
                {
                    child.CalculateSubtreeSize();
                }

                this.SubtreeSize = this.Children.Sum(child => child.SubtreeSize) + 1;
            }
        }
    }
}