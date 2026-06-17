import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import type { ProductDto, CustomerDto, CreateSaleRequest, CreateSaleItemRequest, CreateSaleResponse } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import toast from 'react-hot-toast';

export default function CreateSalePage() {
  const navigate = useNavigate();
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [customerId, setCustomerId] = useState(1);
  const [discount, setDiscount] = useState(0);
  const [lineItems, setLineItems] = useState<Array<CreateSaleItemRequest & { productName?: string; label?: string; originalPrice?: number | null }>>([]);
  const [selectedVariant, setSelectedVariant] = useState(0);
  const [selectedQty, setSelectedQty] = useState(1);
  const [selectedPrice, setSelectedPrice] = useState<number | ''>('');
  const [submitting, setSubmitting] = useState(false);
  const [forceNegative, setForceNegative] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const load = async () => {
      try {
        const [prods, custs] = await Promise.all([
          api.get<ProductDto[]>('/products'),
          api.get<CustomerDto[]>('/customers')
        ]);
        setProducts(prods);
        setCustomers(custs);
      } catch {
        toast.error('Failed to load data');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const allVariants = products.flatMap(p =>
    p.variants.filter(v => v.isActive).map(v => ({
      ...v,
      productName: p.name,
      label: `${p.name} (${v.baseType ?? '-'} ${v.sizeValue}${v.sizeUnit})`
    }))
  );

  const filteredVariants = searchTerm
    ? allVariants.filter(v => v.label.toLowerCase().includes(searchTerm.toLowerCase()))
    : allVariants;

  const addItem = () => {
    if (!selectedVariant) { toast.error('Select a variant'); return; }
    const v = allVariants.find(x => x.id === selectedVariant);
    if (!v) return;
    const price = selectedPrice === '' ? (v.sellingPrice ?? 0) : Number(selectedPrice);
    setLineItems([...lineItems, {
      productVariantId: v.id,
      quantity: selectedQty,
      unitPrice: price,
      productName: v.productName,
      label: v.label,
      originalPrice: v.sellingPrice
    }]);
    setSelectedVariant(0);
    setSelectedQty(1);
    setSelectedPrice('');
    setSearchTerm('');
  };

  const removeItem = (idx: number) => {
    setLineItems(lineItems.filter((_, i) => i !== idx));
  };

  const updateQty = (idx: number, qty: number) => {
    const updated = [...lineItems];
    updated[idx] = { ...updated[idx], quantity: qty };
    setLineItems(updated);
  };

  const subtotal = lineItems.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);
  const total = Math.max(0, subtotal - discount);
  const totalPerLineDiscount = lineItems.reduce((sum, i) => sum + ((i.originalPrice ?? i.unitPrice) - i.unitPrice) * i.quantity, 0);

  const handleSubmit = async () => {
    if (lineItems.length === 0) { toast.error('Add at least one item'); return; }
    setSubmitting(true);
    try {
      const request: CreateSaleRequest = {
        customerId,
        discountAmount: discount,
        forceNegativeInventory: forceNegative,
        items: lineItems.map(i => ({ productVariantId: i.productVariantId, quantity: i.quantity, unitPrice: i.unitPrice }))
      };
      const response = await api.post<CreateSaleResponse>('/sales', request);
      if (response.inventoryWarning) toast('Some items exceeded available stock', { icon: '⚠️' });
      toast.success('Sale created');
      navigate(`/sales/${response.sale.id}`);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to create sale');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <div className="mx-auto max-w-3xl">
      <h1 className="mb-6 text-2xl font-bold text-gray-900">New Sale</h1>

      <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
        <label className="block text-sm font-medium text-gray-700">Customer</label>
        <select value={customerId} onChange={e => setCustomerId(+e.target.value)} className="mt-1 w-full rounded-md border px-3 py-2 text-sm">
          {customers.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
      </div>

      <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
        <h3 className="mb-3 font-medium text-gray-900">Add Items</h3>
        <input type="text" placeholder="Search variant..." value={searchTerm} onChange={e => setSearchTerm(e.target.value)} className="mb-2 w-full rounded-md border px-3 py-2 text-sm" />
        <div className="grid gap-2 sm:grid-cols-4">
          <select value={selectedVariant} onChange={e => setSelectedVariant(+e.target.value)} className="rounded-md border px-3 py-2 text-sm">
            <option value={0}>Select variant</option>
            {filteredVariants.map(v => (
              <option key={v.id} value={v.id}>
                {v.label} — {v.sellingPrice != null ? `${v.sellingPrice} EGP` : 'No price'}
              </option>
            ))}
          </select>
          <input type="number" step="0.01" placeholder="Qty" value={selectedQty} onChange={e => setSelectedQty(+e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
          <input type="number" step="0.01" placeholder="Unit price" value={selectedPrice} onChange={e => setSelectedPrice(e.target.value === '' ? '' : +e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
          <button onClick={addItem} className="rounded-md bg-blue-600 px-3 py-2 text-sm text-white hover:bg-blue-700">Add</button>
        </div>
      </div>

      {lineItems.length > 0 && (
        <div className="mb-6 rounded-lg border bg-white shadow-sm">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-left">
              <tr>
                <th className="px-4 py-3 font-medium text-gray-600">Item</th>
                <th className="px-4 py-3 font-medium text-gray-600">Qty</th>
                <th className="px-4 py-3 font-medium text-gray-600">Unit Price</th>
                <th className="px-4 py-3 font-medium text-gray-600">Subtotal</th>
                <th className="px-4 py-3 font-medium text-gray-600" />
              </tr>
            </thead>
            <tbody>
              {lineItems.map((item, idx) => (
                <tr key={idx} className="border-t">
                  <td className="px-4 py-3">{item.label}</td>
                  <td className="px-4 py-3">
                    <input type="number" step="0.01" value={item.quantity} onChange={e => updateQty(idx, +e.target.value)} className="w-20 rounded-md border px-2 py-1 text-sm" />
                  </td>
                  <td className="px-4 py-3">{item.unitPrice} EGP</td>
                  <td className="px-4 py-3 font-medium">{(item.unitPrice * item.quantity).toFixed(2)} EGP</td>
                  <td className="px-4 py-3">
                    <button onClick={() => removeItem(idx)} className="text-red-600 hover:text-red-800">Remove</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
        <div className="space-y-2">
          <div className="flex justify-between text-sm">
            <span>Subtotal</span>
            <span>{subtotal.toFixed(2)} EGP</span>
          </div>
          {totalPerLineDiscount > 0 && (
            <div className="flex justify-between text-sm text-green-600">
              <span>Per-line discounts</span>
              <span>-{totalPerLineDiscount.toFixed(2)} EGP</span>
            </div>
          )}
          <div className="flex items-center justify-between text-sm">
            <span>Discount</span>
            <input type="number" step="0.01" value={discount} onChange={e => setDiscount(+e.target.value)} className="w-24 rounded-md border px-2 py-1 text-right text-sm" />
          </div>
          <div className="flex justify-between font-semibold text-gray-900">
            <span>Total</span>
            <span>{total.toFixed(2)} EGP</span>
          </div>
        </div>
      </div>

      <div className="mb-6 flex items-center gap-2">
        <input type="checkbox" id="forceNegative" checked={forceNegative} onChange={e => setForceNegative(e.target.checked)} className="rounded" />
        <label htmlFor="forceNegative" className="text-sm text-gray-600">Allow negative inventory (force)</label>
      </div>

      <button onClick={handleSubmit} disabled={submitting || lineItems.length === 0}
        className="w-full rounded-md bg-blue-600 py-3 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
        {submitting ? 'Creating...' : 'Complete Sale'}
      </button>
    </div>
  );
}
