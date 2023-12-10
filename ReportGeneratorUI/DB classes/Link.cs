using Microsoft.EntityFrameworkCore;

namespace ReportGenerator;
public partial class Link
{
    public long UpProduct { get; set; }
    public long Product { get; set; }
    public long Count { get; set; }

    public virtual Product ProductNavigation { get; set; } = null!;
    public virtual Product UpProductNavigation { get; set; } = null!;
}
