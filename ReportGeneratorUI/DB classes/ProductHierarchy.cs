namespace ReportGenerator;

public class ProductHierarchy
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long Count { get; set; } 
    public decimal Cost { get; set; }  
    public decimal Price { get; set; }
    public long InclusionCount { get; set; }
    public long Level { get; set; }
    public Product ProductNavigation { get; set; } = null!;

}
