interface Props {
  status: string;
}

export function StatusBadge({ status }: Props) {
  const colors: Record<string, string> = {
    Active: 'bg-green-100 text-green-800',
    Cancelled: 'bg-red-100 text-red-800',
  };

  return (
    <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${colors[status] ?? 'bg-gray-100 text-gray-800'}`}>
      {status}
    </span>
  );
}
