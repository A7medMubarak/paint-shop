import { useState, useEffect } from 'react';
import { api } from '../services/api';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';

interface TopSellingDto {
  variantId: number;
  variantName: string;
  quantitySold: number;
  revenue: number;
}

export default function TopSellingPage() {
  const [from, setFrom] = useState(() => {
    const d = new Date(); d.setDate(d.getDate() - 7); return d.toISOString().split('T')[0];
  });
  const [to, setTo] = useState(() => new Date().toISOString().split('T')[0]);
  const [items, setItems] = useState<TopSellingDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.get<TopSellingDto[]>(`/reports/top-selling?from=${from}&to=${to}`);
      setItems(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load report');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let ignore = false;
    (async () => {
      try {
        const data = await api.get<TopSellingDto[]>(`/reports/top-selling?from=${from}&to=${to}`);
        if (!ignore) setItems(data);
      } catch (err) {
        if (!ignore) setError(err instanceof Error ? err.message : 'Failed to load report');
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, [from, to]);

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Top Selling Products</h1>
        <div className="flex gap-2">
          <input type="date" value={from} onChange={e => setFrom(e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
          <input type="date" value={to} onChange={e => setTo(e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
        </div>
      </div>
      {loading ? <LoadingSpinner /> : error ? <ErrorMessage message={error} onRetry={load} /> : items.length === 0 ? <EmptyState message="No sales in this period" /> : (
        <div className="overflow-x-auto rounded-lg border bg-white shadow-sm">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-left">
              <tr>
                <th className="px-4 py-3 font-medium text-gray-600">#</th>
                <th className="px-4 py-3 font-medium text-gray-600">Product</th>
                <th className="px-4 py-3 font-medium text-gray-600">Qty Sold</th>
                <th className="px-4 py-3 font-medium text-gray-600">Revenue</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item, idx) => (
                <tr key={item.variantId} className="border-t">
                  <td className="px-4 py-3">{idx + 1}</td>
                  <td className="px-4 py-3">{item.variantName}</td>
                  <td className="px-4 py-3 font-medium">{item.quantitySold}</td>
                  <td className="px-4 py-3">{item.revenue.toFixed(2)} EGP</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
