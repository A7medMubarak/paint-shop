import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import type { DailyReportDto, SaleSummaryDto, LowStockReportDto, PagedResult } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { StatusBadge } from '../components/StatusBadge';

export default function DashboardPage() {
  const [report, setReport] = useState<DailyReportDto | null>(null);
  const [lowStock, setLowStock] = useState<LowStockReportDto[]>([]);
  const [recentSales, setRecentSales] = useState<SaleSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let ignore = false;
    (async () => {
      try {
        const today = new Date().toISOString().split('T')[0];
        const [r, ls, rs] = await Promise.all([
          api.get<DailyReportDto>(`/reports/daily?date=${today}`),
          api.get<LowStockReportDto[]>('/reports/low-stock'),
          api.get<PagedResult<SaleSummaryDto>>('/sales/filtered?Page=1&PageSize=5')
        ]);
        if (!ignore) {
          setReport(r);
          setLowStock(ls);
          setRecentSales(rs.items.slice(0, 5));
        }
      } catch {
        // silently fail on dashboard
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, []);

  if (loading) return <LoadingSpinner />;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>

      <div className="grid gap-4 sm:grid-cols-4">
        <div className="rounded-lg border bg-white p-4 shadow-sm">
          <p className="text-xs text-gray-500">Today's Sales</p>
          <p className="text-2xl font-bold text-gray-900">{report?.totalSalesCount ?? 0}</p>
        </div>
        <div className="rounded-lg border bg-white p-4 shadow-sm">
          <p className="text-xs text-gray-500">Revenue Today</p>
          <p className="text-2xl font-bold text-green-600">{(report?.totalRevenue ?? 0).toFixed(2)} EGP</p>
        </div>
        <div className="rounded-lg border bg-white p-4 shadow-sm">
          <p className="text-xs text-gray-500">Low Stock Items</p>
          <p className={`text-2xl font-bold ${lowStock.length > 0 ? 'text-red-600' : 'text-gray-900'}`}>{lowStock.length}</p>
        </div>
        <div className="rounded-lg border bg-white p-4 shadow-sm">
          <p className="text-xs text-gray-500">Cancelled Today</p>
          <p className="text-2xl font-bold text-red-600">{report?.cancelledSalesCount ?? 0}</p>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-lg border bg-white p-4 shadow-sm">
          <div className="mb-3 flex items-center justify-between">
            <h3 className="font-medium text-gray-900">Low Stock Alerts</h3>
            <Link to="/reports/low-stock" className="text-xs text-blue-600 hover:text-blue-800">View all</Link>
          </div>
          {lowStock.length === 0 ? (
            <p className="text-sm text-gray-500">All items are well-stocked</p>
          ) : (
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {lowStock.slice(0, 5).map(i => (
                <div key={i.variantId} className="flex justify-between text-sm">
                  <span className="text-gray-700">{i.productName} ({i.baseType ?? '-'} {i.sizeValue}{i.sizeUnit})</span>
                  <span className="font-medium text-red-600">{i.shopStock} / {i.threshold}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="rounded-lg border bg-white p-4 shadow-sm">
          <div className="mb-3 flex items-center justify-between">
            <h3 className="font-medium text-gray-900">Recent Sales</h3>
            <Link to="/sales" className="text-xs text-blue-600 hover:text-blue-800">View all</Link>
          </div>
          {recentSales.length === 0 ? (
            <p className="text-sm text-gray-500">No sales yet today</p>
          ) : (
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {recentSales.map(s => (
                <Link key={s.id} to={`/sales/${s.id}`} className="flex items-center justify-between rounded-md bg-gray-50 p-2 text-sm hover:bg-gray-100">
                  <div>
                    <span className="font-medium text-gray-900">#{s.id}</span>
                    <span className="ml-2 text-gray-600">{s.customerName}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{s.totalAmount.toFixed(2)} EGP</span>
                    <StatusBadge status={s.status} />
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
