using ScottPlot;

namespace DeliveryBranchAndBound {
    class Program {
        public static Node minUpperValue(List<Node> V) => V.OrderBy(n => n.upperValue).First();
        public static Node minLowerValue(List<Node> V) => V.OrderBy(n => n.lowerValue).First();
        public static Node minDistance(List<Node> V) => V.OrderBy(n => n.upperValue - n.lowerValue).First();
        public static Node BFS(List<Node> V) => V.First();
        public static Node DFS(List<Node> V) => V.Last();
        static void Main(string[] args) {
            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
            string parentDirectory = Directory.GetParent(projectDirectory)!.FullName;
            string taskDirectory = Path.Combine(parentDirectory, "task");
            if (!Directory.Exists(taskDirectory))
                taskDirectory = Path.Combine(parentDirectory, "tasks");

            string outputDirectory = Path.Combine(parentDirectory, "output");
            Directory.CreateDirectory(outputDirectory);

            if (!Directory.Exists(taskDirectory)) {
                Console.Error.WriteLine($"not found: {taskDirectory}");
                return;
            }

            double totalMeasure = 0;
            int taskCount = 0;
            foreach (string filePath in Directory.GetFiles(taskDirectory).OrderBy(path => path, StringComparer.Ordinal)) {
                string outputPath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(filePath)}_output.txt");
                string graphPath = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(filePath)}_graph.png");
                using var writer = new StreamWriter(outputPath, false);
                TextWriter originalOutput = Console.Out;

                try {
                    Console.SetOut(writer);
                    Console.WriteLine($"=== Task {Path.GetFileName(filePath)} ===\n");

                    
                    var result = TaskReader.ReadFromFile(filePath).Solve(minUpperValue);
                    var resultI = TaskReader.ReadFromFile(filePath).Solve(minUpperValue, improve: true);
                    Console.WriteLine($"Visited nodes count: {result.visitedNodes} (MinUpperValue)");
                    Console.WriteLine($"Visited nodes count: {resultI.visitedNodes} (MinUpperValue, improve)");
                    Console.WriteLine($"Best node: {result.bestNode} (MinUpperValue)");
                    Console.WriteLine($"Best node: {resultI.bestNode} (MinUpperValue, improve)");
                    double measure = (double)(result.visitedNodes - resultI.visitedNodes) / result.visitedNodes;
                    Console.WriteLine($"Measure: {measure}");

                    Multiplot multiplot = new();
                    multiplot.AddPlots(2);
                    Plot plot1 = multiplot.Subplots.GetPlot(0);
                    Plot plot2 = multiplot.Subplots.GetPlot(1);

                    double[] xsBaseUpper = result.upperValueHistory.Select(p => (double)p.Item1).ToArray();
                    double[] ysBaseUpper = result.upperValueHistory.Select(p => (double)p.Item2).ToArray();
                    double[] xsBaseLower = result.lowerValueHistory.Select(p => (double)p.Item1).ToArray();
                    double[] ysBaseLower = result.lowerValueHistory.Select(p => (double)p.Item2).ToArray();

                    double[] xsImproveUpper = resultI.upperValueHistory.Select(p => (double)p.Item1).ToArray();
                    double[] ysImproveUpper = resultI.upperValueHistory.Select(p => (double)p.Item2).ToArray();
                    double[] xsImproveLower = resultI.lowerValueHistory.Select(p => (double)p.Item1).ToArray();
                    double[] ysImproveLower = resultI.lowerValueHistory.Select(p => (double)p.Item2).ToArray();

                    var baseUpperGraph = plot1.Add.Scatter(xsBaseUpper, ysBaseUpper);
                    baseUpperGraph.LegendText = "Base min upper value";
                    baseUpperGraph.Color = Colors.Blue;
                    baseUpperGraph.MarkerSize = 8;

                    var baseLowerGraph = plot2.Add.Scatter(xsBaseLower, ysBaseLower);
                    baseLowerGraph.LegendText = "Base min lower value";
                    baseLowerGraph.Color = Colors.Green;
                    baseLowerGraph.MarkerSize = 8;

                    var improveUpperGraph = plot1.Add.Scatter(xsImproveUpper, ysImproveUpper);
                    improveUpperGraph.LegendText = "Improve min upper value";
                    improveUpperGraph.Color = Colors.Red;
                    improveUpperGraph.MarkerSize = 8;

                    var improveLowerGraph = plot2.Add.Scatter(xsImproveLower, ysImproveLower);
                    improveLowerGraph.LegendText = "Improve min lower value";
                    improveLowerGraph.Color = Colors.Orange;
                    improveLowerGraph.MarkerSize = 8;

                    plot1.XLabel("Iterated nodes");
                    plot1.YLabel("Min upper value");
                    plot1.Title("Base vs Improve");
                    plot1.ShowLegend(Alignment.UpperRight);

                    plot2.XLabel("Iterated nodes");
                    plot2.YLabel("Max lower value");
                    plot2.Title("Base vs Improve");
                    plot2.ShowLegend(Alignment.UpperRight);
                    multiplot.SavePng(graphPath, 1000, 2000);

                    totalMeasure += measure;
                    taskCount++;
                }
                finally {
                    Console.SetOut(originalOutput);
                }
            }
            if (taskCount > 0) {
                Console.WriteLine($"Average measure: {totalMeasure / taskCount}");
            }
        }
    }
}
