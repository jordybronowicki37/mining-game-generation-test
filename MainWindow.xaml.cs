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
                MainRoom = new Position(25, 25),
                RedBase = new Position(2, 2),
                YellowBase = new Position(47, 2),
                GreenBase = new Position(2, 47),
                BlueBase = new Position(47, 47),
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
                    var cellWrapper = new Grid();
                    var cell = new Rectangle
                    {
                        Fill = new SolidColorBrush(Colors.Black),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    cellWrapper.Children.Add(cell);
                    
                    var tile = mapGenerator.Grid[row][col];
                    
                    switch (tile)
                    {
                        case PathTile pathTile:
                            if (pathTile.TrackType == null)
                            {
                                cell.Fill = new SolidColorBrush(Colors.LightSalmon);
                            }
                            else
                            {
                                cell.Fill = new SolidColorBrush(Colors.Brown);
                                var trackText = new TextBlock
                                {
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Text = pathTile.TrackType switch
                                    {
                                        TrackType.Straight => pathTile.TrackDirection == Direction.North ? "|" : "-",
                                        TrackType.Curved => pathTile.TrackDirection switch
                                        {
                                            Direction.North => "┗",
                                            Direction.East => "┏",
                                            Direction.South => "┓",
                                            Direction.West => "┛",
                                            _ => ""
                                        },
                                        TrackType.Switch => pathTile.TrackDirection switch
                                        {
                                            Direction.North => "T",
                                            Direction.East => "⊣",
                                            Direction.South => "Ʇ",
                                            Direction.West => "⊢",
                                            _ => ""
                                        },
                                        TrackType.Intersection => "+",
                                        TrackType.End => pathTile.TrackDirection switch
                                        {
                                            Direction.North => "v",
                                            Direction.East => "<",
                                            Direction.South => "^",
                                            Direction.West => ">",
                                            _ => ""
                                        },
                                        _ => ""
                                    }
                                };
                                cellWrapper.Children.Add(trackText);
                            }
                            
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
                    dynamicGrid.Children.Add(cellWrapper);
                    Grid.SetRow(cellWrapper, row);
                    Grid.SetColumn(cellWrapper, col);
                }
            }

            // Add the dynamically generated grid to the mainGrid in XAML
            mainGrid.Children.Add(dynamicGrid);
        }
    }
}