import { useState, useEffect, useCallback } from 'react';
import { api } from '../services/api';
import type { InventoryItemDto, ProductDto, PagedResult } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';
import { SearchInput } from '../components/SearchInput';
import { Pagination } from '../components/Pagination';
import { useAuth } from '../hooks/useAuth';
import toast from 'react-hot-toast';
import { useNavigate } from 'react-router-dom';

export default function InventoryPage() {
  const { isOwner } = useAuth();
  const navigate = useNavigate();
  const [inventory, setInventory] = useState<InventoryItemDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [showAdd, setShowAdd] = useState(false);
  const [showTransfer, setShowTransfer] = useState(false);
  const [locationFilter, setLocationFilter] = useState<number | undefined>(undefined);
  const [searchQuery, setSearchQuery] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const [form, setForm] = useState({ productVariantId: 0, location: 0, quantity: 0, quantityChange: 0, notes: '', fromLocation: 0, toLocation: 1 });

  const fetchData = useCallback(async (loc: number | undefined, q: string, p: number, ps: number) => {
    setLoading(true); setError(null);
    try {
      const params = new URLSearchParams();
      if (loc !== undefined) params.set('location', String(loc));
      if (q) params.set('search', q);
      params.set('page', String(p));
      params.set('pageSize', String(ps));
      const [invResult, prods] = await Promise.all([
        api.get<PagedResult<InventoryItemDto>>(`/inventory/filtered?${params}`),
        api.get<ProductDto[]>('/products')
      ]);
      setInventory(invResult.items);
      setTotalCount(invResult.totalCount);
      setTotalPages(invResult.totalPages);
      setProducts(prods);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load inventory');
    } finally { setLoading(false); }
  }, []);

  useEffect(() => {
    fetchData(locationFilter, searchQuery, page, pageSize);
  }, [locationFilter, searchQuery, page, pageSize, fetchData]);

  const handleSearch = (q: string) => {
    setSearchQuery(q);
    setPage(1);
  };

  const handleAdd = async () => {
    try {
      await api.post('/inventory/add', { productVariantId: form.productVariantId, location: form.location, quantity: form.quantity });
      toast.success('Stock added'); setShowAdd(false);
      fetchData(locationFilter, searchQuery, page, pageSize);
    } catch { toast.error('Failed'); }
  };

  const handleAdjust = async () => {
    try {
      await api.post('/inventory/adjust', { productVariantId: form.productVariantId, location: form.location, quantityChange: form.quantityChange, notes: form.notes || undefined });
      toast.success('Stock adjusted'); setShowAdd(false);
      fetchData(locationFilter, searchQuery, page, pageSize);
    } catch { toast.error('Failed'); }
  };

  const handleTransfer = async () => {
    try {
      await api.post('/inventory/transfer', { productVariantId: form.productVariantId, quantity: form.quantity, fromLocation: form.fromLocation, toLocation: form.toLocation, notes: form.notes || undefined });
      toast.success('Stock transferred'); setShowTransfer(false);
      fetchData(locationFilter, searchQuery, page, pageSize);
    } catch { toast.error('Failed'); }
  };

  const allVariants = products.flatMap(p => p.variants.map(v => ({ ...v, productName: p.name })));

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} onRetry={() => fetchData(locationFilter, searchQuery, page, pageSize)} />;

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold text-gray-900">Inventory</h1>
        {isOwner && (
          <div className="flex gap-2">
            <button onClick={() => { setShowAdd(true); setShowTransfer(false); }} className="rounded-md bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-700">Add/Adjust</button>
            <button onClick={() => { setShowTransfer(true); setShowAdd(false); }} className="rounded-md bg-gray-600 px-3 py-1.5 text-sm text-white hover:bg-gray-700">Transfer</button>
          </div>
        )}
      </div>

      <div className="mb-4 flex gap-3">
        <div className="max-w-xs flex-1">
          <SearchInput placeholder="Search inventory..." onSearch={handleSearch} />
        </div>
        <select value={locationFilter ?? ''} onChange={e => { setLocationFilter(e.target.value ? +e.target.value : undefined); setPage(1); }} className="rounded-md border px-3 py-2 text-sm">
          <option value="">All Locations</option>
          <option value="0">Shop</option>
          <option value="1">Warehouse</option>
        </select>
        <span className="self-center text-xs text-gray-400">{totalCount} items</span>
      </div>

      {showAdd && (
        <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
          <h3 className="mb-3 font-medium">Add / Adjust Stock</h3>
          <div className="grid gap-3 sm:grid-cols-3">
            <select value={form.productVariantId} onChange={e => setForm({ ...form, productVariantId: +e.target.value })} className="rounded-md border px-3 py-2 text-sm">
              <option value={0}>Select variant</option>
              {allVariants.map(v => <option key={v.id} value={v.id}>{v.productName} ({v.baseType ?? '-'} {v.sizeValue}{v.sizeUnit})</option>)}
            </select>
            <select value={form.location} onChange={e => setForm({ ...form, location: +e.target.value })} className="rounded-md border px-3 py-2 text-sm">
              <option value={0}>Shop</option>
              <option value={1}>Warehouse</option>
            </select>
            <input type="number" step="0.01" placeholder="Qty" onChange={e => setForm({ ...form, quantity: +e.target.value })} className="rounded-md border px-3 py-2 text-sm" />
          </div>
          <div className="mt-2 flex gap-2">
            <input type="number" step="0.01" placeholder="Adjustment (+/-)" onChange={e => setForm({ ...form, quantityChange: +e.target.value })} className="rounded-md border px-3 py-2 text-sm" />
            <input type="text" placeholder="Notes (optional)" onChange={e => setForm({ ...form, notes: e.target.value })} className="rounded-md border px-3 py-2 text-sm" />
            <button onClick={handleAdd} className="rounded-md bg-green-600 px-3 py-2 text-sm text-white hover:bg-green-700">Add</button>
            <button onClick={handleAdjust} className="rounded-md bg-yellow-600 px-3 py-2 text-sm text-white hover:bg-yellow-700">Adjust</button>
          </div>
        </div>
      )}

      {showTransfer && (
        <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
          <h3 className="mb-3 font-medium">Transfer Stock</h3>
          <div className="grid gap-3 sm:grid-cols-4">
            <select value={form.productVariantId} onChange={e => setForm({ ...form, productVariantId: +e.target.value })} className="rounded-md border px-3 py-2 text-sm">
              <option value={0}>Select variant</option>
              {allVariants.map(v => <option key={v.id} value={v.id}>{v.productName} ({v.baseType ?? '-'} {v.sizeValue}{v.sizeUnit})</option>)}
            </select>
            <select value={form.fromLocation} onChange={e => setForm({ ...form, fromLocation: +e.target.value })} className="rounded-md border px-3 py-2 text-sm">
              <option value={0}>From: Shop</option>
              <option value={1}>From: Warehouse</option>
            </select>
            <select value={form.toLocation} onChange={e => setForm({ ...form, toLocation: +e.target.value })} className="rounded-md border px-3 py-2 text-sm">
              <option value={0}>To: Shop</option>
              <option value={1}>To: Warehouse</option>
            </select>
            <input type="number" step="0.01" placeholder="Quantity" onChange={e => setForm({ ...form, quantity: +e.target.value })} className="rounded-md border px-3 py-2 text-sm" />
          </div>
          <div className="mt-2 flex gap-2">
            <input type="text" placeholder="Notes (optional)" onChange={e => setForm({ ...form, notes: e.target.value })} className="rounded-md border px-3 py-2 text-sm" />
            <button onClick={handleTransfer} className="rounded-md bg-blue-600 px-3 py-2 text-sm text-white hover:bg-blue-700">Transfer</button>
          </div>
        </div>
      )}

      {inventory.length === 0 ? <EmptyState message="No matching inventory items" /> : (
        <div className="overflow-x-auto rounded-lg border bg-white shadow-sm">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-left">
              <tr>
                <th className="px-4 py-3 font-medium text-gray-600">Product</th>
                <th className="px-4 py-3 font-medium text-gray-600">Variant</th>
                <th className="px-4 py-3 font-medium text-gray-600">Shop</th>
                <th className="px-4 py-3 font-medium text-gray-600">Warehouse</th>
                <th className="px-4 py-3 font-medium text-gray-600">Status</th>
                <th className="px-4 py-3 font-medium text-gray-600" />
              </tr>
            </thead>
            <tbody>
              {inventory.map((item) => (
                <tr key={item.variantId} className="border-t">
                  <td className="px-4 py-3">{item.productName}</td>
                  <td className="px-4 py-3">{item.baseType ?? '-'} {item.sizeValue}{item.sizeUnit}</td>
                  <td className="px-4 py-3">{item.shopStock}</td>
                  <td className="px-4 py-3">{item.warehouseStock}</td>
                  <td className="px-4 py-3">
                    {item.isLowStock ? <span className="font-medium text-red-600">Low</span> : <span className="text-green-600">OK</span>}
                  </td>
                  <td className="px-4 py-3">
                    <button onClick={() => navigate(`/inventory/movements/${item.variantId}`)} className="text-xs text-blue-600 hover:text-blue-800">History</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      <Pagination currentPage={page} totalPages={totalPages} totalItems={totalCount} pageSize={pageSize} onPageChange={setPage} onPageSizeChange={s => { setPageSize(s); setPage(1); }} />
    </div>
  );
}
