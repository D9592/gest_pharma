namespace MiniPharmacie.Business.Models;

public sealed record CartLine(long ProductId,string Code,string Name,long UnitPriceMinor,int Quantity);
public sealed record Tender(string Method,long AmountMinor,string? Confirmation=null);
public sealed record SaleDraft(long UserId,long? CashSessionId,IReadOnlyList<CartLine> Lines,IReadOnlyList<Tender> Tenders,long DiscountMinor=0,string IdempotencyKey="");
public sealed record SaleReceipt(long SaleId,string Number,DateTimeOffset Date,long TotalMinor,long DiscountMinor,string Seller,IReadOnlyList<(string Name,int Quantity,long UnitPriceMinor)> Lines,IReadOnlyList<Tender> Payments,string PharmacyName,string PharmacyAddress,string PharmacyPhone);
