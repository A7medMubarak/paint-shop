interface Props {
  message?: string;
}

export function EmptyState({ message = 'No data found' }: Props) {
  return (
    <div className="py-12 text-center text-gray-500">
      <p>{message}</p>
    </div>
  );
}
