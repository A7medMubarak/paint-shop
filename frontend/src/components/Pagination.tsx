interface Props {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (size: number) => void;
}

export function Pagination({ currentPage, totalPages, totalItems, pageSize, onPageChange, onPageSizeChange }: Props) {
  if (totalPages <= 1) return null;

  const pages: number[] = [];
  const start = Math.max(1, Math.min(currentPage - 2, totalPages - 4));
  const end = Math.min(totalPages, start + 4);
  for (let i = start; i <= end; i++) pages.push(i);

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 pt-4">
      <div className="flex items-center gap-2 text-sm text-gray-500">
        <span>{totalItems} items</span>
        {onPageSizeChange && (
          <select value={pageSize} onChange={e => onPageSizeChange(+e.target.value)} className="rounded border px-2 py-1 text-xs">
            {[10, 20, 50, 100].map(s => <option key={s} value={s}>{s}</option>)}
          </select>
        )}
      </div>
      <div className="flex items-center gap-1">
        <button disabled={currentPage === 1} onClick={() => onPageChange(currentPage - 1)} className="rounded border px-2 py-1 text-sm disabled:opacity-30 hover:bg-gray-100">Prev</button>
        {start > 1 && <span className="px-1 text-gray-400">...</span>}
        {pages.map(p => (
          <button key={p} onClick={() => onPageChange(p)} className={`rounded px-2 py-1 text-sm ${p === currentPage ? 'bg-blue-600 text-white' : 'hover:bg-gray-100'}`}>{p}</button>
        ))}
        {end < totalPages && <span className="px-1 text-gray-400">...</span>}
        <button disabled={currentPage === totalPages} onClick={() => onPageChange(currentPage + 1)} className="rounded border px-2 py-1 text-sm disabled:opacity-30 hover:bg-gray-100">Next</button>
      </div>
    </div>
  );
}
