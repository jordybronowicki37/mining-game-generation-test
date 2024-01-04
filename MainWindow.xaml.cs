using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MiningGameMapGenerationTest.MapGeneration;
using MiningGameMapGenerationTest.MapGeneration.Tiles;

namespace MiningGameMapGenerationTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GenerateGrid();
        }
        
        private void GenerateGrid()
        {
            var mapGenerator = new MapGenerator
            {
                Width = 50,
                Height = 50,
                MainRoom = new Position(24, 24),
                RedBase = new Position(1, 1),
                YellowBase = new Position(46, 1),
                GreenBase = new Position(1, 46),
                BlueBase = new Position(46, 46),
            };
            mapGenerator.GenerateAll();

            // Create a new Grid
            var dynamicGrid = new Grid();

            // Generate rows
            for (var i = 0; i < mapGenerator.Height; i++)
            {
                dynamicGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Generate columns
            for (var j = 0; j < mapGenerator.Width; j++)
            {
                dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Add some controls or content to the grid cells (optional)
            for (var row = 0; row < dynamicGrid.RowDefinitions.Count; row++)
            {
                for (var col = 0; col < dynamicGrid.ColumnDefinitions.Count; col++)
                {
                    var cell = new Rectangle
                    {
                        Fill = new SolidColorBrush(Colors.Black),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    var tile = mapGenerator.Grid[row][col];
                    
                    switch (tile)
                    {
                        case PathTile pathTile:
                            cell.Fill = new SolidColorBrush(pathTile.TrackType == null ? Colors.LightSalmon: Colors.Brown);
                            break;
                        case BaseTile baseTile:
                            cell.Fill = baseTile.BaseType switch
                            {
                                BaseType.General => new SolidColorBrush(Colors.Gray),
                                BaseType.Red => new SolidColorBrush(Colors.Red),
                                BaseType.Yellow => new SolidColorBrush(Colors.Yellow),
                                BaseType.Green => new SolidColorBrush(Colors.Green),
                                BaseType.Blue => new SolidColorBrush(Colors.Blue),
                                _ => cell.Fill
                            };
                            break;
                    } 

                    // Add the Rectangle to the grid cell
                    dynamicGrid.Children.Add(cell);
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                }
            }

            // Add the dynamically generated grid to the mainGrid in XAML
            mainGrid.Children.Add(dynamicGrid);
        }
    }
}