namespace MiniPharmacie.Business.Models;

public sealed record ProductDraft(string Code,string Name,long? CategoryId,string Dosage,string PharmaceuticalForm,string Unit,long SalePriceMinor,int ReorderLevel,string? Barcode,string? ShelfLocation);
public sealed record ProductStock(long ProductId,string Code,string Name,int Physical,int Sellable,int ReorderLevel,long SalePriceMinor=0);
public sealed record ReceiptLineDraft(long ProductId,int Quantity,long UnitCostMinor,string LotNumber,DateOnly? ExpiryDate);
public sealed record ReceiptDraft(long SupplierId,long UserId,string Reference,DateTimeOffset ReceivedAt,IReadOnlyList<ReceiptLineDraft> Lines,string? Notes);
public sealed record StockAlert(string Type,long ProductId,string Product,string Lot,int Quantity,string Message);
public sealed record InventoryDifference(long LotId,long ProductId,string Product,int Expected,int Counted);
