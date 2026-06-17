import { useState, useEffect } from 'react';
import { api } from '../services/api';
import type { InventoryValuationDto } from '../types';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';
import { EmptyState } from '../components/EmptyState';
import { downloadCsv, printElement } from '../utils/csv';

export default function InventoryValuationPage() {
  const [data, setData] = useState<InventoryValuationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const result = await api.get<InventoryValuationDto[]>('/reports/inventory-valuation');
        setData(result);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load report');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const exportCsv = () => {
    downloadCsv('inventory-valuation.csv',
      ['Location', 'Product', 'Variant', 'Quantity', 'Cost Price', 'Selling Price', 'Item Value'],
      data.flatMap(loc =>
        loc.items.map(i => [
          loc.location,
          i.productName,
          i.baseType ? `${i.baseType} ${i.sizeValue}${i.sizeUnit}` : `${i.sizeValue}${i.sizeUnit}`,
          i.quantity,
          i.costPrice?.toFixed(2) ?? '-',
          i.sellingPrice?.toFixed(2) ?? '-',
          i.itemValue.toFixed(2),
        ])
      )
    );
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Inventory Valuation</h1>
        {data.length > 0 && (
          <div className="flex gap-2">
            <button onClick={() => printElement('valuation-content')} className="rounded-md border bg-white px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors">Print</button>
            <button onClick={exportCsv} className="rounded-md border bg-white px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 transition-colors">CSV</button>
          </div>
        )}
      </div>

      {data.length === 0 ? <EmptyState message="No inventory data" /> : (
        <div id="valuation-content" className="space-y-6">
          {data.map(loc => (
            <div key={loc.location} className="rounded-lg border bg-white p-4 shadow-sm">
              <div className="mb-3 flex items-center justify-between">
                <h3 className="font-medium text-gray-900">{loc.location}</h3>
                <div className="text-sm text-gray-500">
                  <span className="mr-4">{loc.totalItems} items</span>
                  <span className="font-semibold text-blue-700">Total: {loc.totalValue.toFixed(2)} EGP</span>
                </div>
              </div>
              {loc.items.length === 0 ? <EmptyState message="No items" /> : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead className="text-left text-gray-500 bg-gray-50">
                      <tr>
                        <th className="px-3 py-2">Product</th>
                        <th className="px-3 py-2">Variant</th>
                        <th className="px-3 py-2 text-right">Qty</th>
                        <th className="px-3 py-2 text-right">Cost Price</th>
                        <th className="px-3 py-2 text-right">Selling Price</th>
                        <th className="px-3 py-2 text-right">Value</th>
                      </tr>
                    </thead>
                    <tbody>
                      {loc.items.map(i => (
                        <tr key={i.variantId} className="border-t">
                          <td className="px-3 py-2 font-medium">{i.productName}</td>
                          <td className="px-3 py-2">{i.baseType ? `${i.baseType} ${i.sizeValue}${i.sizeUnit}` : `${i.sizeValue}${i.sizeUnit}`}</td>
                          <td className="px-3 py-2 text-right">{i.quantity}</td>
                          <td className="px-3 py-2 text-right">{i.costPrice ? `${i.costPrice.toFixed(2)} EGP` : '-'}</td>
                          <td className="px-3 py-2 text-right">{i.sellingPrice ? `${i.sellingPrice.toFixed(2)} EGP` : '-'}</td>
                          <td className={`px-3 py-2 text-right font-medium ${i.itemValue < 0 ? 'text-red-600' : 'text-green-700'}`}>{i.itemValue.toFixed(2)} EGP</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
