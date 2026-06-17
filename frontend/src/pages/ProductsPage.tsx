import { useEffect, useState, useCallback } from 'react';
import { api } from '../services/api';
import type { ProductDto, PagedResult } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';
import { SearchInput } from '../components/SearchInput';
import { Pagination } from '../components/Pagination';
import { useAuth } from '../hooks/useAuth';
import toast from 'react-hot-toast';

const categories = ['GlcPlastic', 'GlcDecore', 'GlcOilBased'];
const baseTypes = ['BaseA', 'BaseB', 'BaseC', 'White', 'Silver', 'Gold'];

interface ModalState {
  type: 'product' | 'variant' | null;
  productId?: number;
  variantId?: number;
  initial?: Record<string, unknown>;
}

export default function ProductsPage() {
  const { isOwner } = useAuth();
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const [modal, setModal] = useState<ModalState | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const fetchData = useCallback(async (q: string, p: number, ps: number) => {
    setLoading(true); setError(null);
    try {
      const params = new URLSearchParams();
      if (q) params.set('search', q);
      params.set('page', String(p));
      params.set('pageSize', String(ps));
      const data = await api.get<PagedResult<ProductDto>>(`/products/filtered?${params}`);
      setProducts(data.items);
      setTotalCount(data.totalCount);
      setTotalPages(data.totalPages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load products');
    } finally { setLoading(false); }
  }, []);

  useEffect(() => {
    fetchData(searchQuery, page, pageSize);
  }, [searchQuery, page, pageSize, fetchData]);

  const handleSearch = (q: string) => {
    setSearchQuery(q);
    setPage(1);
  };

  const refetch = () => fetchData(searchQuery, page, pageSize);

  const toggleVariant = async (variantId: number) => {
    try { await api.patch(`/products/variants/${variantId}/toggle`); toast.success('Variant toggled'); refetch(); } catch { toast.error('Failed'); }
  };
  const toggleProduct = async (id: number) => {
    try { await api.patch(`/products/${id}/toggle`); toast.success('Product toggled'); refetch(); } catch { toast.error('Failed'); }
  };

  const createProduct = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    try {
      await api.post('/products', { name: fd.get('name'), productCategory: categories.indexOf(fd.get('category') as string) });
      toast.success('Product created'); setModal(null); refetch();
    } catch { toast.error('Failed'); }
  };
  const editProduct = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    try {
      await api.put(`/products/${modal?.productId}`, { name: fd.get('name'), productCategory: categories.indexOf(fd.get('category') as string) });
      toast.success('Product updated'); setModal(null); refetch();
    } catch { toast.error('Failed'); }
  };
  const createVariant = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    const baseType = fd.get('baseType') as string;
    try {
      await api.post(`/products/${modal?.productId}/variants`, {
        baseType: baseType ? baseTypes.indexOf(baseType) : null,
        sizeValue: parseFloat(fd.get('sizeValue') as string),
        sizeUnit: fd.get('sizeUnit') || 'L',
        sellingPrice: parseFloat(fd.get('sellingPrice') as string) || null,
        costPrice: parseFloat(fd.get('costPrice') as string) || null,
        lowStockThreshold: parseFloat(fd.get('lowStockThreshold') as string) || null,
      });
      toast.success('Variant created'); setModal(null); refetch();
    } catch { toast.error('Failed'); }
  };
  const editVariant = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    try {
      await api.put(`/products/variants/${modal?.variantId}`, {
        sellingPrice: parseFloat(fd.get('sellingPrice') as string) || null,
        costPrice: parseFloat(fd.get('costPrice') as string) || null,
        lowStockThreshold: parseFloat(fd.get('lowStockThreshold') as string) || null,
      });
      toast.success('Variant updated'); setModal(null); refetch();
    } catch { toast.error('Failed'); }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} onRetry={refetch} />;

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Products</h1>
        {isOwner && (
          <button onClick={() => setModal({ type: 'product' })} className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">New Product</button>
        )}
      </div>
      <div className="mb-4 max-w-sm">
        <SearchInput placeholder="Search products..." onSearch={handleSearch} />
      </div>

      {products.length === 0 ? <EmptyState message="No products found" /> : (
        <div className="space-y-3">
          {products.map((p) => (
            <div key={p.id} className="rounded-lg border bg-white shadow-sm">
              <button onClick={() => setExpandedId(expandedId === p.id ? null : p.id)} className="flex w-full items-center justify-between px-4 py-3 text-left">
                <div className="flex items-center gap-2">
                  <span className="font-medium text-gray-900">{p.name}</span>
                  <span className="text-xs text-gray-500">{p.productCategory}</span>
                  {!p.isActive && <span className="text-xs text-red-500">(inactive)</span>}
                </div>
                <div className="flex items-center gap-2">
                  {isOwner && (
                    <>
                      <button onClick={(e) => { e.stopPropagation(); setModal({ type: 'variant', productId: p.id }); }} className="text-xs text-green-600 hover:text-green-800">+Variant</button>
                      <button onClick={(e) => { e.stopPropagation(); setModal({ type: 'product', productId: p.id, initial: { name: p.name, category: p.productCategory } }); }} className="text-xs text-blue-600 hover:text-blue-800">Edit</button>
                      <button onClick={(e) => { e.stopPropagation(); toggleProduct(p.id); }} className="text-xs text-orange-600 hover:text-orange-800">{p.isActive ? 'Deactivate' : 'Activate'}</button>
                    </>
                  )}
                  <svg className={`h-5 w-5 text-gray-400 transition-transform ${expandedId === p.id ? 'rotate-180' : ''}`} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </div>
              </button>
              {expandedId === p.id && (
                <div className="overflow-x-auto border-t px-4 py-3">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="text-left text-gray-500">
                        <th className="pb-2 pr-2">Base</th><th className="pb-2 pr-2">Size</th><th className="pb-2 pr-2">Sell Price</th><th className="pb-2 pr-2">Cost</th><th className="pb-2 pr-2">Shop</th><th className="pb-2 pr-2">Whse</th><th className="pb-2 pr-2">Threshold</th><th className="pb-2 pr-2">Status</th>
                        {isOwner && <th className="pb-2" />}
                      </tr>
                    </thead>
                    <tbody>
                      {p.variants.map((v) => (
                        <tr key={v.id} className="border-t">
                          <td className="py-2 pr-2">{v.baseType ?? '-'}</td>
                          <td className="py-2 pr-2">{v.sizeValue} {v.sizeUnit}</td>
                          <td className="py-2 pr-2">{v.sellingPrice != null ? `${v.sellingPrice} EGP` : 'N/A'}</td>
                          <td className="py-2 pr-2">{v.costPrice != null ? `${v.costPrice} EGP` : '-'}</td>
                          <td className="py-2 pr-2">{v.shopStock}</td>
                          <td className="py-2 pr-2">{v.warehouseStock}</td>
                          <td className="py-2 pr-2">{v.lowStockThreshold ?? '-'}</td>
                          <td className="py-2 pr-2">
                            {v.isLowStock ? <span className="text-red-600">Low</span> : <span className="text-green-600">OK</span>}
                            {!v.isActive && <span className="ml-1 text-gray-400">(off)</span>}
                          </td>
                          {isOwner && (
                            <td className="py-2">
                              <button onClick={() => setModal({ type: 'variant', variantId: v.id, productId: p.id, initial: { sellingPrice: v.sellingPrice, costPrice: v.costPrice, lowStockThreshold: v.lowStockThreshold } })} className="mr-2 text-xs text-blue-600 hover:text-blue-800">Edit</button>
                              <button onClick={() => toggleVariant(v.id)} className="text-xs text-orange-600 hover:text-orange-800">{v.isActive ? 'Off' : 'On'}</button>
                            </td>
                          )}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          ))}
          <Pagination currentPage={page} totalPages={totalPages} totalItems={totalCount} pageSize={pageSize} onPageChange={setPage} onPageSizeChange={s => { setPageSize(s); setPage(1); }} />
        </div>
      )}

      {modal?.type === 'product' && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30" onClick={() => setModal(null)}>
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl" onClick={e => e.stopPropagation()}>
            <h3 className="mb-4 text-lg font-medium">{modal.productId ? 'Edit Product' : 'New Product'}</h3>
            <form onSubmit={modal.productId ? editProduct : createProduct}>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm text-gray-600">Name</label>
                  <input name="name" defaultValue={(modal.initial?.name as string) ?? ''} required className="w-full rounded-md border px-3 py-2 text-sm" />
                </div>
                <div>
                  <label className="block text-sm text-gray-600">Category</label>
                  <select name="category" defaultValue={(modal.initial?.category as string) ?? categories[0]} className="w-full rounded-md border px-3 py-2 text-sm">
                    {categories.map(c => <option key={c} value={c}>{c}</option>)}
                  </select>
                </div>
              </div>
              <div className="mt-4 flex justify-end gap-2">
                <button type="button" onClick={() => setModal(null)} className="rounded-md border px-3 py-1.5 text-sm text-gray-700">Cancel</button>
                <button type="submit" className="rounded-md bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-700">Save</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {modal?.type === 'variant' && !modal.variantId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30" onClick={() => setModal(null)}>
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl" onClick={e => e.stopPropagation()}>
            <h3 className="mb-4 text-lg font-medium">New Variant</h3>
            <form onSubmit={createVariant}>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm text-gray-600">Base Type (optional)</label>
                  <select name="baseType" className="w-full rounded-md border px-3 py-2 text-sm">
                    <option value="">None</option>
                    {baseTypes.map(b => <option key={b} value={b}>{b}</option>)}
                  </select>
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <div>
                    <label className="block text-sm text-gray-600">Size Value</label>
                    <input name="sizeValue" type="number" step="0.01" required className="w-full rounded-md border px-3 py-2 text-sm" />
                  </div>
                  <div>
                    <label className="block text-sm text-gray-600">Unit</label>
                    <input name="sizeUnit" defaultValue="L" className="w-full rounded-md border px-3 py-2 text-sm" />
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <div>
                    <label className="block text-sm text-gray-600">Selling Price</label>
                    <input name="sellingPrice" type="number" step="0.01" className="w-full rounded-md border px-3 py-2 text-sm" />
                  </div>
                  <div>
                    <label className="block text-sm text-gray-600">Cost Price</label>
                    <input name="costPrice" type="number" step="0.01" className="w-full rounded-md border px-3 py-2 text-sm" />
                  </div>
                </div>
                <div>
                  <label className="block text-sm text-gray-600">Low Stock Threshold</label>
                  <input name="lowStockThreshold" type="number" step="0.01" className="w-full rounded-md border px-3 py-2 text-sm" />
                </div>
              </div>
              <div className="mt-4 flex justify-end gap-2">
                <button type="button" onClick={() => setModal(null)} className="rounded-md border px-3 py-1.5 text-sm text-gray-700">Cancel</button>
                <button type="submit" className="rounded-md bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-700">Save</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {modal?.type === 'variant' && modal.variantId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30" onClick={() => setModal(null)}>
          <div className="w-full max-w-md rounded-lg bg-white p-6 shadow-xl" onClick={e => e.stopPropagation()}>
            <h3 className="mb-4 text-lg font-medium">Edit Variant</h3>
            <form onSubmit={editVariant}>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm text-gray-600">Selling Price</label>
                  <input name="sellingPrice" type="number" step="0.01" defaultValue={(modal.initial?.sellingPrice as number) ?? ''} className="w-full rounded-md border px-3 py-2 text-sm" />
                </div>
                <div>
                  <label className="block text-sm text-gray-600">Cost Price</label>
                  <input name="costPrice" type="number" step="0.01" defaultValue={(modal.initial?.costPrice as number) ?? ''} className="w-full rounded-md border px-3 py-2 text-sm" />
                </div>
                <div>
                  <label className="block text-sm text-gray-600">Low Stock Threshold</label>
                  <input name="lowStockThreshold" type="number" step="0.01" defaultValue={(modal.initial?.lowStockThreshold as number) ?? ''} className="w-full rounded-md border px-3 py-2 text-sm" />
                </div>
              </div>
              <div className="mt-4 flex justify-end gap-2">
                <button type="button" onClick={() => setModal(null)} className="rounded-md border px-3 py-1.5 text-sm text-gray-700">Cancel</button>
                <button type="submit" className="rounded-md bg-blue-600 px-3 py-1.5 text-sm text-white hover:bg-blue-700">Save</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
