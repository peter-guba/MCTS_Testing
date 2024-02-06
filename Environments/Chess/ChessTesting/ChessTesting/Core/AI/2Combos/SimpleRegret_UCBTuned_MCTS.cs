﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessTesting.Core.AI.TwoCombos
{
    // A combination of SR+CR MCTS and UCB1-Tuned MCTS.
    class SimpleRegret_UCBTuned_MCTS : BasicMCTS
    {
        /// <summary>
        /// Determines whether the variant should use the epsilon-greedy policy or UCBsqrt at the root.
        /// </summary>
        private bool useEpsilonGreedy = false;

        /// <summary>
        /// If the epsilon-greedy policy is used, this is the value of the epsilon parameter.
        /// </summary>
        private float epsilon = 0.5f;

        public SimpleRegret_UCBTuned_MCTS(int maxNumOfPlayouts, int playoutDepthLimit, string name, string bmrkID, bool uEG, float epsilon) : base(maxNumOfPlayouts, playoutDepthLimit, name, bmrkID)
        {
            useEpsilonGreedy = uEG;
            this.epsilon = epsilon;
        }

        protected override MCTSNode SelectAndExpand(MCTSNode node)
        {
            while (true)
            {
                MCTSNode bestChild;

                if (node.parent == null && useEpsilonGreedy)
                {
                    if (node.isTerminal)
                    {
                        return node;
                    }
                    else if (node.Children.Count == 0)
                    {
                        return node.Expand();
                    }
                    else
                    {
                        bestChild = SelectChildWithEpsilonGreedy(node);

                        // If the node is a new one (meaning it was created in the SelectChildWithEpsilonGreedy
                        // function), we should return.
                        if (bestChild.Visits == 0)
                        {
                            return bestChild;
                        }
                    }
                }
                else
                {
                    // If node is not fully expanded - generate new kind node with this new action and return it
                    if (!node.FullyExpanded)
                    {
                        return node.Expand();
                    }
                    else if (node.isTerminal)
                    {
                        return node;
                    }

                    if (node.parent == null)
                    {
                        bestChild = SelectBestChild(node, UCBSqrt);
                    }
                    else
                    {
                        bestChild = SelectBestChild(node, UCBTuned);
                    }
                }

                if (bestChild == null)
                    return null;

                node = bestChild;
            }
        }

        /// <summary>
        /// Implementation of a tree policy that picks the best child with probability
        /// epsilon and a random different child with probability 1 - epsilon.
        /// </summary>
        private MCTSNode SelectChildWithEpsilonGreedy(MCTSNode node)
        {
            // Find the best child.
            double bestScore = 0.0;
            int bestChildIndex = -1;
            MCTSNode bestChild = null;
            for (int i = 0; i < node.Children.Count; ++i)
            {
                MCTSNode child = node.Children[i];
                double score = child.Value / child.Visits;
                if (bestChild == null || score > bestScore)
                {
                    bestScore = score;
                    bestChildIndex = i;
                    bestChild = child;
                }
            }

            // Roll a random number between 0 and 1. If it is smaller than epsilon,
            // return the best child.
            if (rand.NextDouble() < epsilon)
            {
                return bestChild;
            }
            // Otherwise pick a different random child.
            else
            {
                int index = rand.Next(0, node.Children.Count - 1 + node.PossibleActions.Count);

                if (index < bestChildIndex)
                {
                    return node.Children[index];
                }
                else if (index >= bestChildIndex && index < node.Children.Count - 1)
                {
                    return node.Children[index + 1];
                }
                // If the index of the random child is larger than the number of children the node
                // currently has, it must be instantiated first by calling the expand function.
                else if (index >= node.Children.Count - 1 && index < node.PossibleActions.Count + node.Children.Count - 1)
                {
                    return node.Expand(index - node.Children.Count + 1);
                }
                else
                {
                    return bestChild;
                }
            }
        }

        /// <summary>
        /// Similar to UCB, but swaps the logarithm for a square root function.
        /// </summary>
        /// <param name="redundant"> This parameter isn't used in the function, but it
        /// must be present so that it has the same parameters as UCB. </param>
        private float UCBSqrt(MCTSNode n, bool redundant)
        {
            // Scale the exploitation value between 0 and 1.
            float exploitation = (1.0f + n.Value / n.Visits) / 2;
            return exploitation + C * (float)Math.Sqrt(Math.Sqrt(n.parent.Visits) / n.Visits);
        }

        /// <summary>
        /// Like UCB, but with a tighter bound on the uncertainty of observations.
        /// </summary>
        /// <param name="redundant"> This parameter isn't used in the function, but it
        /// must be present so that it has the same parameters as UCB. </param>
        private float UCBTuned(MCTSNode node, bool redundant)
        {
            // Scale the exploitation value to between 0 and 1.
            float exploit = (1.0f + node.Value / node.Visits) / 2;
            double v = node.SquaredValueSum / node.Visits - exploit * exploit + Math.Sqrt(2 * Math.Log(node.parent.Visits) / node.Visits);
            double explore = Math.Sqrt((Math.Log(node.parent.Visits) / node.Visits) * Math.Min(0.25, v));
            return (float)(exploit + explore);
        }

        protected override void Backpropagate(MCTSNode node, float val)
        {
            node.Value += val;
            // Scale the value to between 0 and 1 before squaring it.
            node.SquaredValueSum += (1.0f + val) * (1.0f + val) / 4;

            ++node.Visits;

            if (node.parent != null)
            {
                Backpropagate(node.parent, -val);
            }
        }

        public override string ToString()
        {
            if (useEpsilonGreedy)
            {
                return $"SR+CR + UCB1-Tuned MCTS, metric: EpsilonGreedy, epsilon: {epsilon}, playouts: {maxNumOfPlayouts}";
            }
            else
            {
                return $"SR+CR + UCB1-Tuned MCTS, metric: UCTSqrt, playouts: {maxNumOfPlayouts}";
            }
        }
    }
}