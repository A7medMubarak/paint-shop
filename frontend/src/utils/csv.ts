export function downloadCsv(filename: string, headers: string[], rows: (string | number | null | undefined)[][]) {
  const csvContent = [
    headers.join(','),
    ...rows.map(row => row.map(cell => {
      const val = cell?.toString() ?? '';
      return val.includes(',') || val.includes('"') ? `"${val.replace(/"/g, '""')}"` : val;
    }).join(','))
  ].join('\n');

  const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}

export function printElement(id: string) {
  const el = document.getElementById(id);
  if (!el) return;
  const win = window.open('', '_blank');
  if (!win) return;
  win.document.write(`
    <html><head><title>Print</title>
    <style>
      body { font-family: Arial, sans-serif; margin: 20px; }
      table { width: 100%; border-collapse: collapse; margin: 10px 0; }
      th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }
      th { background: #f5f5f5; }
      h1 { color: #333; }
    </style></head><body>
    ${el.innerHTML}
    <script>window.print();window.close();</script>
    </body></html>
  `);
  win.document.close();
}
