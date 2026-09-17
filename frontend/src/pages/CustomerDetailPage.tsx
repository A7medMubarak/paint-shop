import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../services/api';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';
import { StatusBadge } from '../components/StatusBadge';
import toast from 'react-hot-toast';

interface CustomerDetailDto {
  id: number;
  name: string;
  phone?: string;
  createdAt: string;
  recentSales: Array<{
    id: number;
    employeeName: string;
    customerName: string;
    totalAmount: number;
    status: string;
    createdAt: string;
    itemCount: number;
  }>;
}

export default function CustomerDetailPage() {
  const { id } = useParams();
  const [customer, setCustomer] = useState<CustomerDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editing, setEditing] = useState(false);
  const [editName, setEditName] = useState('');
  const [editPhone, setEditPhone] = useState('');

  useEffect(() => {
    let ignore = false;
    (async () => {
      try {
        const data = await api.get<CustomerDetailDto>(`/customers/${id}`);
        if (!ignore) { setCustomer(data); setEditName(data.name); setEditPhone(data.phone ?? ''); }
      } catch (err) {
        if (!ignore) setError(err instanceof Error ? err.message : 'Failed to load customer');
      } finally { if (!ignore) setLoading(false); }
    })();
    return () => { ignore = true; };
  }, [id]);

  const handleEdit = async () => {
    if (!editName.trim()) { toast.error('Name is required'); return; }
    try {
      const updated = await api.put<{ id: number; name: string; phone?: string; createdAt: string }>(`/customers/${id}`, { name: editName, phone: editPhone || undefined });
      setCustomer(prev => prev ? { ...prev, ...updated, recentSales: prev.recentSales } : prev);
      setEditing(false);
      toast.success('Customer updated');
    } catch { toast.error('Failed to update'); }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;
  if (!customer) return null;

  if (editing) {
    return (
      <div className="mx-auto max-w-lg">
        <h1 className="mb-4 text-2xl font-bold text-gray-900">Edit Customer</h1>
        <div className="rounded-lg border bg-white p-4 shadow-sm">
          <div className="space-y-3">
            <input type="text" value={editName} onChange={e => setEditName(e.target.value)} className="w-full rounded-md border px-3 py-2 text-sm" placeholder="Name" />
            <input type="text" value={editPhone} onChange={e => setEditPhone(e.target.value)} className="w-full rounded-md border px-3 py-2 text-sm" placeholder="Phone (optional)" />
            <div className="flex gap-2">
              <button onClick={handleEdit} className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">Save</button>
              <button onClick={() => setEditing(false)} className="rounded-md border px-4 py-2 text-sm text-gray-700">Cancel</button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{customer.name}</h1>
            {customer.phone && <p className="text-gray-500">{customer.phone}</p>}
            <p className="mt-1 text-xs text-gray-400">Customer since {new Date(customer.createdAt).toLocaleDateString()}</p>
          </div>
          <button onClick={() => { setEditName(customer.name); setEditPhone(customer.phone ?? ''); setEditing(true); }} className="rounded-md border px-3 py-1.5 text-sm text-gray-700 hover:bg-gray-50">Edit</button>
        </div>
      </div>

      <h2 className="mb-3 text-lg font-semibold text-gray-900">Recent Sales</h2>
      {customer.recentSales.length === 0 ? <EmptyState message="No sales yet" /> : (
        <div className="space-y-2">
          {customer.recentSales.map((s) => (
            <Link key={s.id} to={`/sales/${s.id}`} className="block rounded-lg border bg-white p-3 shadow-sm hover:shadow-md">
              <div className="flex items-center justify-between">
                <div>
                  <span className="font-medium">#{s.id}</span>
                  <span className="ml-2 text-sm text-gray-500">{new Date(s.createdAt).toLocaleDateString()}</span>
                </div>
                <div className="text-right">
                  <span className="font-medium">{s.totalAmount} EGP</span>
                  <div className="mt-1"><StatusBadge status={s.status} /></div>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
