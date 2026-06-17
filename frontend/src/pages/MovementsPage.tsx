import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../services/api';
import type { StockMovementDto } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';

export default function MovementsPage() {
  const { variantId } = useParams();
  const [movements, setMovements] = useState<StockMovementDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const data = await api.get<StockMovementDto[]>(`/inventory/${variantId}/movements`);
        setMovements(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load movements');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [variantId]);

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold text-gray-900">Movement History</h1>
      {movements.length === 0 ? <EmptyState message="No movements recorded" /> : (
        <div className="overflow-x-auto rounded-lg border bg-white shadow-sm">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-left">
              <tr>
                <th className="px-4 py-3 font-medium text-gray-600">Date</th>
                <th className="px-4 py-3 font-medium text-gray-600">Location</th>
                <th className="px-4 py-3 font-medium text-gray-600">Change</th>
                <th className="px-4 py-3 font-medium text-gray-600">Reason</th>
                <th className="px-4 py-3 font-medium text-gray-600">By</th>
              </tr>
            </thead>
            <tbody>
              {movements.map((m) => (
                <tr key={m.id} className="border-t">
                  <td className="px-4 py-3">{new Date(m.createdAt).toLocaleString()}</td>
                  <td className="px-4 py-3">{m.location}</td>
                  <td className={`px-4 py-3 font-medium ${m.quantityChange > 0 ? 'text-green-600' : 'text-red-600'}`}>
                    {m.quantityChange > 0 ? '+' : ''}{m.quantityChange}
                  </td>
                  <td className="px-4 py-3">{m.reason}</td>
                  <td className="px-4 py-3">{m.createdBy}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
