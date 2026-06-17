import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../services/api';
import type { SaleDto } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { StatusBadge } from '../components/StatusBadge';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { useAuth } from '../hooks/useAuth';
import toast from 'react-hot-toast';

export default function SaleDetailPage() {
  const { id } = useParams();
  const { isOwner } = useAuth();
  const [sale, setSale] = useState<SaleDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCancel, setShowCancel] = useState(false);

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.get<SaleDto>(`/sales/${id}`);
      setSale(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load sale');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let ignore = false;
    (async () => {
      try {
        const data = await api.get<SaleDto>(`/sales/${id}`);
        if (!ignore) setSale(data);
      } catch (err) {
        if (!ignore) setError(err instanceof Error ? err.message : 'Failed to load sale');
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, [id]);

  const handleCancel = async () => {
    try {
      await api.patch(`/sales/${id}/cancel`);
      toast.success('Sale cancelled');
      setShowCancel(false);
      load();
    } catch { toast.error('Failed to cancel sale'); }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} onRetry={load} />;
  if (!sale) return null;

  return (
    <div className="mx-auto max-w-3xl">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Sale #{sale.id}</h1>
        <div className="flex gap-2">
          <Link to={`/sales/${id}/invoice`} className="rounded-md bg-gray-600 px-3 py-1.5 text-sm text-white hover:bg-gray-700">Invoice</Link>
          {isOwner && sale.status === 'Active' && (
            <button onClick={() => setShowCancel(true)} className="rounded-md bg-red-600 px-3 py-1.5 text-sm text-white hover:bg-red-700">Cancel</button>
          )}
        </div>
      </div>

      <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
        <div className="grid gap-4 sm:grid-cols-3">
          <div>
            <span className="text-xs text-gray-500">Employee</span>
            <p className="font-medium">{sale.employeeName}</p>
          </div>
          <div>
            <span className="text-xs text-gray-500">Customer</span>
            <p className="font-medium">{sale.customerName}</p>
          </div>
          <div>
            <span className="text-xs text-gray-500">Status</span>
            <StatusBadge status={sale.status} />
          </div>
          <div>
            <span className="text-xs text-gray-500">Date</span>
            <p className="font-medium">{new Date(sale.createdAt).toLocaleString()}</p>
          </div>
          <div>
            <span className="text-xs text-gray-500">Subtotal</span>
            <p className="font-medium">{sale.subtotal.toFixed(2)} EGP</p>
          </div>
          <div>
            <span className="text-xs text-gray-500">Discount</span>
            <p className="font-medium">{sale.discountAmount.toFixed(2)} EGP</p>
          </div>
        </div>
        <div className="mt-4 border-t pt-4">
          <span className="text-lg font-bold text-gray-900">Total: {sale.totalAmount.toFixed(2)} EGP</span>
        </div>
      </div>

      <div className="rounded-lg border bg-white shadow-sm">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-left">
            <tr>
              <th className="px-4 py-3 font-medium text-gray-600">Product</th>
              <th className="px-4 py-3 font-medium text-gray-600">Qty</th>
              <th className="px-4 py-3 font-medium text-gray-600">Original Price</th>
              <th className="px-4 py-3 font-medium text-gray-600">Unit Price</th>
              <th className="px-4 py-3 font-medium text-gray-600">Subtotal</th>
            </tr>
          </thead>
          <tbody>
            {sale.items.map((item) => (
              <tr key={item.id} className="border-t">
                <td className="px-4 py-3">
                  {item.productName}
                  <div className="text-xs text-gray-500">{item.baseType ?? '-'} {item.sizeValue}{item.sizeUnit}{item.colorCode ? ` · ${item.colorCode}` : ''}</div>
                </td>
                <td className="px-4 py-3">{item.quantity}</td>
                <td className="px-4 py-3">{item.originalPrice != null ? `${item.originalPrice} EGP` : '-'}</td>
                <td className="px-4 py-3">{item.unitPrice} EGP</td>
                <td className="px-4 py-3 font-medium">{(item.unitPrice * item.quantity).toFixed(2)} EGP</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <ConfirmDialog open={showCancel} title="Cancel Sale" message="Are you sure you want to cancel this sale? Inventory will be restored." confirmLabel="Cancel Sale" onConfirm={handleCancel} onCancel={() => setShowCancel(false)} />
    </div>
  );
}
