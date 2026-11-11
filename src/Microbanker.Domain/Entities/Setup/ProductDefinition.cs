namespace Microbanker.Domain.Entities.Setup;

public class ProductDefinition : BaseEntity
{
    public static readonly string[] CalcMethods = { "SIMPLE", "COMPOUND", "DAILY_BALANCE", "MINIMUM_BALANCE" };

    public Guid OrganizationId { get; private set; }
    public Guid ProductTypeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public decimal MinBalance { get; private set; }
    /// <summary>Annual interest rate in percent (e.g. 5.25 = 5.25%).</summary>
    public decimal InterestRate { get; private set; }
    public string? InterestCalcMethod { get; private set; }
    /// <summary>Months between interest postings (0 = none).</summary>
    public int InterestFrequency { get; private set; }
    public int DormancyDays { get; private set; }
    public int MaxDebitPerMonth { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ProductType ProductType { get; private set; } = null!;

    private ProductDefinition() { }

    public ProductDefinition(Guid orgId, Guid productTypeId, string name, string code)
    {
        OrganizationId = orgId;
        ProductTypeId = productTypeId;
        Name = name;
        Code = code;
        IsActive = true;
    }

    public void Update(Guid productTypeId, string name, string code, decimal minBalance, decimal interestRate,
        string? interestCalcMethod, int interestFrequency, int dormancyDays, int maxDebitPerMonth,
        string? description, bool isActive)
    {
        ProductTypeId = productTypeId;
        Name = name;
        Code = code;
        MinBalance = minBalance;
        InterestRate = interestRate;
        InterestCalcMethod = interestCalcMethod;
        InterestFrequency = interestFrequency;
        DormancyDays = dormancyDays;
        MaxDebitPerMonth = maxDebitPerMonth;
        Description = description;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
