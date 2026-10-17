namespace DeliveryBranchAndBound {    
    public class TaskReader {
        public static TaskSolver ReadFromFile(string filePath) {
            string[] lines = File.ReadAllLines(filePath)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .ToArray();

            int n = int.Parse(lines[0]);
            int[] dueDates = lines[1]
                .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();

            int[,] distances = new int[n + 1, n + 1];
            for (int i = 0; i < n + 1; i++) {
                int[] rowValues = lines[i + 2]
                    .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToArray();

                for (int j = 0; j < n + 1; j++) {
                    distances[i, j] = rowValues[j];
                }
            }

            return new TaskSolver(n, distances, dueDates);
        }
    }
}