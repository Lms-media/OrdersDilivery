namespace DeliveryBranchAndBound {
    public class Node {
        public List<int> fixedPart { get; set; }
        public List<int> variablePart { get; set; }
        public int upperValue { get; set; }
        public int lowerValue { get; set; }

        public Node(List<int> fixedPart, List<int> variablePart, Func<Node, int> upperBoundFunc, Func<Node, int> lowerBoundFunc) {
            this.fixedPart = [.. fixedPart];
            this.variablePart = [.. variablePart];
            upperValue = upperBoundFunc(this);
            lowerValue = lowerBoundFunc(this);
        }

        public bool isEqual(Node other) {
            return fixedPart.SequenceEqual(other.fixedPart) && upperValue == other.upperValue && lowerValue == other.lowerValue;
        }

        public override string ToString() {
            return $"Node(fixedPart: [{string.Join(", ", fixedPart)}], variablePart: [{string.Join(", ", variablePart)}], upperValue: {upperValue}, lowerValue: {lowerValue})";
        }
    }
}