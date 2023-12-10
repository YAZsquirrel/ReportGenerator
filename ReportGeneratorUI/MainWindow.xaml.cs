using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ReportGenerator;
using System.IO;
using System.Windows;

namespace ReportGeneratorUI;

public partial class MainWindow : Window
{
    private bool unsavedChanges = false;
    ProductsDBContext db;
    readonly string getHierarchyQuery;
    public MainWindow()
    {
        getHierarchyQuery = File.ReadAllText("./Queries/ReportQuery.sql");

        InitializeComponent();

        db = new ProductsDBContext();
        db.Database.EnsureCreated();

        db.Products.Load();

        var ps = db.Products.Local.ToBindingList();
        ps.ListChanged += (_, _) => save.IsEnabled = unsavedChanges = true;
        productData.ItemsSource = ps;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        window?.Close();
        e.Cancel = !ProcessUnsavedChanges();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        db.Dispose();
    }

    private void report_Click(object sender, RoutedEventArgs e)
    {

        if (!ProcessUnsavedChanges()) return;
            
        SaveFileDialog save = new() { Filter = "Office Excel (*.xlsx)|*.xlsx", AddExtension = true };

        save.ShowDialog();

        var result = db.ProductHierarchy
            .FromSqlRaw(getHierarchyQuery)
            .AsEnumerable()
            .Where(x => x.Level <= maxLevels);

        if (save.FileName != string.Empty)
            ExcelReporter.SaveReport(save.FileName, result);
    }

    Window window;
    private void changeLinks_Click(object sender, RoutedEventArgs e)
    {
        window = new LinksChange();
        window.Activate();
        window.Show();
    }

    private void save_Click(object sender, RoutedEventArgs e)
    {
        db.SaveChanges();
        save.IsEnabled = unsavedChanges = false;
    }

    private void cb_showHierarchy_Checked(object sender, RoutedEventArgs e)
    {
        if (!ProcessUnsavedChanges()) return;

        levels.Maximum = db.ProductHierarchy.FromSqlRaw(getHierarchyQuery).AsEnumerable().Max(x => x.Level); 

        ShowHierarchyInDatagrid();
    }

    private void ShowHierarchyInDatagrid()
    {
        var result = db.ProductHierarchy
            .FromSqlRaw(getHierarchyQuery)
            .AsEnumerable()
            .Where(x => x.Level <= maxLevels)
            .Select(x => new
            {
                Name = new string(' ', (int)x.Level * 2) + x.Name,
                x.Count,
                x.Cost,
                x.Price,
                x.InclusionCount
            });

        hierarchyData.ItemsSource = result.ToList();

        productData.Visibility = Visibility.Hidden;
        hierarchyData.Visibility = Visibility.Visible;
    }

    private void cb_showHierarchy_Unchecked(object sender, RoutedEventArgs e)
    {
        db.Products.Load();
        var ps = db.Products.Local.ToBindingList(); 
        ps.ListChanged += (_, _) => unsavedChanges = save.IsEnabled = true;

        productData.ItemsSource = ps;

        productData.Visibility = Visibility.Visible;
        hierarchyData.Visibility = Visibility.Hidden;
    }

    bool ProcessUnsavedChanges()
    {
        if (unsavedChanges)
        {
            var res = MessageBox.Show("Остались несохраненные изменения", "Сохранить?", MessageBoxButton.YesNoCancel);
            switch (res)
            {
                case MessageBoxResult.Cancel:
                    return false;
                case MessageBoxResult.Yes:
                    db.SaveChanges();
                    save.IsEnabled = unsavedChanges = false;
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }
        return true;
    }
    int maxLevels = 1;
    private void levels_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        maxLevels = (int)e.NewValue;
        if (L_level is not null)
            L_level.Text = maxLevels.ToString();

        if (cb_showHierarchy.IsChecked!.Value)
            ShowHierarchyInDatagrid();
    }

    private void levels_Loaded(object sender, RoutedEventArgs e)
    {
        levels.Maximum = maxLevels = (int)db.ProductHierarchy.FromSqlRaw(getHierarchyQuery).AsEnumerable().Max(x => x.Level);
        levels.Value = maxLevels;
    }
}