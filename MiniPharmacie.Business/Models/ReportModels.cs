namespace MiniPharmacie.Business.Models;

public sealed record DashboardSnapshot(long SalesTodayMinor,int OutOfStock,int AtMinimum,int ExpiringLots);
public sealed record ReportEntry(string Label,long Quantity,long RevenueMinor,long RefundMinor,long CostMinor,long MarginMinor);
