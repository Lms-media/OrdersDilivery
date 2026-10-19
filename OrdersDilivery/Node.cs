namespace DeliveryBranchAndBound {
    public class Node {
        public List<int> fixedPart { get; set; }
        public List<int> variablePart { get; set; }
        public List<int> upperValuePart { get; set; }
        public int upperValue { get; set; }
        public int lowerValue { get; set; }

        public Node(List<int> fixedPart, List<int> variablePart, Func<Node, (List<int>, int)> upperBoundFunc, Func<Node, int> lowerBoundFunc) {
            this.fixedPart = [.. fixedPart];
            this.variablePart = [.. variablePart];
            upperValuePart = upperBoundFunc(this).Item1;
            upperValue = upperBoundFunc(this).Item2;
            lowerValue = lowerBoundFunc(this);
        }

        public bool isEqual(Node other) {
            return fixedPart.SequenceEqual(other.fixedPart) && upperValue == other.upperValue && lowerValue == other.lowerValue;
        }

        public override string ToString() {
            return $"Node(Path: [{string.Join(", ", upperValuePart)}], upperValue: {upperValue}, lowerValue: {lowerValue})";
        }
    }
}