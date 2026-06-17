import { useState, useEffect, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import type { SaleSummaryDto, PagedResult } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';
import { SearchInput } from '../components/SearchInput';
import { Pagination } from '../components/Pagination';
import { StatusBadge } from '../components/StatusBadge';

export default function SalesPage() {
  const navigate = useNavigate();
  const [sales, setSales] = useState<SaleSummaryDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dateFilter, setDateFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const fetchData = useCallback(async (q: string, d: string, st: string, p: number, ps: number) => {
    setLoading(true); setError(null);
    try {
      const params = new URLSearchParams();
      if (q) params.set('search', q);
      if (d) params.set('date', d);
      if (st) params.set('status', st);
      params.set('page', String(p));
      params.set('pageSize', String(ps));
      const data = await api.get<PagedResult<SaleSummaryDto>>(`/sales/filtered?${params}`);
      setSales(data.items);
      setTotalCount(data.totalCount);
      setTotalPages(data.totalPages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load sales');
    } finally { setLoading(false); }
  }, []);

  useEffect(() => {
    fetchData(searchQuery, dateFilter, statusFilter, page, pageSize);
  }, [searchQuery, dateFilter, statusFilter, page, pageSize, fetchData]);

  const handleSearch = (q: string) => {
    setSearchQuery(q);
    setPage(1);
  };

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold text-gray-900">Sales</h1>
        <button onClick={() => navigate('/sales/new')} className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">New Sale</button>
      </div>
      <div className="mb-4 flex flex-wrap gap-3">
        <div className="max-w-xs flex-1">
          <SearchInput placeholder="Search by customer or employee..." onSearch={handleSearch} />
        </div>
        <input type="date" value={dateFilter} onChange={e => { setDateFilter(e.target.value); setPage(1); }} className="rounded-md border px-3 py-2 text-sm" />
        <select value={statusFilter} onChange={e => { setStatusFilter(e.target.value); setPage(1); }} className="rounded-md border px-3 py-2 text-sm">
          <option value="">All statuses</option>
          <option value="0">Active</option>
          <option value="1">Cancelled</option>
        </select>
      </div>
      {loading ? <LoadingSpinner /> : error ? <ErrorMessage message={error} onRetry={() => fetchData(searchQuery, dateFilter, statusFilter, page, pageSize)} /> : sales.length === 0 ? <EmptyState message="No sales found" /> : (
        <div className="space-y-3">
          {sales.map((s) => (
            <Link key={s.id} to={`/sales/${s.id}`} className="block rounded-lg border bg-white p-4 shadow-sm hover:shadow-md">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-gray-900">#{s.id} — {s.customerName}</p>
                  <p className="text-sm text-gray-500">{s.employeeName} · {new Date(s.createdAt).toLocaleString()}</p>
                </div>
                <div className="text-right">
                  <p className="font-semibold text-gray-900">{s.totalAmount} EGP</p>
                  <p className="text-xs text-gray-500">{s.itemCount} items</p>
                </div>
              </div>
              <div className="mt-2">
                <StatusBadge status={s.status} />
              </div>
            </Link>
          ))}
          <Pagination currentPage={page} totalPages={totalPages} totalItems={totalCount} pageSize={pageSize} onPageChange={setPage} onPageSizeChange={s => { setPageSize(s); setPage(1); }} />
        </div>
      )}
    </div>
  );
}
