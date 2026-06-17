export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  role: string;
  username: string;
}

export interface UserDto {
  id: number;
  username: string;
  role: string;
  createdAt: string;
}

export interface CreateEmployeeRequest {
  username: string;
  password: string;
}

export interface ProductDto {
  id: number;
  name: string;
  productCategory: string;
  isActive: boolean;
  createdAt: string;
  variants: ProductVariantDto[];
}

export interface ProductVariantDto {
  id: number;
  productId: number;
  baseType: string | null;
  sizeValue: number;
  sizeUnit: string;
  sellingPrice: number | null;
  costPrice: number | null;
  lowStockThreshold: number | null;
  isActive: boolean;
  shopStock: number;
  warehouseStock: number;
  isLowStock: boolean;
}

export interface CreateProductRequest {
  name: string;
  productCategory: number;
}

export interface CreateProductVariantRequest {
  baseType?: number | null;
  sizeValue: number;
  sizeUnit: string;
  sellingPrice?: number | null;
  costPrice?: number | null;
  lowStockThreshold?: number | null;
}

export interface UpdateProductVariantRequest {
  sellingPrice?: number | null;
  costPrice?: number | null;
  lowStockThreshold?: number | null;
  isActive?: boolean;
}

export interface InventoryItemDto {
  variantId: number;
  productName: string;
  baseType: string | null;
  sizeValue: number;
  sizeUnit: string;
  shopStock: number;
  warehouseStock: number;
  isLowStock: boolean;
}

export interface AddStockRequest {
  productVariantId: number;
  location: number;
  quantity: number;
}

export interface AdjustStockRequest {
  productVariantId: number;
  location: number;
  quantityChange: number;
  notes?: string;
}

export interface TransferStockRequest {
  productVariantId: number;
  quantity: number;
  fromLocation: number;
  toLocation: number;
  notes?: string;
}

export interface StockMovementDto {
  id: number;
  location: string;
  quantityChange: number;
  reason: string;
  createdBy: string;
  createdAt: string;
}

export interface CreateSaleRequest {
  customerId: number;
  discountAmount: number;
  forceNegativeInventory: boolean;
  items: CreateSaleItemRequest[];
}

export interface CreateSaleItemRequest {
  productVariantId: number;
  quantity: number;
  unitPrice: number;
  colorCode?: string;
}

export interface CreateSaleResponse {
  sale: SaleDto;
  inventoryWarning: boolean;
  warningVariantIds: number[];
}

export interface SaleDto {
  id: number;
  employeeName: string;
  customerName: string;
  discountAmount: number;
  totalAmount: number;
  subtotal: number;
  status: string;
  createdAt: string;
  items: SaleItemDto[];
}

export interface SaleItemDto {
  id: number;
  productVariantId: number;
  productName: string;
  baseType: string | null;
  sizeValue: number;
  sizeUnit: string;
  quantity: number;
  originalPrice: number | null;
  unitPrice: number;
  colorCode?: string;
}

export interface SaleSummaryDto {
  id: number;
  employeeName: string;
  customerName: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  itemCount: number;
}

export interface CustomerDto {
  id: number;
  name: string;
  phone?: string;
  createdAt: string;
}

export interface CreateCustomerRequest {
  name: string;
  phone?: string;
}

export interface DailyReportDto {
  date: string;
  totalSalesCount: number;
  totalRevenue: number;
  totalDiscountsGiven: number;
  cancelledSalesCount: number;
  salesByEmployee: EmployeeSalesDto[];
  topVariants: TopProductDto[];
}

export interface EmployeeSalesDto {
  employeeId: number;
  employeeName: string;
  salesCount: number;
  revenue: number;
  discountsGiven: number;
}

export interface TopProductDto {
  variantId: number;
  variantName: string;
  quantitySold: number;
  revenue: number;
}

export interface LowStockReportDto {
  variantId: number;
  productName: string;
  baseType: string | null;
  sizeValue: number;
  sizeUnit: string;
  shopStock: number;
  threshold: number;
}

export interface PeriodReportDto {
  from: string;
  to: string;
  totalSalesCount: number;
  totalRevenue: number;
  totalDiscountsGiven: number;
  cancelledSalesCount: number;
  salesByEmployee: EmployeeSalesDto[];
  topVariants: TopProductDto[];
}

export interface InventoryValuationDto {
  location: string;
  totalItems: number;
  totalValue: number;
  items: InventoryItemValuationDto[];
}

export interface InventoryItemValuationDto {
  variantId: number;
  productName: string;
  baseType: string | null;
  sizeValue: number;
  sizeUnit: string;
  quantity: number;
  costPrice: number | null;
  sellingPrice: number | null;
  itemValue: number;
}
