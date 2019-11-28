using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeDApp2
{
    public class Algorithm
    {
        //PathSNode Root = new PathSNode();
        //bool GotPathTree = false;
        private Dictionary<int, PathSNode> Nodes = null;
        private Dictionary<string, PathSLink> Links = null;
        private PathSNode Root = null;
        private bool GotPathTree = false;
        private PathSNode Destination = null;


        // Find a shortest path tree rooted at this node
        // using a label setting algorithm.
        private void FindPathTree(PathSNode root)
        {
            if (root == null) return;
            Root = root;

            List<PathSNode> candidates = new List<PathSNode>();

            // Reset all nodes' Marked and NodeStatus values,
            // and all links' Used and LinkUsage flags.
            ResetPathTree();

            // Start with the root in the shortest path tree.
            root.Dist = 0;
            root.InLink = null;
            root.NodeStatus = PathSNode.NodeStatusType.NowInList;
            candidates.Add(root);

            // Process the candidates.
            while (candidates.Count > 0)
            {
                // Find the candidate closest to the root.
                int best_dist = int.MaxValue;
                int best_i = -1;
                for (int i = 0; i < candidates.Count; i++)
                {
                    PathSNode candidate_node = candidates[i];
                    int new_dist = candidate_node.Dist;
                    if (new_dist < best_dist)
                    {
                        best_i = i;
                        best_dist = new_dist;
                    }
                }

                // Add this node to the shortest path tree.
                PathSNode node = candidates[best_i];
                candidates.RemoveAt(best_i);
                node.NodeStatus = PathSNode.NodeStatusType.WasInList;

                // Examine the node's neighbors.
                foreach (PathSLink link in node.Links.Values)
                {
                    PathSNode to_node;
                    if (node == link.Node1)
                    {
                        to_node = link.Node2;
                    }
                    else
                    {
                        to_node = link.Node1;
                    }
                    if (to_node.NodeStatus ==
                        PathSNode.NodeStatusType.NotInList)
                    {
                        // The node has not been in the candidate list.
                        // Add it.
                        candidates.Add(to_node);
                        to_node.NodeStatus =
                            PathSNode.NodeStatusType.NowInList;
                        to_node.Dist = best_dist + link.Cost;
                        to_node.InLink = link;
                    }
                    else if (to_node.NodeStatus ==
                        PathSNode.NodeStatusType.NowInList)
                    {
                        // The node is in the candidate list.
                        // Update its Dist and inlink values if necessary.
                        int new_dist = best_dist + link.Cost;
                        if (new_dist < to_node.Dist)
                        {
                            to_node.Dist = new_dist;
                            to_node.InLink = link;
                        }
                    }
                } // foreach (PathSLink link in node.Links)
            } // while (candidates.Count > 0)

            GotPathTree = true;

            // Mark the inlinks so they are easy to draw.
            foreach (PathSNode node in Nodes.Values)
            {
                if (node.InLink != null)
                {
                    node.InLink.LinkUsage =
                        PathSLink.LinkUseageType.InTree;
                }
            }

            // Start with no destination.
            Destination = null;

            // Redraw the network.
          ///  this.Refresh();
        }


        // Find the shortest path from the destination to the root.
        private void FindShortestPath()
        {
            // Reset any links that were in the previous shortest path.
            foreach (PathSLink link in Links.Values)
            {
                if (link.LinkUsage == PathSLink.LinkUseageType.InPath)
                {
                    link.LinkUsage = PathSLink.LinkUseageType.InTree;
                }
            }

            // Trace the path from the destination to the root.
            PathSNode node = Destination;
            while (node != Root)
            {
                node.InLink.LinkUsage = PathSLink.LinkUseageType.InPath;
                if (node.InLink.Node1 == node)
                {
                    node = node.InLink.Node2;
                }
                else
                {
                    node = node.InLink.Node1;
                }
            }

            // Redraw the network.
          //  this.Refresh();
        }

        // Remove all links from the shortest path tree.
        private void ResetPathTree()
        {
            // Don't bother if there's no shortest path tree.
            if (!GotPathTree) return;

            foreach (PathSNode node in Nodes.Values)
            {
                node.NodeStatus = PathSNode.NodeStatusType.NotInList;
            }

            foreach (PathSLink link in Links.Values)
            {
                link.LinkUsage = PathSLink.LinkUseageType.Unused;
            }

            GotPathTree = false;
        }

    }
}
