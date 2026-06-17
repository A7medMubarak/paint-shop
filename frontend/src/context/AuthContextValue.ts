import { createContext } from 'react';

export interface AuthState {
  token: string | null;
  role: string | null;
  username: string | null;
  isOwner: boolean;
  isAuthenticated: boolean;
  login: (data: { username: string; password: string }) => Promise<{ token: string; role: string; username: string }>;
  logout: () => void;
}

export const AuthContext = createContext<AuthState | null>(null);
