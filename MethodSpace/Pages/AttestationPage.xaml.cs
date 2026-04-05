using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class AttestationPage : Page
    {
        private SQL _context;
        private int _userId;
        private string _userRole;

        public AttestationPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
            _userId = userId;
            _userRole = userRole;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var attestations = _context.TeacherAttestations
                .Select(a => new
                {
                    a.AttestationID,
                    TeacherName = a.User != null ? a.User.FullName : "Неизвестно",
                    a.AttestationDate,
                    a.Result,
                    a.CertificateNumber,
                    a.Comments
                })
                .OrderByDescending(a => a.AttestationDate)
                .ToList();

            AttestationList.ItemsSource = attestations;
        }
    }
}