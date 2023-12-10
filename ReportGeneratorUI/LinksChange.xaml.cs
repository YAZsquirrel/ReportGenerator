using ReportGenerator;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ReportGeneratorUI;

public partial class LinksChange : Window
{
    ProductsDBContext db = new ProductsDBContext();
    bool adding = false;
    bool editing = false;

    bool unsavedChanges = false;
    record FullLinks(long UpId, long Id, string Name1, string Name2, long Count, Link LinkNavigation);
    public LinksChange()
    {
        InitializeComponent();
        PopulateLinksDatagrid(true);
    }

    private void PopulateLinksDatagrid(bool reload)
    {
        if (reload)
        {
           db.Dispose();
           db = new();
        }

        db.Links.Load();
        db.Products.Load();
        var links = from l in db.Links.Local
                    join p in db.Products.Local on l.UpProduct equals p.Id
                    join pp in db.Products.Local on l.Product equals pp.Id
                    select new FullLinks(
                            l.UpProduct,
                            l.Product,
                            p.Name,
                            pp.Name,
                            l.Count,
                            l);

        productsLeft.ItemsSource = db.Products.Local.ToList();
        productsRight.ItemsSource = db.Products.Local.ToList();
        linksData.ItemsSource = links.ToList();
    }

    private void save_Click(object sender, RoutedEventArgs e)
    {
        db.SaveChanges();
        unsavedChanges = false;
        save.IsEnabled = false;
    }

    private void cancel_Click(object sender, RoutedEventArgs e)
    {
        if (editing)
        {
            adding = false;
            SwitchToEditingMode(false);
        }
        PopulateLinksDatagrid(true);
    }
    private void add_Click(object sender, RoutedEventArgs e)
    {
        adding = true;

        SwitchToEditingMode(true);
    }

    private void delete_Click(object sender, RoutedEventArgs e)
    {
        if (linksData.SelectedItem is null) return;
        if (linksData.SelectedItem is FullLinks link)
            db.Links.Local.Remove(link.LinkNavigation);

        PopulateLinksDatagrid(false);
    }

    private void update_Click(object sender, RoutedEventArgs e)
    {
        if (linksData.SelectedItem is null) return;
        if (linksData.SelectedItem is FullLinks link)
        {
            var prod1 = link.LinkNavigation.UpProduct;
            var prod2 = link.LinkNavigation.Product;

            productsLeft.SelectedItem = prod1;
            productsRight.SelectedItem = prod2;

            input_count.Text = link.Count.ToString();
        }

        SwitchToEditingMode(true);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = !ProcessUnsavedChanges();
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
                    unsavedChanges = false;
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }
        return true;
    }

    private void confirm_Click(object sender, RoutedEventArgs e)
    {
        if (adding)
        {
            if (productsLeft.SelectedItem is null || productsRight.SelectedItem is null) return;

            if (productsLeft.SelectedItem is Product prod1 
                && productsRight.SelectedItem is Product prod2 
                && long.TryParse(input_count.Text, out long count))
            {
                if (!db.Links.Local.Any(x => x.UpProduct == prod1.Id && x.Product == prod2.Id))
                    db.Links.Local.Add(new Link { Count = count, UpProduct = prod1.Id, Product = prod2.Id });
            }
            adding = false;
        }
        else
        {
            if (linksData.SelectedItem is null) return;
            if (linksData.SelectedItem is FullLinks fulllink)
            {
                if (productsLeft.SelectedItem is Product prod1
                    && productsRight.SelectedItem is Product prod2
                    && long.TryParse(input_count.Text, out long count))
                {
                    Link link = fulllink.LinkNavigation;

                    link.UpProduct = prod1.Id;
                    link.Product = prod2.Id;
                    link.Count = count;

                    if (!db.Links.Local.Any(x => x.UpProduct == prod1.Id && x.Product == prod2.Id))
                    {
                        db.Links.Local.Remove(link);
                        db.Links.Local.Add(new Link { Count = count, UpProduct = prod1.Id, Product = prod2.Id });
                    }
                }
            }
        }
        unsavedChanges = true;
        save.IsEnabled = true;
        PopulateLinksDatagrid(false);
        SwitchToEditingMode(false);
    }

    private void input_count_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !long.TryParse(e.Text, out _);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        db.Dispose();
    }

    void SwitchToEditingMode(bool switchTo)
    {
        add.IsEnabled = !switchTo;
        save.IsEnabled = !switchTo;
        update.IsEnabled = !switchTo;
        delete.IsEnabled = !switchTo;
        editing = switchTo;

        if (switchTo)
        {
            products.Visibility = Visibility.Visible;
            editPanel.Visibility = Visibility.Visible;
            linksData.Visibility = Visibility.Hidden;
        }
        else
        {
            products.Visibility = Visibility.Hidden;
            editPanel.Visibility = Visibility.Hidden;
            linksData.Visibility = Visibility.Visible;
        }
    }
}

