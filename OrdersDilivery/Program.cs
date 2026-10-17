namespace DeliveryBranchAndBound {
    class Program {
        static void Main(string[] args) {
            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
            string parentDirectory = Directory.GetParent(projectDirectory)!.FullName;
            string taskDirectory = Path.Combine(parentDirectory, "task");
            if (!Directory.Exists(taskDirectory))
                taskDirectory = Path.Combine(parentDirectory, "tasks");

            string outputDirectory = Path.Combine(parentDirectory, "output");
            Directory.CreateDirectory(outputDirectory);

            if (!Directory.Exists(taskDirectory)) {
                Console.Error.WriteLine($"Папка с задачами не найдена: {taskDirectory}");
                return;
            }

            foreach (string filePath in Directory.GetFiles(taskDirectory).OrderBy(path => path, StringComparer.Ordinal)) {
                string outputPath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(filePath)}_output.txt");
                using var writer = new StreamWriter(outputPath, false);
                TextWriter originalOutput = Console.Out;

                try {
                    Console.SetOut(writer);
                    Console.WriteLine("=== Запуск решения задачи о доставке заказов методом ветвей и границ ===\n");

                    TaskSolver problem = TaskReader.ReadFromFile(filePath);
                    var result = problem.Solve();
                    Console.WriteLine($"Visited nodes count: {result.visitedNodes}");
                    Console.WriteLine($"Best node: {result.bestValue}");
                }
                finally {
                    Console.SetOut(originalOutput);
                }
            }
        }
    }
}
