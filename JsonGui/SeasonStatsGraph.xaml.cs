using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JsonGui
{
    /// <summary>
    /// Interaction logic for SeasonStatsGraph.xaml
    /// </summary>
    public partial class SeasonStatsGraph : Window
    {
        // List to store the team season stats data
        private List<JsonCore.Models.GameStats> teamSeasonStats;

        public SeasonStatsGraph(List<JsonCore.Models.GameStats> s)
        {
            InitializeComponent();
            teamSeasonStats = s ?? new List<JsonCore.Models.GameStats>(); // Store the passed team season stats data
            if (teamSeasonStats.Count > 0)
                RenderPlot();
        }

        private void RenderPlot()
        {
            // Plot game scores across the season with date labels on the X axis.
            // Prepare Y values (game scores) and X indices + labels (dates formatted short)
            var y = teamSeasonStats.Select(g => (double)g.score).ToArray();
            var x = Enumerable.Range(0, y.Length).Select(i => (double)i).ToArray();
            var labels = teamSeasonStats.Select(g =>
            {
                if (DateTime.TryParse(g.gameDate, out var dt))
                    return dt.ToString("MM-dd");
                return g.gameDate ?? "";
            }).ToArray();

            wpfPlot.Plot.Clear();
            var scatterPlot = wpfPlot.Plot.Add.ScatterLine(x, y, ScottPlot.Color.FromHex("#800020"));
            wpfPlot.Plot.Title("Season Scores");
            wpfPlot.Plot.XLabel("Game Date");
            wpfPlot.Plot.YLabel("Score");

            // Format markers to be larger and more visible
            scatterPlot.MarkerStyle.Shape = ScottPlot.MarkerShape.FilledDiamond;
            scatterPlot.MarkerStyle.Size = 15; // Make them a bit larger
            scatterPlot.MarkerStyle.FillColor = ScottPlot.Colors.Blue;
            scatterPlot.MarkerStyle.OutlineColor = ScottPlot.Colors.Black;
            scatterPlot.MarkerStyle.OutlineWidth = 2;

            // Set custom X ticks using the date labels
            ScottPlot.Tick[] customTicks = labels.Select((label, index) => new ScottPlot.Tick(index, label)).ToArray();
            wpfPlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(customTicks);
            wpfPlot.Refresh();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Close the graph window when the close button is clicked 
        }
    }
}
