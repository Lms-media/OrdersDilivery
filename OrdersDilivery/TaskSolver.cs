namespace DeliveryBranchAndBound {
    public class TaskSolver {
        private int n;
        private int[] t_d;
        private int[,] t;

        public TaskSolver(int n, int[,] distances, int[] dueDates) {
            this.n = n;
            t = distances;
            t_d = dueDates;
            Console.WriteLine($"Task with {n} orders");
            for (int i = 0; i < n; i++) Console.WriteLine($"Order {i + 1}: due date: {t_d[i]}");
            for (int i = 0; i < n + 1; i++) {
                for (int j = 0; j < n + 1; j++) Console.Write($"{t[i, j],-2} ");
                Console.WriteLine();
            }
        }

        public int GetDeliveryTime(List<int> permutation, int indexInPermutation) {
            int time = t[0, permutation[0]];
            for (int i = 0; i < indexInPermutation; i++) {
                if (t[permutation[i], permutation[i + 1]] == 0) return int.MaxValue;
                time += t[permutation[i], permutation[i + 1]];
            }
            return time;
        }

        public int CalculateTotalPenalty(List<int> permutation) {
            int totalPenalty = 0;
            for (int i = 0; i < permutation.Count; i++) {
                int deliveryTime = GetDeliveryTime(permutation, i);
                if (deliveryTime == int.MaxValue) return int.MaxValue;

                totalPenalty += deliveryTime <= t_d[permutation[i] - 1] ? 0 : 1;
            }
            return totalPenalty;
        }

        public List<int> GetGreedyPath(Node node) {
            List<int> currentPermutation = [.. node.fixedPart];
            List<int> available = [.. node.variablePart];

            while (available.Count > 0) {
                List<int> A = new List<int>();
                foreach (var item in available) {
                    List<int> tempPerm = new List<int>(currentPermutation) { item };
                    int currentDeliveryTime = GetDeliveryTime(tempPerm, tempPerm.Count - 1);
                    if (currentDeliveryTime < t_d[item - 1]) A.Add(item);
                }

                int nextItem;
                if (A.Count > 0) nextItem = A.OrderBy(i => t_d[i - 1]).First();
                else nextItem = available.Any(item => t[currentPermutation.Last(), item] > 0) ? available.First(item => t[currentPermutation.Last(), item] > 0) : available[0];

                currentPermutation.Add(nextItem);
                available.Remove(nextItem);
            }
            return currentPermutation;
        }

        public int CalculateUpperValue(Node node) {
            return CalculateTotalPenalty(GetGreedyPath(node));
        }

        public int CalculateLowerValue(Node node) {
            int k = node.fixedPart.Count;
            int currTime = k != 0 ? t[0, node.fixedPart[0]] : 0;
            int lowerBound = k != 0 && currTime > t_d[node.fixedPart[0] - 1] ? 1 : 0;

            for (int i = 0; i < k - 1; i++) {
                if (t[node.fixedPart[i], node.fixedPart[i + 1]] == 0) return int.MaxValue;

                currTime += t[node.fixedPart[i], node.fixedPart[i + 1]];
                if (currTime > t_d[node.fixedPart[i + 1] - 1]) lowerBound++;
            }

            if (k != 0)
                foreach (var variable in node.variablePart)
                    if (t[node.fixedPart[k - 1], variable] != 0)
                        lowerBound += currTime + t[node.fixedPart[k - 1], variable] > t_d[variable - 1] ? 1 : 0;

            return lowerBound;
        }

        public (Node bestValue, int visitedNodes, List<int> bHistory, List<int> maxHHistory) Solve() {
            List<Node> V = new List<Node>{
                new Node(
                    new List<int>(),
                    Enumerable.Range(1, n).ToList(),
                    CalculateUpperValue,
                    CalculateLowerValue
                )
            };

            int visitedNodes = 1;
            List<int> bHistory = new List<int>();
            List<int> maxHHistory = new List<int>();

            Node bestNode = V[0];

            while (V.Count > 0) {
                Node v = V.OrderBy(node => node.upperValue).First();
                V.Remove(v);

                foreach (var item in v.variablePart.ToList()) {
                    List<int> newFixedPart = new List<int>(v.fixedPart) { item };
                    List<int> newVariablePart = [.. v.variablePart];
                    newVariablePart.Remove(item);

                    Node child = new Node(newFixedPart, newVariablePart, CalculateUpperValue, CalculateLowerValue);
                    if (child.lowerValue != int.MaxValue && child.upperValue != int.MaxValue) {
                         V.Add(child);
                         visitedNodes++;
                    }
                }

                if (V.Count != 0)
                    bestNode = V
                        .Where(node => !node.Equals(bestNode))
                        .OrderBy(node => node.upperValue)
                        .First();

                V.RemoveAll(node => node.lowerValue > bestNode.upperValue);

                bHistory.Add(bestNode.upperValue);
                maxHHistory.Add(V.Count > 0 ? V.Max(node => node.lowerValue) : bestNode.upperValue);

                if (V.Count == 1) {
                    Node optimalNode = new Node(GetGreedyPath(V[0]), new List<int>(), CalculateUpperValue, CalculateLowerValue);
                    return (optimalNode, visitedNodes, bHistory, maxHHistory);
                }
                if (V.Any(node => node.upperValue == node.lowerValue)) {
                    Node optimalNode = new Node(GetGreedyPath(V.First(node => node.upperValue == node.lowerValue)), new List<int>(), CalculateUpperValue, CalculateLowerValue);
                    return (optimalNode, visitedNodes, bHistory, maxHHistory);
                }
            }

            return (bestNode, visitedNodes, bHistory, maxHHistory);
        }
    }
}