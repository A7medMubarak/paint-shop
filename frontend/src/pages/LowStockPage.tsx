import { useState, useEffect } from 'react';
import { api } from '../services/api';
import type { LowStockReportDto } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';

export default function LowStockPage() {
  const [items, setItems] = useState<LowStockReportDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const data = await api.get<LowStockReportDto[]>('/reports/low-stock');
        setItems(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load report');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold text-gray-900">Low Stock Report</h1>
      {items.length === 0 ? <EmptyState message="No low stock items" /> : (
        <div className="overflow-x-auto rounded-lg border bg-white shadow-sm">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-left">
              <tr>
                <th className="px-4 py-3 font-medium text-gray-600">Product</th>
                <th className="px-4 py-3 font-medium text-gray-600">Variant</th>
                <th className="px-4 py-3 font-medium text-gray-600">Shop Stock</th>
                <th className="px-4 py-3 font-medium text-gray-600">Threshold</th>
              </tr>
            </thead>
            <tbody>
              {items.map((i) => (
                <tr key={i.variantId} className="border-t">
                  <td className="px-4 py-3 font-medium">{i.productName}</td>
                  <td className="px-4 py-3">{i.baseType} {i.sizeValue}{i.sizeUnit}</td>
                  <td className="px-4 py-3 font-medium text-red-600">{i.shopStock}</td>
                  <td className="px-4 py-3">{i.threshold}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
