import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import type { CustomerDto, PagedResult } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';
import { SearchInput } from '../components/SearchInput';
import { Pagination } from '../components/Pagination';
import toast from 'react-hot-toast';

export default function CustomersPage() {
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
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
      const data = await api.get<PagedResult<CustomerDto>>(`/customers/filtered?${params}`);
      setCustomers(data.items);
      setTotalCount(data.totalCount);
      setTotalPages(data.totalPages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load customers');
    } finally { setLoading(false); }
  }, []);

  useEffect(() => {
    fetchData(searchQuery, page, pageSize);
  }, [searchQuery, page, pageSize, fetchData]);

  const handleSearch = (q: string) => {
    setSearchQuery(q); setPage(1);
  };

  const handleCreate = async () => {
    if (!name.trim()) { toast.error('Name is required'); return; }
    try {
      await api.post('/customers', { name, phone: phone || undefined });
      toast.success('Customer created'); setShowCreate(false); setName(''); setPhone('');
      fetchData(searchQuery, page, pageSize);
    } catch { toast.error('Failed'); }
  };

  if (loading && customers.length === 0) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} onRetry={() => fetchData(searchQuery, page, pageSize)} />;

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Customers</h1>
        <button onClick={() => setShowCreate(!showCreate)} className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
          {showCreate ? 'Cancel' : 'New Customer'}
        </button>
      </div>

      {showCreate && (
        <div className="mb-6 rounded-lg border bg-white p-4 shadow-sm">
          <h3 className="mb-3 font-medium">New Customer</h3>
          <div className="grid gap-3 sm:grid-cols-3">
            <input type="text" placeholder="Name" value={name} onChange={e => setName(e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
            <input type="text" placeholder="Phone (optional)" value={phone} onChange={e => setPhone(e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
            <button onClick={handleCreate} className="rounded-md bg-green-600 px-3 py-2 text-sm text-white hover:bg-green-700">Save</button>
          </div>
        </div>
      )}

      <div className="mb-4 max-w-sm">
        <SearchInput placeholder="Search customers..." onSearch={handleSearch} />
      </div>

      {customers.length === 0 ? <EmptyState message={searchQuery ? 'No matching customers' : 'No customers yet'} /> : (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {customers.map((c) => (
              <Link key={c.id} to={`/customers/${c.id}`} className="rounded-lg border bg-white p-4 shadow-sm hover:shadow-md">
                <p className="font-medium text-gray-900">{c.name}</p>
                {c.phone && <p className="text-sm text-gray-500">{c.phone}</p>}
                <p className="mt-2 text-xs text-gray-400">Since {new Date(c.createdAt).toLocaleDateString()}</p>
              </Link>
            ))}
          </div>
          <Pagination currentPage={page} totalPages={totalPages} totalItems={totalCount} pageSize={pageSize} onPageChange={setPage} onPageSizeChange={s => { setPageSize(s); setPage(1); }} />
        </>
      )}
    </div>
  );
}
