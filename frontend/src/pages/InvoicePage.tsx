import { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../services/api';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorMessage } from '../components/ErrorMessage';

export default function InvoicePage() {
  const { id } = useParams();
  const [html, setHtml] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const iframeRef = useRef<HTMLIFrameElement>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const text = await api.get<string>(`/sales/${id}/invoice`);
        setHtml(text);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load invoice');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id]);

  const handlePrint = () => {
    if (iframeRef.current) {
      const win = iframeRef.current.contentWindow;
      win?.focus();
      win?.print();
    }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;
  if (!html) return null;

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <button onClick={handlePrint} className="rounded-md bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">Print Invoice</button>
      </div>
      <iframe ref={iframeRef} srcDoc={html} className="w-full rounded-lg border bg-white" style={{ height: '80vh' }} title="Invoice" />
    </div>
  );
}
