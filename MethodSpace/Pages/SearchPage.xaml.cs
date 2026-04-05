using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class SearchPage : Page
    {
        private readonly SQL _context;
        private readonly int _userId;
        private readonly string _userRole;

        public SearchPage(int userId, string userRole)
        {
            _context = new SQL();
            InitializeComponent();
            _userId = userId;
            _userRole = userRole;
        }

        private void SearchType_Changed(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            try
            {
                if (_context == null || SearchBox == null || ResultsList == null)
                {
                    return;
                }

                string query = SearchBox.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(query))
                {
                    ResultsList.ItemsSource = null;
                    return;
                }

                var results = new List<SearchResult>();

                if ((SearchAll != null && SearchAll.IsChecked == true) || (SearchDocuments != null && SearchDocuments.IsChecked == true))
                {
                    var documents = _context.Documents
                        .Where(d => d.Title.ToLower().Contains(query) ||
                                   (d.Description != null && d.Description.ToLower().Contains(query)))
                        .ToList();

                    foreach (var doc in documents)
                    {
                        results.Add(new SearchResult
                        {
                            Title = doc.Title,
                            Description = doc.Description ?? "Документ",
                            Type = "Документ"
                        });
                    }
                }

                if ((SearchAll != null && SearchAll.IsChecked == true) || (SearchCourses != null && SearchCourses.IsChecked == true))
                {
                    var courses = _context.Courses
                        .Where(c => c.CourseName.ToLower().Contains(query))
                        .ToList();

                    foreach (var course in courses)
                    {
                        string description = "Курс: " + course.StartDate.ToString("dd.MM.yyyy") + " - " +
                                             course.EndDate.ToString("dd.MM.yyyy") + ", место: " + course.Location;

                        results.Add(new SearchResult
                        {
                            Title = course.CourseName,
                            Description = description,
                            Type = "Курс"
                        });
                    }
                }

                if ((SearchAll != null && SearchAll.IsChecked == true) || (SearchNews != null && SearchNews.IsChecked == true))
                {
                    var news = _context.News
                        .Where(n => n.Title.ToLower().Contains(query) ||
                                   n.Content.ToLower().Contains(query))
                        .ToList();

                    foreach (var newsItem in news)
                    {
                        string description = newsItem.Content;
                        if (description.Length > 100)
                        {
                            description = description.Substring(0, 100) + "...";
                        }

                        results.Add(new SearchResult
                        {
                            Title = newsItem.Title,
                            Description = description,
                            Type = "Новость"
                        });
                    }
                }

                if ((SearchAll != null && SearchAll.IsChecked == true) || (SearchTips != null && SearchTips.IsChecked == true))
                {
                    var tips = _context.TeacherTips
                        .Where(t => t.Title.ToLower().Contains(query) ||
                                   t.Content.ToLower().Contains(query))
                        .ToList();

                    foreach (var tip in tips)
                    {
                        string description = tip.Content;
                        if (description.Length > 100)
                        {
                            description = description.Substring(0, 100) + "...";
                        }

                        results.Add(new SearchResult
                        {
                            Title = tip.Title,
                            Description = description,
                            Type = "Совет"
                        });
                    }
                }

                ResultsList.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class SearchResult
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Type { get; set; }
        }
    }
}
