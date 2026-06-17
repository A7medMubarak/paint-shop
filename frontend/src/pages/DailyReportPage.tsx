import { useState, useEffect } from 'react';
import { api } from '../services/api';
import type { DailyReportDto } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';
import { downloadCsv, printElement } from '../utils/csv';

export default function DailyReportPage() {
  const [date, setDate] = useState(() => new Date().toISOString().split('T')[0]);
  const [report, setReport] = useState<DailyReportDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = async (d: string) => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.get<DailyReportDto>(`/reports/daily?date=${d}`);
      setReport(data);
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
        const data = await api.get<DailyReportDto>(`/reports/daily?date=${date}`);
        if (!ignore) setReport(data);
      } catch (err) {
        if (!ignore) setError(err instanceof Error ? err.message : 'Failed to load report');
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, [date]);

  const exportCsv = () => {
    if (!report) return;
    downloadCsv(`daily-report-${date}.csv`,
      ['Metric', 'Value'],
      [
        ['Total Sales', report.totalSalesCount],
        ['Revenue', report.totalRevenue.toFixed(2)],
        ['Discounts Given', report.totalDiscountsGiven.toFixed(2)],
        ['Cancelled', report.cancelledSalesCount],
        ['Avg Sale Value', report.totalSalesCount > 0 ? (report.totalRevenue / report.totalSalesCount).toFixed(2) : '0'],
      ]
    );
  };

  const avgSaleValue = report && report.totalSalesCount > 0
    ? report.totalRevenue / report.totalSalesCount
    : 0;

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Daily Report</h1>
        <div className="flex items-center gap-2">
          <input type="date" value={date} onChange={e => setDate(e.target.value)} className="rounded-md border px-3 py-2 text-sm" />
          {report && (
            <>
              <button onClick={() => printElement('daily-report-content')} className="rounded-md border bg-white px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors">Print</button>
              <button onClick={exportCsv} className="rounded-md border bg-white px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors">CSV</button>
            </>
          )}
        </div>
      </div>

      {loading ? <LoadingSpinner /> : error ? <ErrorMessage message={error} onRetry={() => load(date)} /> : !report ? <EmptyState /> : (
        <div id="daily-report-content" className="space-y-6">
          <div className="grid gap-4 sm:grid-cols-5">
            <div className="rounded-lg border bg-white p-4 shadow-sm">
              <p className="text-xs text-gray-500">Total Sales</p>
              <p className="text-2xl font-bold text-gray-900">{report.totalSalesCount}</p>
            </div>
            <div className="rounded-lg border bg-white p-4 shadow-sm">
              <p className="text-xs text-gray-500">Revenue</p>
              <p className="text-2xl font-bold text-green-600">{report.totalRevenue.toFixed(2)} EGP</p>
            </div>
            <div className="rounded-lg border bg-white p-4 shadow-sm">
              <p className="text-xs text-gray-500">Avg Sale</p>
              <p className="text-2xl font-bold text-blue-600">{avgSaleValue.toFixed(2)} EGP</p>
            </div>
            <div className="rounded-lg border bg-white p-4 shadow-sm">
              <p className="text-xs text-gray-500">Discounts Given</p>
              <p className="text-2xl font-bold text-yellow-600">{report.totalDiscountsGiven.toFixed(2)} EGP</p>
            </div>
            <div className="rounded-lg border bg-white p-4 shadow-sm">
              <p className="text-xs text-gray-500">Cancelled</p>
              <p className="text-2xl font-bold text-red-600">{report.cancelledSalesCount}</p>
            </div>
          </div>

          <div className="rounded-lg border bg-white p-4 shadow-sm">
            <h3 className="mb-3 font-medium text-gray-900">By Employee</h3>
            {report.salesByEmployee.length === 0 ? <EmptyState message="No sales" /> : (
              <table className="w-full text-sm">
                <thead className="text-left text-gray-500">
                  <tr><th className="pb-2 pr-2">Employee</th><th className="pb-2 pr-2">Sales</th><th className="pb-2 pr-2">Revenue</th><th className="pb-2">Discounts</th></tr>
                </thead>
                <tbody>
                  {report.salesByEmployee.map(e => (
                    <tr key={e.employeeId} className="border-t">
                      <td className="py-2 pr-2 font-medium">{e.employeeName}</td>
                      <td className="py-2 pr-2">{e.salesCount}</td>
                      <td className="py-2 pr-2">{e.revenue.toFixed(2)} EGP</td>
                      <td className="py-2">{e.discountsGiven.toFixed(2)} EGP</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          <div className="rounded-lg border bg-white p-4 shadow-sm">
            <h3 className="mb-3 font-medium text-gray-900">Top 10 Products</h3>
            {report.topVariants.length === 0 ? <EmptyState message="No sales" /> : (
              <table className="w-full text-sm">
                <thead className="text-left text-gray-500">
                  <tr><th className="pb-2 pr-2">Product</th><th className="pb-2 pr-2">Qty Sold</th><th className="pb-2">Revenue</th></tr>
                </thead>
                <tbody>
                  {report.topVariants.map((v, i) => (
                    <tr key={i} className="border-t">
                      <td className="py-2 pr-2">{v.variantName}</td>
                      <td className="py-2 pr-2">{v.quantitySold}</td>
                      <td className="py-2">{v.revenue.toFixed(2)} EGP</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
