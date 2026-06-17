interface Props {
  message: string;
  onRetry?: () => void;
}

export function ErrorMessage({ message, onRetry }: Props) {
  return (
    <div className="rounded-lg bg-red-50 p-4 text-center">
      <p className="text-red-600">{message}</p>
      {onRetry && (
        <button onClick={onRetry} className="mt-2 text-sm font-medium text-red-700 underline hover:text-red-800">
          Try again
        </button>
      )}
    </div>
  );
}
