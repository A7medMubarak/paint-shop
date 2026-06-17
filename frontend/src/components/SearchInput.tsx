import { useState } from 'react';

interface Props {
  placeholder?: string;
  onSearch: (query: string) => void;
  debounceMs?: number;
}

export function SearchInput({ placeholder = 'Search...', onSearch, debounceMs = 300 }: Props) {
  const [timer, setTimer] = useState<ReturnType<typeof setTimeout> | null>(null);

  const handleChange = (value: string) => {
    if (timer) clearTimeout(timer);
    const t = setTimeout(() => onSearch(value), debounceMs);
    setTimer(t);
  };

  return (
    <input
      type="text"
      placeholder={placeholder}
      onChange={(e) => handleChange(e.target.value)}
      className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm focus:border-blue-500 focus:outline-none"
    />
  );
}
