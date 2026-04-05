using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;
using Microsoft.Win32;

namespace MethodSpace.Pages
{
    public partial class DocumentsPage : Page
    {
        private readonly SQL _context;

        public DocumentsPage(int userId, string userRole)
        {
            _context = new SQL();
            InitializeComponent();

            Loaded += (sender, args) => LoadDocuments();
        }

        private void LoadDocuments()
        {
            try
            {
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки документов: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка фильтрации: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_context == null)
            {
                return;
            }

            var query = _context.Documents.AsQueryable();

            if (WorkingPrograms != null && WorkingPrograms.IsChecked == true)
            {
                query = query.Where(item => item.DocumentType == "working_program");
            }
            else if (MethodicalRecs != null && MethodicalRecs.IsChecked == true)
            {
                query = query.Where(item => item.DocumentType == "methodical_recommendation");
            }
            else if (Regulations != null && Regulations.IsChecked == true)
            {
                query = query.Where(item => item.DocumentType == "regulation");
            }
            else if (Instructions != null && Instructions.IsChecked == true)
            {
                query = query.Where(item => item.DocumentType == "instruction");
            }

            var documents = query
                .OrderByDescending(item => item.UploadDate)
                .ToList()
                .Select(item => new
                {
                    item.DocumentID,
                    item.Title,
                    DocumentType = GetDocumentTypeDisplay(item.DocumentType),
                    item.DisciplineName,
                    item.UploadDate,
                    item.DownloadsCount
                })
                .ToList();

            if (DocumentsList != null)
            {
                DocumentsList.ItemsSource = documents;
            }

            if (StatusText != null)
            {
                StatusText.Text = "Найдено документов: " + documents.Count;
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button == null || button.Tag == null)
                {
                    return;
                }

                Document document = _context.Documents.Find((int)button.Tag);
                if (document == null)
                {
                    return;
                }

                if (!File.Exists(document.FilePath))
                {
                    MessageBox.Show("Исходный файл документа не найден.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string sourceFileName = Path.GetFileName(document.FilePath);
                string sourceExtension = Path.GetExtension(document.FilePath);

                var dialog = new SaveFileDialog
                {
                    Title = "Сохранить документ",
                    FileName = sourceFileName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    OverwritePrompt = true,
                    AddExtension = true,
                    DefaultExt = string.IsNullOrWhiteSpace(sourceExtension) ? ".txt" : sourceExtension,
                    Filter = BuildFileFilter(sourceExtension)
                };

                bool? dialogResult = dialog.ShowDialog(Window.GetWindow(this));
                if (dialogResult != true || string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    if (StatusText != null)
                    {
                        StatusText.Text = "Скачивание отменено.";
                    }

                    return;
                }

                File.Copy(document.FilePath, dialog.FileName, true);

                document.DownloadsCount = (document.DownloadsCount ?? 0) + 1;
                _context.SaveChanges();

                if (StatusText != null)
                {
                    StatusText.Text = "Документ сохранен: " + dialog.FileName;
                }

                Process.Start(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при скачивании: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string BuildFileFilter(string extension)
        {
            string normalizedExtension = string.IsNullOrWhiteSpace(extension) ? ".txt" : extension.ToLowerInvariant();

            switch (normalizedExtension)
            {
                case ".pdf":
                    return "PDF (*.pdf)|*.pdf|Все файлы (*.*)|*.*";
                case ".doc":
                    return "Документ Word (*.doc)|*.doc|Все файлы (*.*)|*.*";
                case ".docx":
                    return "Документ Word (*.docx)|*.docx|Все файлы (*.*)|*.*";
                case ".xlsx":
                    return "Таблица Excel (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*";
                case ".xls":
                    return "Таблица Excel (*.xls)|*.xls|Все файлы (*.*)|*.*";
                case ".pptx":
                    return "Презентация PowerPoint (*.pptx)|*.pptx|Все файлы (*.*)|*.*";
                case ".ppt":
                    return "Презентация PowerPoint (*.ppt)|*.ppt|Все файлы (*.*)|*.*";
                case ".txt":
                    return "Текстовый файл (*.txt)|*.txt|Все файлы (*.*)|*.*";
                default:
                    return string.Format("Файл документа (*{0})|*{0}|Все файлы (*.*)|*.*", normalizedExtension);
            }
        }

        private string GetDocumentTypeDisplay(string documentType)
        {
            switch (documentType)
            {
                case "working_program":
                    return "Рабочая программа";
                case "methodical_recommendation":
                    return "Методические рекомендации";
                case "regulation":
                    return "Нормативный документ";
                case "instruction":
                    return "Инструкция";
                default:
                    return documentType;
            }
        }
    }
}
