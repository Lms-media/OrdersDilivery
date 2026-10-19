namespace DeliveryBranchAndBound {
    public class TaskSolver {
        private int n;
        private int[] t_d;
        private int[,] t;

        public TaskSolver(int n, int[,] distances, int[] dueDates) {
            this.n = n;
            t = distances;
            t_d = dueDates;
        }

        public int GetDeliveryTime(List<int> permutation, int indexInPermutation) {
            int time = t[0, permutation[0]];
            for (int i = 0; i < indexInPermutation; i++)
                time += t[permutation[i], permutation[i + 1]];
            return time;
        }

        public int CalculateTotalPenalty(List<int> permutation) {
            int totalPenalty = 0;
            for (int i = 0; i < permutation.Count; i++)
                totalPenalty += GetDeliveryTime(permutation, i) <= t_d[permutation[i] - 1] ? 0 : 1;
            return totalPenalty;
        }

        public List<int> GetGreedyPath(Node node) {
            List<int> currentPermutation = [.. node.fixedPart];
            List<int> available = [.. node.variablePart];

            while (available.Count > 0) {
                int bestNextItem = -1;
                int minTimeUntilFine = int.MaxValue;

                foreach (var item in available) {
                    List<int> tempPerm = new List<int>(currentPermutation) { item };
                    int deliveryTime = GetDeliveryTime(tempPerm, tempPerm.Count - 1);
                    int timeUntilFine = t_d[item - 1] - deliveryTime;

                    if (timeUntilFine >= 0 && timeUntilFine < minTimeUntilFine) {
                        minTimeUntilFine = timeUntilFine;
                        bestNextItem = item;
                    }
                }

                int nextItem = bestNextItem != -1 ? bestNextItem : available[0];

                currentPermutation.Add(nextItem);
                available.Remove(nextItem);
            }
            return currentPermutation;
        }

        public (List<int>, int) CalculateUpperValue(Node node) {
            var greedyPath = GetGreedyPath(node);
            return (greedyPath, CalculateTotalPenalty(greedyPath));
        }

        public int CalculateLowerValue(Node node) {
            int k = node.fixedPart.Count;
            int currTime = k != 0 ? t[0, node.fixedPart[0]] : 0;
            int lowerBound = k != 0 && currTime > t_d[node.fixedPart[0] - 1] ? 1 : 0;

            for (int i = 0; i < k - 1; i++) {
                currTime += t[node.fixedPart[i], node.fixedPart[i + 1]];
                if (currTime > t_d[node.fixedPart[i + 1] - 1]) lowerBound++;
            }

            int fixItem = k != 0 ? node.fixedPart[k - 1] : 0;
            foreach (var variableItem in node.variablePart)
                lowerBound += currTime + t[fixItem, variableItem] > t_d[variableItem - 1] ? 1 : 0;

            return lowerBound;
        }

        public int CalculateLowerValueImprove(Node node) {
            int k = node.fixedPart.Count;
            int currTime = k != 0 ? t[0, node.fixedPart[0]] : 0;
            int lowerBound = k != 0 && currTime > t_d[node.fixedPart[0] - 1] ? 1 : 0;

            for (int i = 0; i < k - 1; i++) {
                currTime += t[node.fixedPart[i], node.fixedPart[i + 1]];
                if (currTime > t_d[node.fixedPart[i + 1] - 1]) lowerBound++;
            }

            int fixItem = k != 0 ? node.fixedPart[k - 1] : 0;
            foreach (var variableItem in node.variablePart)
                lowerBound += currTime + t[fixItem, variableItem] > t_d[variableItem - 1] ? 1 : 0;

            var variable = node.variablePart;
            int m = variable.Count;
            var edge = new bool[m, m];

            for (int i = 0; i < m; i++) {
                for (int j = i + 1; j < m; j++) {
                    int a = variable[i];
                    int b = variable[j];

                    int timeA = currTime + t[fixItem, a];
                    int timeB = currTime + t[fixItem, b];

                    bool aOnTime = timeA <= t_d[a - 1];
                    bool bOnTime = timeB <= t_d[b - 1];
                    if (!aOnTime || !bOnTime) continue;

                    int timeAB = timeA + t[a, b];
                    int timeBA = timeB + t[b, a];

                    bool conflict = (timeAB > t_d[b - 1]) && (timeBA > t_d[a - 1]);
                    if (conflict) edge[i, j] = edge[j, i] = true;
                }
            }

            var used = new bool[m];
            for (int i = 0; i < m; i++) {
                if (used[i]) continue;
                for (int j = i + 1; j < m; j++) {
                    if (!used[j] && edge[i, j]) {
                        lowerBound++;
                        used[i] = used[j] = true;
                        break;
                    }
                }
            }

            return lowerBound;
        }
        public (Node bestNode, int visitedNodes, List<(int, int)> upperValueHistory, List<(int, int)> lowerValueHistory) Solve(Func<List<Node>, Node> selectNode, bool improve = false) {
            List<Node> V = new List<Node> {
                new Node(
                    new List<int>(),
                    Enumerable.Range(1, n).ToList(),
                    CalculateUpperValue,
                    improve ? CalculateLowerValueImprove : CalculateLowerValue
                )
            };

            int visitedNodes = 1;
            List<(int, int)> minUpperValueHistory = new List<(int, int)>();
            List<(int, int)> maxLowerValueHistory = new List<(int, int)>();
            minUpperValueHistory.Add((visitedNodes, V[0].upperValue));
            maxLowerValueHistory.Add((visitedNodes, V[0].lowerValue));

            Node bestNode = V[0];
            if (V[0].upperValue == V[0].lowerValue)
                return (new Node(GetGreedyPath(V[0]), new List<int>(), CalculateUpperValue, improve ? CalculateLowerValueImprove : CalculateLowerValue), visitedNodes, minUpperValueHistory, maxLowerValueHistory);

            while (V.Count > 0) {
                Node v = selectNode(V);
                V.Remove(v);

                IEnumerable<int> ordersToBranch = v.variablePart;
                if (improve) {
                    int currentLast = v.fixedPart.Count > 0 ? v.fixedPart.Last() : 0;
                    List<int> freeOrders = v.variablePart.Where(item => t[currentLast, item] == 0).ToList();

                    ordersToBranch = freeOrders.Count > 0 
                        ? freeOrders 
                        : v.variablePart;
                }

                foreach (var item in ordersToBranch) {
                    List<int> newFixedPart = new List<int>(v.fixedPart) { item };
                    List<int> newVariablePart = [.. v.variablePart];
                    newVariablePart.Remove(item);

                    Node child = new Node(newFixedPart, newVariablePart, CalculateUpperValue, improve ? CalculateLowerValueImprove : CalculateLowerValue);
                    V.Add(child);
                    visitedNodes++;

                    if (child.upperValue < bestNode.upperValue) bestNode = child;
                }

                V.RemoveAll(node => !node.isEqual(bestNode) && node.lowerValue >= bestNode.upperValue);

                minUpperValueHistory.Add((visitedNodes, bestNode.upperValue));
                maxLowerValueHistory.Add((
                    visitedNodes,
                    V.Select(node => node.lowerValue)
                    .Append(bestNode.lowerValue)
                    .Max()
                ));

                if (V.Count == 1 && V[0].upperValue == V[0].lowerValue)
                    return (V[0], visitedNodes, minUpperValueHistory, maxLowerValueHistory);
            }

            Node resultNode = new Node(GetGreedyPath(bestNode), new List<int>(), CalculateUpperValue, improve ? CalculateLowerValueImprove : CalculateLowerValue);
            minUpperValueHistory.Add((visitedNodes, resultNode.upperValue));
            maxLowerValueHistory.Add((visitedNodes, resultNode.lowerValue));
            return (resultNode, visitedNodes, minUpperValueHistory, maxLowerValueHistory);
        }
    }
}