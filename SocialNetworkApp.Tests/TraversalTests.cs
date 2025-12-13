using System.Collections.Generic;
using SocialNetworkApp.Models;
using SocialNetworkApp.Algorithms;
using Xunit;

namespace SocialNetworkApp.Tests
{
    public class TraversalTests
    {
        private Graph CreateSampleGraph()
        {
            var g = new Graph();
            g.AddNode(new Node(1, "A", 0.5, 1));
            g.AddNode(new Node(2, "B", 0.2, 0));
            g.AddNode(new Node(3, "C", 0.1, 0));
            g.AddNode(new Node(4, "D", 0.4, 2));

            g.AddEdge(new Edge(1,2,0));
            g.AddEdge(new Edge(2,3,0));
            g.AddEdge(new Edge(2,4,0));
            return g;
        }

        [Fact]
        public void BFSTraversal_StartExists_ReturnsNodes()
        {
            var g = CreateSampleGraph();
            var order = TraversalAlgorithms.BFS(g, 1);
            Assert.Contains(1, order);
            Assert.Contains(2, order);
            Assert.Contains(3, order);
            Assert.Contains(4, order);
        }

        [Fact]
        public void BFSTraversal_StartMissing_ReturnsEmpty()
        {
            var g = CreateSampleGraph();
            var order = TraversalAlgorithms.BFS(g, 99);
            Assert.Empty(order);
        }

        [Fact]
        public void DFSTraversal_StartExists_ReturnsNodes()
        {
            var g = CreateSampleGraph();
            var order = TraversalAlgorithms.DFS(g, 1);
            Assert.Contains(1, order);
            Assert.Contains(2, order);
            Assert.Contains(3, order);
            Assert.Contains(4, order);
        }
    }
}
