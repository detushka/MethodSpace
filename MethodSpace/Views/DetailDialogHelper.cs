using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MethodSpace.Views
{
    internal static class DetailDialogHelper
    {
        public static void Show(Window owner, string title, string subtitle, string body)
        {
            var window = new Window
            {
                Title = title,
                Width = 620,
                Height = 460,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = owner,
                ResizeMode = ResizeMode.CanResize,
                Background = owner != null
                    ? owner.TryFindResource("AppBackgroundBrush") as Brush ?? Brushes.WhiteSmoke
                    : Brushes.WhiteSmoke
            };

            Brush cardBrush = owner != null
                ? owner.TryFindResource("CardBrush") as Brush ?? Brushes.White
                : Brushes.White;
            Brush cardBorderBrush = owner != null
                ? owner.TryFindResource("CardBorderBrush") as Brush ?? Brushes.LightGray
                : Brushes.LightGray;
            Brush primaryTextBrush = owner != null
                ? owner.TryFindResource("PrimaryTextBrush") as Brush ?? Brushes.Black
                : Brushes.Black;
            Brush secondaryTextBrush = owner != null
                ? owner.TryFindResource("SecondaryTextBrush") as Brush ?? Brushes.DimGray
                : Brushes.DimGray;

            var root = new Grid { Margin = new Thickness(18) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new Border
            {
                Background = cardBrush,
                BorderBrush = cardBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 14)
            };

            var headerPanel = new StackPanel();
            headerPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 22,
                FontWeight = FontWeights.SemiBold,
                Foreground = primaryTextBrush,
                TextWrapping = TextWrapping.Wrap
            });

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                headerPanel.Children.Add(new TextBlock
                {
                    Text = subtitle,
                    Margin = new Thickness(0, 8, 0, 0),
                    Foreground = secondaryTextBrush,
                    TextWrapping = TextWrapping.Wrap
                });
            }

            header.Child = headerPanel;
            Grid.SetRow(header, 0);
            root.Children.Add(header);

            var bodyBorder = new Border
            {
                Background = cardBrush,
                BorderBrush = cardBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(18)
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(body) ? "Подробная информация отсутствует." : body,
                    Foreground = primaryTextBrush,
                    FontSize = 14,
                    LineHeight = 22,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            bodyBorder.Child = scrollViewer;
            Grid.SetRow(bodyBorder, 1);
            root.Children.Add(bodyBorder);

            var closeButton = new Button
            {
                Content = "Закрыть",
                Width = 120,
                Height = 38,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 14, 0, 0),
                Style = owner != null ? owner.TryFindResource("ActionButtonStyle") as Style : null
            };
            closeButton.Click += (sender, args) => window.Close();

            Grid.SetRow(closeButton, 2);
            root.Children.Add(closeButton);

            window.Content = root;
            window.ShowDialog();
        }
    }
}
